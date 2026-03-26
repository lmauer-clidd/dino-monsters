// ============================================================
// Dino Monsters -- Audio Manager
// Procedural audio: generates all music & SFX from code using
// sine/square wave oscillators. No external audio assets needed.
// ============================================================

using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // --- Audio sources ---
    private AudioSource musicSource;
    private AudioSource sfxSource;

    // --- Settings ---
    private const int SAMPLE_RATE = 44100;
    private const float MASTER_VOLUME = 0.5f;
    private const float MUSIC_VOLUME = 0.35f;
    private const float SFX_VOLUME = 0.6f;

    // --- Note frequencies (Hz) ---
    private const float C3 = 130.81f;
    private const float D3 = 146.83f;
    private const float E3 = 164.81f;
    private const float F3 = 174.61f;
    private const float G3 = 196.00f;
    private const float A3 = 220.00f;
    private const float B3 = 246.94f;
    private const float C4 = 261.63f;
    private const float D4 = 293.66f;
    private const float E4 = 329.63f;
    private const float F4 = 349.23f;
    private const float G4 = 392.00f;
    private const float A4 = 440.00f;
    private const float B4 = 493.88f;
    private const float C5 = 523.25f;
    private const float D5 = 587.33f;
    private const float E5 = 659.25f;
    private const float F5 = 698.46f;
    private const float G5 = 783.99f;
    private const float A5 = 880.00f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = MUSIC_VOLUME * MASTER_VOLUME;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.volume = SFX_VOLUME * MASTER_VOLUME;
        sfxSource.playOnAwake = false;
    }

    // ===============================================================
    // MUSIC — Procedural looping melodies
    // ===============================================================

    public void PlayTitleMusic()
    {
        // C major, slow majestic arpeggios
        float bpm = 80f;
        float beatDur = 60f / bpm;
        var notes = new (float freq, float dur, float vol)[]
        {
            (C4, beatDur * 2, 0.3f), (E4, beatDur * 2, 0.25f),
            (G4, beatDur * 2, 0.25f), (C5, beatDur * 3, 0.3f),
            (0, beatDur, 0f), // rest
            (G4, beatDur * 1.5f, 0.25f), (E4, beatDur * 1.5f, 0.2f),
            (C4, beatDur * 2, 0.25f), (G3, beatDur * 2, 0.2f),
            (C4, beatDur * 3, 0.3f), (0, beatDur, 0f),
            // Second phrase
            (F4, beatDur * 2, 0.25f), (A4, beatDur * 2, 0.25f),
            (C5, beatDur * 2, 0.3f), (A4, beatDur * 1.5f, 0.2f),
            (F4, beatDur * 1.5f, 0.2f),
            (G4, beatDur * 2, 0.25f), (E4, beatDur * 2, 0.2f),
            (C4, beatDur * 4, 0.3f), (0, beatDur * 2, 0f),
        };
        PlayMelody(notes, true);
    }

    public void PlayTownMusic()
    {
        // F major, medium cheerful
        float bpm = 120f;
        float beatDur = 60f / bpm;
        var notes = new (float freq, float dur, float vol)[]
        {
            (F4, beatDur, 0.25f), (A4, beatDur * 0.5f, 0.2f),
            (C5, beatDur * 0.5f, 0.2f), (F5, beatDur, 0.25f),
            (C5, beatDur * 0.5f, 0.2f), (A4, beatDur * 0.5f, 0.2f),
            (F4, beatDur, 0.25f), (0, beatDur * 0.5f, 0f),
            (G4, beatDur, 0.22f), (B4, beatDur * 0.5f, 0.18f),
            (D5, beatDur, 0.22f), (B4, beatDur * 0.5f, 0.18f),
            (G4, beatDur, 0.22f), (0, beatDur * 0.5f, 0f),
            (A4, beatDur, 0.25f), (C5, beatDur * 0.5f, 0.2f),
            (F5, beatDur, 0.28f), (E5, beatDur * 0.5f, 0.2f),
            (D5, beatDur * 0.5f, 0.18f), (C5, beatDur, 0.22f),
            (A4, beatDur * 0.5f, 0.18f), (F4, beatDur * 1.5f, 0.25f),
            (0, beatDur, 0f),
        };
        PlayMelody(notes, true);
    }

    public void PlayRouteMusic()
    {
        // G major, upbeat walking
        float bpm = 132f;
        float beatDur = 60f / bpm;
        var notes = new (float freq, float dur, float vol)[]
        {
            (G4, beatDur * 0.5f, 0.22f), (A4, beatDur * 0.5f, 0.18f),
            (B4, beatDur, 0.25f), (D5, beatDur * 0.5f, 0.2f),
            (B4, beatDur * 0.5f, 0.18f), (G4, beatDur, 0.22f),
            (A4, beatDur * 0.5f, 0.18f), (B4, beatDur * 0.5f, 0.2f),
            (C5, beatDur, 0.25f), (E5, beatDur * 0.5f, 0.22f),
            (D5, beatDur * 0.5f, 0.2f), (B4, beatDur, 0.22f),
            (0, beatDur * 0.25f, 0f),
            (G4, beatDur * 0.5f, 0.22f), (B4, beatDur * 0.5f, 0.2f),
            (D5, beatDur, 0.25f), (C5, beatDur * 0.5f, 0.2f),
            (A4, beatDur, 0.22f), (G4, beatDur * 1.5f, 0.25f),
            (0, beatDur * 0.5f, 0f),
        };
        PlayMelody(notes, true);
    }

    public void PlayBattleMusic()
    {
        // A minor, fast intense
        float bpm = 160f;
        float beatDur = 60f / bpm;
        var notes = new (float freq, float dur, float vol)[]
        {
            (A3, beatDur * 0.5f, 0.3f), (A3, beatDur * 0.25f, 0.2f),
            (C4, beatDur * 0.5f, 0.28f), (E4, beatDur * 0.5f, 0.25f),
            (A4, beatDur * 0.5f, 0.3f), (E4, beatDur * 0.25f, 0.2f),
            (C4, beatDur * 0.5f, 0.25f), (A3, beatDur * 0.5f, 0.3f),
            (0, beatDur * 0.25f, 0f),
            (G3, beatDur * 0.5f, 0.28f), (B3, beatDur * 0.5f, 0.25f),
            (D4, beatDur * 0.5f, 0.28f), (G4, beatDur * 0.5f, 0.3f),
            (D4, beatDur * 0.25f, 0.2f), (B3, beatDur * 0.5f, 0.25f),
            (G3, beatDur * 0.5f, 0.28f),
            (A3, beatDur * 0.5f, 0.3f), (C4, beatDur * 0.5f, 0.28f),
            (E4, beatDur * 0.75f, 0.3f), (D4, beatDur * 0.5f, 0.25f),
            (C4, beatDur * 0.5f, 0.25f), (A3, beatDur, 0.3f),
            (0, beatDur * 0.25f, 0f),
        };
        PlayMelody(notes, true, true); // use square wave for battle
    }

    public void PlayVictoryJingle()
    {
        StopMusic();
        // C major fanfare, ~3 seconds, does not loop
        float bpm = 150f;
        float beatDur = 60f / bpm;
        var notes = new (float freq, float dur, float vol)[]
        {
            (C4, beatDur * 0.5f, 0.3f), (E4, beatDur * 0.5f, 0.28f),
            (G4, beatDur * 0.5f, 0.28f), (C5, beatDur, 0.35f),
            (0, beatDur * 0.25f, 0f),
            (G4, beatDur * 0.5f, 0.25f), (C5, beatDur * 0.5f, 0.3f),
            (E5, beatDur, 0.35f), (C5, beatDur * 1.5f, 0.3f),
            (E5, beatDur * 0.5f, 0.3f), (G5, beatDur * 2, 0.4f),
        };
        PlayMelody(notes, false); // no loop
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }

    // ===============================================================
    // SFX — Short procedural sound effects
    // ===============================================================

    public void PlayMenuSelect()
    {
        // Quick rising chirp 440→880Hz, 50ms
        var clip = GenerateSweep(440f, 880f, 0.05f, 0.4f);
        PlaySFX(clip);
    }

    public void PlayMenuMove()
    {
        // Soft tick 600Hz, 30ms
        var clip = GenerateTone(600f, 0.03f, 0.25f, WaveType.Square);
        PlaySFX(clip);
    }

    public void PlayHit()
    {
        // White noise burst + low thud, 100ms
        int samples = (int)(SAMPLE_RATE * 0.1f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = 1f - t; // linear decay
            // Noise component
            float noise = UnityEngine.Random.Range(-1f, 1f) * 0.5f * envelope;
            // Low thud (sine at 80Hz)
            float thud = Mathf.Sin(2f * Mathf.PI * 80f * i / SAMPLE_RATE) * 0.5f * envelope;
            data[i] = (noise + thud) * 0.5f;
        }
        var clip = AudioClip.Create("Hit", samples, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        PlaySFX(clip);
    }

    public void PlaySuperEffective()
    {
        // High-pitched rising sweep, 200ms
        var clip = GenerateSweep(400f, 1200f, 0.2f, 0.45f);
        PlaySFX(clip);
    }

    public void PlayCritical()
    {
        // Sharp crack + rising tone
        int samples = (int)(SAMPLE_RATE * 0.15f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Pow(1f - t, 2f);
            float freq = Mathf.Lerp(300f, 900f, t);
            float sine = Mathf.Sin(2f * Mathf.PI * freq * i / SAMPLE_RATE);
            float noise = (i < samples / 4) ? UnityEngine.Random.Range(-1f, 1f) * 0.3f : 0f;
            data[i] = (sine * 0.5f + noise) * envelope;
        }
        var clip = AudioClip.Create("Critical", samples, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        PlaySFX(clip);
    }

    public void PlayFaint()
    {
        // Descending tone 440→110Hz, 500ms
        var clip = GenerateSweep(440f, 110f, 0.5f, 0.4f);
        PlaySFX(clip);
    }

    public void PlayLevelUp()
    {
        // Ascending arpeggio C-E-G-C, 400ms total
        float noteDur = 0.1f;
        float[] freqs = { C5, E5, G5, C5 * 2f };
        int totalSamples = (int)(SAMPLE_RATE * noteDur * freqs.Length);
        float[] data = new float[totalSamples];
        int samplesPerNote = (int)(SAMPLE_RATE * noteDur);

        for (int n = 0; n < freqs.Length; n++)
        {
            for (int i = 0; i < samplesPerNote; i++)
            {
                int idx = n * samplesPerNote + i;
                if (idx >= totalSamples) break;
                float t = (float)i / samplesPerNote;
                float envelope = 1f - t * 0.5f; // gentle decay per note
                float sine = Mathf.Sin(2f * Mathf.PI * freqs[n] * i / SAMPLE_RATE);
                data[idx] = sine * 0.4f * envelope;
            }
        }
        var clip = AudioClip.Create("LevelUp", totalSamples, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        PlaySFX(clip);
    }

    public void PlayCapture()
    {
        // 3 clicking sounds + success jingle
        float clickDur = 0.06f;
        float pauseDur = 0.15f;
        float jingleDur = 0.3f;
        float totalDur = 3 * (clickDur + pauseDur) + jingleDur;
        int totalSamples = (int)(SAMPLE_RATE * totalDur);
        float[] data = new float[totalSamples];

        // 3 clicks
        for (int c = 0; c < 3; c++)
        {
            float clickStart = c * (clickDur + pauseDur);
            int startIdx = (int)(clickStart * SAMPLE_RATE);
            int clickSamples = (int)(clickDur * SAMPLE_RATE);
            for (int i = 0; i < clickSamples; i++)
            {
                int idx = startIdx + i;
                if (idx >= totalSamples) break;
                float t = (float)i / clickSamples;
                float envelope = 1f - t;
                data[idx] = Mathf.Sin(2f * Mathf.PI * 800f * i / SAMPLE_RATE) * 0.35f * envelope;
            }
        }

        // Success jingle after clicks
        float jingleStart = 3 * (clickDur + pauseDur);
        int jStartIdx = (int)(jingleStart * SAMPLE_RATE);
        int jSamples = (int)(jingleDur * SAMPLE_RATE);
        float[] jingleFreqs = { C5, E5, G5 };
        int perNote = jSamples / jingleFreqs.Length;
        for (int n = 0; n < jingleFreqs.Length; n++)
        {
            for (int i = 0; i < perNote; i++)
            {
                int idx = jStartIdx + n * perNote + i;
                if (idx >= totalSamples) break;
                float t = (float)i / perNote;
                float envelope = 1f - t * 0.3f;
                data[idx] = Mathf.Sin(2f * Mathf.PI * jingleFreqs[n] * i / SAMPLE_RATE) * 0.4f * envelope;
            }
        }

        var clip = AudioClip.Create("Capture", totalSamples, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        PlaySFX(clip);
    }

    public void PlayHeal()
    {
        // Ascending chime C5-E5-G5, 300ms
        float noteDur = 0.1f;
        float[] freqs = { C5, E5, G5 };
        int totalSamples = (int)(SAMPLE_RATE * noteDur * freqs.Length);
        float[] data = new float[totalSamples];
        int samplesPerNote = (int)(SAMPLE_RATE * noteDur);

        for (int n = 0; n < freqs.Length; n++)
        {
            for (int i = 0; i < samplesPerNote; i++)
            {
                int idx = n * samplesPerNote + i;
                if (idx >= totalSamples) break;
                float t = (float)i / samplesPerNote;
                float envelope = 1f - t * 0.4f;
                // Mix sine + harmonic for chime quality
                float sine = Mathf.Sin(2f * Mathf.PI * freqs[n] * i / SAMPLE_RATE);
                float harmonic = Mathf.Sin(2f * Mathf.PI * freqs[n] * 2f * i / SAMPLE_RATE) * 0.3f;
                data[idx] = (sine + harmonic) * 0.35f * envelope;
            }
        }
        var clip = AudioClip.Create("Heal", totalSamples, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        PlaySFX(clip);
    }

    // ===============================================================
    // Internal — Melody player
    // ===============================================================

    private void PlayMelody((float freq, float dur, float vol)[] notes, bool loop, bool useSquare = false)
    {
        // Calculate total duration
        float totalDuration = 0f;
        foreach (var note in notes)
            totalDuration += note.dur;

        int totalSamples = (int)(SAMPLE_RATE * totalDuration);
        if (totalSamples <= 0) return;

        float[] data = new float[totalSamples];
        int currentSample = 0;

        foreach (var note in notes)
        {
            int noteSamples = (int)(SAMPLE_RATE * note.dur);
            for (int i = 0; i < noteSamples; i++)
            {
                int idx = currentSample + i;
                if (idx >= totalSamples) break;

                if (note.freq <= 0f)
                {
                    data[idx] = 0f; // rest
                    continue;
                }

                float t = (float)i / noteSamples;
                // ADSR-like envelope: quick attack, sustain, fade at end
                float envelope;
                if (t < 0.05f)
                    envelope = t / 0.05f; // attack
                else if (t < 0.8f)
                    envelope = 1f; // sustain
                else
                    envelope = (1f - t) / 0.2f; // release

                float phase = 2f * Mathf.PI * note.freq * i / SAMPLE_RATE;
                float sample;

                if (useSquare)
                {
                    // Square wave with slight softening
                    float sine = Mathf.Sin(phase);
                    sample = sine > 0 ? 0.7f : -0.7f;
                    // Add a bit of sine to soften
                    sample = sample * 0.6f + Mathf.Sin(phase) * 0.4f;
                }
                else
                {
                    // Sine + slight second harmonic for richer sound
                    sample = Mathf.Sin(phase) * 0.8f +
                             Mathf.Sin(phase * 2f) * 0.15f +
                             Mathf.Sin(phase * 3f) * 0.05f;
                }

                data[idx] = sample * note.vol * envelope;
            }
            currentSample += noteSamples;
        }

        var clip = AudioClip.Create("Music", totalSamples, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = MUSIC_VOLUME * MASTER_VOLUME;
        musicSource.Play();
    }

    // ===============================================================
    // Internal — Tone generators
    // ===============================================================

    private enum WaveType { Sine, Square }

    private AudioClip GenerateTone(float freq, float duration, float volume, WaveType wave = WaveType.Sine)
    {
        int samples = (int)(SAMPLE_RATE * duration);
        if (samples <= 0) samples = 1;
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = 1f - t; // linear decay
            float phase = 2f * Mathf.PI * freq * i / SAMPLE_RATE;
            float sample = wave == WaveType.Sine
                ? Mathf.Sin(phase)
                : (Mathf.Sin(phase) > 0 ? 1f : -1f);
            data[i] = sample * volume * envelope;
        }

        var clip = AudioClip.Create("Tone", samples, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip GenerateSweep(float startFreq, float endFreq, float duration, float volume)
    {
        int samples = (int)(SAMPLE_RATE * duration);
        if (samples <= 0) samples = 1;
        float[] data = new float[samples];

        float phase = 0f;
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = 1f - t * 0.5f; // gentle decay
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            phase += 2f * Mathf.PI * freq / SAMPLE_RATE;
            data[i] = Mathf.Sin(phase) * volume * envelope;
        }

        var clip = AudioClip.Create("Sweep", samples, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, SFX_VOLUME * MASTER_VOLUME);
        }
    }

    // ===============================================================
    // AMBIENT — Procedural ambient sounds per map
    // ===============================================================

    private AudioSource ambientSource;
    private Coroutine ambientCoroutine;
    private const float AMBIENT_VOLUME = 0.15f;

    private void EnsureAmbientSource()
    {
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.volume = AMBIENT_VOLUME * MASTER_VOLUME;
            ambientSource.playOnAwake = false;
        }
    }

    public void PlayAmbientForMap(string mapId)
    {
        StopAmbient();
        EnsureAmbientSource();

        if (string.IsNullOrEmpty(mapId)) return;

        AmbientType ambType = GetAmbientType(mapId);
        if (ambType == AmbientType.None) return;

        AudioClip clip = GenerateAmbientClip(ambType);
        if (clip != null)
        {
            ambientSource.clip = clip;
            ambientSource.loop = true;
            ambientSource.volume = AMBIENT_VOLUME * MASTER_VOLUME;
            ambientSource.Play();
        }
    }

    public void StopAmbient()
    {
        if (ambientSource != null && ambientSource.isPlaying)
        {
            ambientSource.Stop();
            ambientSource.clip = null;
        }
    }

    private enum AmbientType { None, Town, Forest, Water, Mountain, Snow, Swamp }

    private AmbientType GetAmbientType(string mapId)
    {
        switch (mapId)
        {
            // Forest routes
            case "ROUTE_1":
            case "ROUTE_2":
                return AmbientType.Forest;

            // Water routes
            case "ROUTE_3":
            case "PORT_COQUILLE":
                return AmbientType.Water;

            // Mountain routes
            case "ROUTE_4":
            case "ROUTE_5":
            case "ROUTE_9":
            case "VICTORY_ROAD":
                return AmbientType.Mountain;

            // Snow routes
            case "ROUTE_6":
            case "CRYO_CITE":
                return AmbientType.Snow;

            // Swamp routes
            case "ROUTE_8":
            case "MARAIS_NOIR":
                return AmbientType.Swamp;

            // Towns
            case "BOURG_NID":
            case "VILLE_FOUGERE":
            case "ROCHE_HAUTE":
            case "VOLCANVILLE":
            case "ELECTROPOLIS":
            case "CIEL_HAUT":
            case "PALEO_CAPITAL":
                return AmbientType.Town;

            // Route 7 — between snow and electric city, treat as mountain
            case "ROUTE_7":
                return AmbientType.Mountain;

            default:
                return AmbientType.Town;
        }
    }

    private AudioClip GenerateAmbientClip(AmbientType type)
    {
        // Generate a 4-second loopable ambient clip
        float duration = 4f;
        int samples = (int)(SAMPLE_RATE * duration);
        float[] data = new float[samples];

        switch (type)
        {
            case AmbientType.Town:
                // Soft low hum (sine at 60Hz, very quiet)
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / SAMPLE_RATE;
                    data[i] = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.08f
                            + Mathf.Sin(2f * Mathf.PI * 90f * t) * 0.04f;
                }
                break;

            case AmbientType.Forest:
                // Background hum + bird chirps (random high sine blips)
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / SAMPLE_RATE;
                    // Base: very soft noise/hum
                    data[i] = Mathf.Sin(2f * Mathf.PI * 80f * t) * 0.03f;
                }
                // Add bird chirps at random intervals
                AddBirdChirps(data, samples, duration);
                break;

            case AmbientType.Water:
                // Rhythmic wave sounds (low filtered noise)
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / SAMPLE_RATE;
                    // Wave rhythm: modulated noise
                    float waveEnvelope = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.3f * t);
                    // Low-pass approximation: blend sine with noise
                    float noise = Mathf.PerlinNoise(t * 5f, 0f) * 2f - 1f;
                    float lowSine = Mathf.Sin(2f * Mathf.PI * 50f * t);
                    data[i] = (noise * 0.15f + lowSine * 0.05f) * waveEnvelope;
                }
                break;

            case AmbientType.Mountain:
                // Wind: filtered white noise with slow modulation
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / SAMPLE_RATE;
                    float modulation = 0.4f + 0.6f * Mathf.Sin(2f * Mathf.PI * 0.15f * t);
                    // Perlin noise for smoother wind
                    float wind = Mathf.PerlinNoise(t * 3f, 1.5f) * 2f - 1f;
                    data[i] = wind * 0.2f * modulation;
                }
                break;

            case AmbientType.Snow:
                // Wind + occasional crystal chime
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / SAMPLE_RATE;
                    // Softer wind
                    float modulation = 0.3f + 0.7f * Mathf.Sin(2f * Mathf.PI * 0.12f * t);
                    float wind = Mathf.PerlinNoise(t * 2f, 2.5f) * 2f - 1f;
                    data[i] = wind * 0.12f * modulation;
                }
                // Add crystal chimes
                AddCrystalChimes(data, samples, duration);
                break;

            case AmbientType.Swamp:
                // Bubbling (random low blips) + insect buzz
                for (int i = 0; i < samples; i++)
                {
                    float t = (float)i / SAMPLE_RATE;
                    // High insect buzz (modulated high sine)
                    float buzzMod = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 6f * t);
                    float buzz = Mathf.Sin(2f * Mathf.PI * 2200f * t) * 0.03f * buzzMod;
                    // Low background
                    float low = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.04f;
                    data[i] = buzz + low;
                }
                // Add bubble blips
                AddBubbles(data, samples, duration);
                break;
        }

        var clip = AudioClip.Create("Ambient", samples, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void AddBirdChirps(float[] data, int totalSamples, float duration)
    {
        // Add 3-6 bird chirps randomly distributed in the clip
        // Use deterministic seed for consistent loop
        System.Random rng = new System.Random(42);
        int chirpCount = 3 + rng.Next(4);

        for (int c = 0; c < chirpCount; c++)
        {
            float startTime = (float)(rng.NextDouble() * (duration - 0.15f));
            int startSample = (int)(startTime * SAMPLE_RATE);
            float chirpDur = 0.05f + (float)(rng.NextDouble() * 0.1f);
            int chirpSamples = (int)(chirpDur * SAMPLE_RATE);
            float freq = 1800f + (float)(rng.NextDouble() * 1200f);

            for (int i = 0; i < chirpSamples; i++)
            {
                int idx = startSample + i;
                if (idx >= totalSamples) break;
                float t = (float)i / chirpSamples;
                float env = Mathf.Sin(Mathf.PI * t); // bell curve
                float chirpFreq = freq + t * 400f; // rising pitch
                data[idx] += Mathf.Sin(2f * Mathf.PI * chirpFreq * i / SAMPLE_RATE) * 0.15f * env;
            }
        }
    }

    private void AddCrystalChimes(float[] data, int totalSamples, float duration)
    {
        System.Random rng = new System.Random(77);
        int chimeCount = 2 + rng.Next(3);

        for (int c = 0; c < chimeCount; c++)
        {
            float startTime = (float)(rng.NextDouble() * (duration - 0.3f));
            int startSample = (int)(startTime * SAMPLE_RATE);
            float chimeDur = 0.2f + (float)(rng.NextDouble() * 0.15f);
            int chimeSamples = (int)(chimeDur * SAMPLE_RATE);
            float freq = 2000f + (float)(rng.NextDouble() * 1500f);

            for (int i = 0; i < chimeSamples; i++)
            {
                int idx = startSample + i;
                if (idx >= totalSamples) break;
                float t = (float)i / chimeSamples;
                float env = Mathf.Pow(1f - t, 2f); // fast decay
                float sine = Mathf.Sin(2f * Mathf.PI * freq * i / SAMPLE_RATE);
                float harmonic = Mathf.Sin(2f * Mathf.PI * freq * 2.5f * i / SAMPLE_RATE) * 0.3f;
                data[idx] += (sine + harmonic) * 0.12f * env;
            }
        }
    }

    private void AddBubbles(float[] data, int totalSamples, float duration)
    {
        System.Random rng = new System.Random(99);
        int bubbleCount = 5 + rng.Next(6);

        for (int c = 0; c < bubbleCount; c++)
        {
            float startTime = (float)(rng.NextDouble() * (duration - 0.1f));
            int startSample = (int)(startTime * SAMPLE_RATE);
            float blipDur = 0.03f + (float)(rng.NextDouble() * 0.05f);
            int blipSamples = (int)(blipDur * SAMPLE_RATE);
            float freq = 100f + (float)(rng.NextDouble() * 200f);

            for (int i = 0; i < blipSamples; i++)
            {
                int idx = startSample + i;
                if (idx >= totalSamples) break;
                float t = (float)i / blipSamples;
                float env = Mathf.Sin(Mathf.PI * t);
                float freqRise = freq + t * freq; // rising bubble
                data[idx] += Mathf.Sin(2f * Mathf.PI * freqRise * i / SAMPLE_RATE) * 0.18f * env;
            }
        }
    }
}
