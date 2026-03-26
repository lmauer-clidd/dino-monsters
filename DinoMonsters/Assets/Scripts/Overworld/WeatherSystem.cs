// ============================================================
// Dino Monsters -- Weather System
// Per-map weather effects using spawned primitive GameObjects.
// No ParticleSystem components — just cubes/spheres with velocity.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    public enum WeatherType { Clear, Rain, Snow, Sandstorm, Fog }

    private WeatherType currentWeather = WeatherType.Clear;
    private List<WeatherParticle> particles = new List<WeatherParticle>();
    private List<GameObject> fogPlanes = new List<GameObject>();
    private Light directionalLight;
    private Color originalLightColor;
    private float originalLightIntensity;
    private float originalFarClip;
    private Camera mainCamera;

    // Particle bounds (area around camera)
    private const float SPAWN_RADIUS = 12f;
    private const float SPAWN_HEIGHT = 8f;
    private const float KILL_Y = -1f;

    // FOV oscillation for sandstorm
    private float originalFOV;
    private float sandstormFovTimer;

    // Splash pool
    private List<SplashEffect> splashes = new List<SplashEffect>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalFOV = mainCamera.fieldOfView;
            originalFarClip = mainCamera.farClipPlane;
        }

        // Find directional light
        directionalLight = FindObjectOfType<Light>();
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalLightIntensity = directionalLight.intensity;
        }
    }

    void Update()
    {
        if (currentWeather == WeatherType.Clear) return;

        Vector3 camPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;

        // Update existing particles
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            if (p.go == null) { particles.RemoveAt(i); continue; }

            p.go.transform.position += p.velocity * Time.deltaTime;
            p.lifetime -= Time.deltaTime;

            // Snow sway
            if (currentWeather == WeatherType.Snow)
            {
                float sway = Mathf.Sin(Time.time * 2f + p.phase) * 0.5f * Time.deltaTime;
                p.go.transform.position += new Vector3(sway, 0, sway * 0.5f);
            }

            // Kill if below ground or expired or too far from camera
            bool outOfBounds = p.go.transform.position.y < KILL_Y || p.lifetime <= 0f;
            float distFromCam = Vector3.Distance(p.go.transform.position, camPos);
            if (outOfBounds || distFromCam > SPAWN_RADIUS * 2f)
            {
                // Rain splash on ground
                if (currentWeather == WeatherType.Rain && p.go.transform.position.y < 0.1f)
                {
                    SpawnSplash(new Vector3(p.go.transform.position.x, 0.02f, p.go.transform.position.z));
                }
                Destroy(p.go);
                particles.RemoveAt(i);
            }
        }

        // Update splashes
        for (int i = splashes.Count - 1; i >= 0; i--)
        {
            var s = splashes[i];
            if (s.go == null) { splashes.RemoveAt(i); continue; }
            s.lifetime -= Time.deltaTime;
            if (s.lifetime <= 0f)
            {
                Destroy(s.go);
                splashes.RemoveAt(i);
            }
            else
            {
                // Expand and fade
                float t = 1f - (s.lifetime / s.maxLifetime);
                float scale = Mathf.Lerp(0.02f, 0.12f, t);
                s.go.transform.localScale = new Vector3(scale, 0.005f, scale);
                var r = s.go.GetComponent<Renderer>();
                if (r != null)
                {
                    Color c = r.material.color;
                    c.a = 1f - t;
                    r.material.color = c;
                }
            }
        }

        // Spawn new particles to maintain count
        int targetCount = GetTargetParticleCount();
        int toSpawn = targetCount - particles.Count;
        for (int i = 0; i < toSpawn; i++)
        {
            SpawnParticle(camPos);
        }

        // Sandstorm FOV oscillation
        if (currentWeather == WeatherType.Sandstorm && mainCamera != null)
        {
            sandstormFovTimer += Time.deltaTime;
            mainCamera.fieldOfView = originalFOV + Mathf.Sin(sandstormFovTimer * 1.5f) * 1.5f;
        }
    }

    // ===============================================================
    // Public API
    // ===============================================================

    public void SetWeather(WeatherType type)
    {
        if (type == currentWeather) return;

        // Clean up previous weather
        ClearWeatherEffects();

        currentWeather = type;

        // Apply lighting changes
        ApplyLighting(type);

        // Fog planes
        if (type == WeatherType.Fog)
        {
            SetupFog();
        }
    }

    public void SetWeatherForMap(string mapId)
    {
        if (string.IsNullOrEmpty(mapId))
        {
            SetWeather(WeatherType.Clear);
            return;
        }

        switch (mapId)
        {
            // Rain maps
            case "ROUTE_3":
            case "PORT_COQUILLE":
            case "ROUTE_8":
            case "MARAIS_NOIR":
                SetWeather(WeatherType.Rain);
                break;

            // Snow maps
            case "ROUTE_6":
            case "CRYO_CITE":
                SetWeather(WeatherType.Snow);
                break;

            // Sandstorm maps
            case "ROUTE_5":
            case "VOLCANVILLE":
                SetWeather(WeatherType.Sandstorm);
                break;

            // Fog maps
            case "ROUTE_9":
            case "VICTORY_ROAD":
                SetWeather(WeatherType.Fog);
                break;

            // Clear for everything else
            default:
                SetWeather(WeatherType.Clear);
                break;
        }
    }

    public WeatherType GetCurrentWeather()
    {
        return currentWeather;
    }

    // ===============================================================
    // Particle Spawning
    // ===============================================================

    private void SpawnParticle(Vector3 camPos)
    {
        GameObject go = null;
        Vector3 velocity = Vector3.zero;
        float lifetime = 5f;

        // Random position around camera
        float rx = Random.Range(-SPAWN_RADIUS, SPAWN_RADIUS);
        float rz = Random.Range(-SPAWN_RADIUS, SPAWN_RADIUS);

        switch (currentWeather)
        {
            case WeatherType.Rain:
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = camPos + new Vector3(rx, SPAWN_HEIGHT + Random.Range(0f, 3f), rz);
                go.transform.localScale = new Vector3(0.02f, 0.08f, 0.02f);
                go.transform.rotation = Quaternion.Euler(0, 0, 10f); // slight angle
                SetColor(go, new Color(0.4f, 0.5f, 0.9f, 0.7f));
                velocity = new Vector3(-0.5f, -8f, 0f);
                lifetime = 3f;
                break;

            case WeatherType.Snow:
                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = camPos + new Vector3(rx, SPAWN_HEIGHT + Random.Range(0f, 2f), rz);
                float snowSize = Random.Range(0.03f, 0.06f);
                go.transform.localScale = new Vector3(snowSize, snowSize, snowSize);
                SetColor(go, new Color(1f, 1f, 1f, 0.85f));
                velocity = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-1.5f, -0.8f), Random.Range(-0.2f, 0.2f));
                lifetime = 8f;
                break;

            case WeatherType.Sandstorm:
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = camPos + new Vector3(-SPAWN_RADIUS, Random.Range(0.5f, 4f), rz);
                float sandSize = Random.Range(0.02f, 0.05f);
                go.transform.localScale = new Vector3(sandSize, sandSize, sandSize);
                go.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                SetColor(go, new Color(0.82f, 0.72f, 0.45f, 0.6f));
                velocity = new Vector3(Random.Range(6f, 10f), Random.Range(-0.5f, 0.5f), Random.Range(-1f, 1f));
                lifetime = 3f;
                break;

            case WeatherType.Fog:
                // Fog uses planes, not particles per frame
                return;
        }

        if (go != null)
        {
            go.name = "WeatherParticle";
            go.transform.SetParent(transform);
            // Remove collider
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            var p = new WeatherParticle
            {
                go = go,
                velocity = velocity,
                lifetime = lifetime,
                phase = Random.Range(0f, Mathf.PI * 2f)
            };
            particles.Add(p);
        }
    }

    private void SpawnSplash(Vector3 position)
    {
        if (splashes.Count > 30) return; // limit splash count

        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "RainSplash";
        go.transform.SetParent(transform);
        go.transform.position = position;
        go.transform.localScale = new Vector3(0.02f, 0.005f, 0.02f);

        var col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        // Transparent blue-white
        SetColor(go, new Color(0.7f, 0.8f, 1f, 0.6f));

        float maxLife = 0.3f;
        splashes.Add(new SplashEffect { go = go, lifetime = maxLife, maxLifetime = maxLife });
    }

    // ===============================================================
    // Fog Setup
    // ===============================================================

    private void SetupFog()
    {
        if (mainCamera == null) return;

        // Reduce far clip for fog
        mainCamera.farClipPlane = 20f;

        Vector3 camPos = mainCamera.transform.position;

        // Create several semi-transparent planes at various distances
        for (int i = 0; i < 6; i++)
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.name = "FogPlane";
            plane.transform.SetParent(transform);

            float dist = 5f + i * 3f;
            float yOff = Random.Range(-0.5f, 2f);
            plane.transform.position = camPos + mainCamera.transform.forward * dist + new Vector3(0, yOff, 0);
            plane.transform.rotation = mainCamera.transform.rotation;
            plane.transform.localScale = new Vector3(25f, 10f, 1f);

            var col = plane.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            var r = plane.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Standard"));
            r.material.SetFloat("_Mode", 3); // Transparent
            r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            r.material.SetInt("_ZWrite", 0);
            r.material.DisableKeyword("_ALPHATEST_ON");
            r.material.EnableKeyword("_ALPHABLEND_ON");
            r.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            r.material.renderQueue = 3000;
            float alpha = Mathf.Lerp(0.05f, 0.25f, (float)i / 5f);
            r.material.color = new Color(0.85f, 0.88f, 0.9f, alpha);

            fogPlanes.Add(plane);
        }
    }

    // ===============================================================
    // Lighting
    // ===============================================================

    private void ApplyLighting(WeatherType type)
    {
        if (directionalLight == null) return;

        switch (type)
        {
            case WeatherType.Clear:
                directionalLight.color = originalLightColor;
                directionalLight.intensity = originalLightIntensity;
                break;

            case WeatherType.Rain:
                // Subtle darkening
                directionalLight.color = Color.Lerp(originalLightColor, new Color(0.5f, 0.5f, 0.6f), 0.3f);
                directionalLight.intensity = originalLightIntensity * 0.75f;
                break;

            case WeatherType.Snow:
                // Lighter, blue-white tint
                directionalLight.color = Color.Lerp(originalLightColor, new Color(0.8f, 0.85f, 1f), 0.3f);
                directionalLight.intensity = originalLightIntensity * 1.1f;
                break;

            case WeatherType.Sandstorm:
                // Warm yellowish tint, slightly dimmed
                directionalLight.color = Color.Lerp(originalLightColor, new Color(0.9f, 0.8f, 0.5f), 0.25f);
                directionalLight.intensity = originalLightIntensity * 0.8f;
                break;

            case WeatherType.Fog:
                // Greyish, flat lighting
                directionalLight.color = Color.Lerp(originalLightColor, new Color(0.7f, 0.75f, 0.8f), 0.4f);
                directionalLight.intensity = originalLightIntensity * 0.65f;
                break;
        }
    }

    // ===============================================================
    // Cleanup
    // ===============================================================

    private void ClearWeatherEffects()
    {
        // Destroy all particles
        foreach (var p in particles)
        {
            if (p.go != null) Destroy(p.go);
        }
        particles.Clear();

        // Destroy splashes
        foreach (var s in splashes)
        {
            if (s.go != null) Destroy(s.go);
        }
        splashes.Clear();

        // Destroy fog planes
        foreach (var f in fogPlanes)
        {
            if (f != null) Destroy(f);
        }
        fogPlanes.Clear();

        // Restore lighting
        if (directionalLight != null)
        {
            directionalLight.color = originalLightColor;
            directionalLight.intensity = originalLightIntensity;
        }

        // Restore camera
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = originalFOV;
            mainCamera.farClipPlane = originalFarClip;
        }

        sandstormFovTimer = 0f;
    }

    // ===============================================================
    // Helpers
    // ===============================================================

    private int GetTargetParticleCount()
    {
        switch (currentWeather)
        {
            case WeatherType.Rain: return 75;
            case WeatherType.Snow: return 45;
            case WeatherType.Sandstorm: return 60;
            default: return 0;
        }
    }

    private void SetColor(GameObject go, Color color)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        r.material = new Material(Shader.Find("Standard"));

        if (color.a < 1f)
        {
            // Enable transparency
            r.material.SetFloat("_Mode", 3);
            r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            r.material.SetInt("_ZWrite", 0);
            r.material.DisableKeyword("_ALPHATEST_ON");
            r.material.EnableKeyword("_ALPHABLEND_ON");
            r.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            r.material.renderQueue = 3000;
        }

        r.material.color = color;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        ClearWeatherEffects();
    }

    // ===============================================================
    // Data classes
    // ===============================================================

    private class WeatherParticle
    {
        public GameObject go;
        public Vector3 velocity;
        public float lifetime;
        public float phase;
    }

    private class SplashEffect
    {
        public GameObject go;
        public float lifetime;
        public float maxLifetime;
    }
}
