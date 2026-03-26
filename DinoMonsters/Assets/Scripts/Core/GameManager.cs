using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameScene { Title, Overworld, Battle, DinoCenter, Shop, Evolution, Dinodex }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(GameScene scene, object data = null)
    {
        // Store transition data
        SceneTransitionData.Instance = data;
        SceneManager.LoadScene(scene.ToString());
    }

    public void StartNewGame(string playerName, int starterSpeciesId)
    {
        GameState.Instance.Init(playerName, starterSpeciesId);
        LoadScene(GameScene.Overworld);
    }

    public void StartBattle(BattleSetupData setup, bool skipFade = false)
    {
        StartCoroutine(StartBattleWithFade(setup, skipFade));
    }

    private IEnumerator StartBattleWithFade(BattleSetupData setup, bool skipFade)
    {
        SceneTransitionData.Instance = setup;

        // Fade out before loading battle scene (skip if caller already faded, e.g. wild encounter flash)
        if (!skipFade && ScreenFade.Instance != null)
        {
            yield return ScreenFade.Instance.FadeOutCoroutine(0.5f);
        }

        SceneManager.LoadScene("Battle");
    }

    // ===============================================================
    // Gym Battle
    // ===============================================================

    public void StartGymBattle(int gymId)
    {
        var setup = GetGymLeaderData(gymId);
        if (setup == null)
        {
            Debug.LogError($"[GameManager] No gym leader data for gymId {gymId}");
            return;
        }

        // Set return data to current position
        setup.returnScene = GameState.Instance.CurrentMapId;
        setup.returnPosition = new Vector2(GameState.Instance.PlayerX, GameState.Instance.PlayerY);

        StartBattle(setup);
    }

    private BattleSetupData GetGymLeaderData(int gymId)
    {
        var setup = new BattleSetupData
        {
            isWild = false,
            isGymLeader = true,
            gymId = gymId,
            trainerId = $"GYM_LEADER_{gymId}"
        };

        switch (gymId)
        {
            case 0: // Flora — FLORA
                setup.trainerName = "FLORA";
                setup.trainerParty = new List<TrainerDinoEntry>
                {
                    new TrainerDinoEntry { speciesId = 19, level = 12 },
                    new TrainerDinoEntry { speciesId = 20, level = 14 }
                };
                setup.badgeName = "Badge Feuille";
                break;

            case 1: // Water — MARIN
                setup.trainerName = "MARIN";
                setup.trainerParty = new List<TrainerDinoEntry>
                {
                    new TrainerDinoEntry { speciesId = 4, level = 16 },
                    new TrainerDinoEntry { speciesId = 12, level = 15 },
                    new TrainerDinoEntry { speciesId = 13, level = 18 }
                };
                setup.badgeName = "Badge Vague";
                break;

            case 2: // Fossil — PETRA
                setup.trainerName = "PETRA";
                setup.trainerParty = new List<TrainerDinoEntry>
                {
                    new TrainerDinoEntry { speciesId = 15, level = 20 },
                    new TrainerDinoEntry { speciesId = 16, level = 19 },
                    new TrainerDinoEntry { speciesId = 17, level = 22 }
                };
                setup.badgeName = "Badge Fossile";
                break;

            case 3: // Fire — BRAZIER
                setup.trainerName = "BRAZIER";
                setup.trainerParty = new List<TrainerDinoEntry>
                {
                    new TrainerDinoEntry { speciesId = 1, level = 24 },
                    new TrainerDinoEntry { speciesId = 9, level = 23 },
                    new TrainerDinoEntry { speciesId = 14, level = 26 }
                };
                setup.badgeName = "Badge Flamme";
                break;

            case 4: // Ice — GIVRALIA
                setup.trainerName = "GIVRALIA";
                setup.trainerParty = new List<TrainerDinoEntry>
                {
                    new TrainerDinoEntry { speciesId = 11, level = 28 },
                    new TrainerDinoEntry { speciesId = 21, level = 27 },
                    new TrainerDinoEntry { speciesId = 22, level = 30 }
                };
                setup.badgeName = "Badge Givre";
                break;

            case 5: // Electric — VOLTAIRE
                setup.trainerName = "VOLTAIRE";
                setup.trainerParty = new List<TrainerDinoEntry>
                {
                    new TrainerDinoEntry { speciesId = 10, level = 32 },
                    new TrainerDinoEntry { speciesId = 23, level = 31 },
                    new TrainerDinoEntry { speciesId = 24, level = 34 }
                };
                setup.badgeName = "Badge Eclair";
                break;

            case 6: // Venom — TOXICA
                setup.trainerName = "TOXICA";
                setup.trainerParty = new List<TrainerDinoEntry>
                {
                    new TrainerDinoEntry { speciesId = 17, level = 36 },
                    new TrainerDinoEntry { speciesId = 25, level = 35 },
                    new TrainerDinoEntry { speciesId = 26, level = 38 }
                };
                setup.badgeName = "Badge Toxique";
                break;

            case 7: // Air — CELESTA
                setup.trainerName = "CELESTA";
                setup.trainerParty = new List<TrainerDinoEntry>
                {
                    new TrainerDinoEntry { speciesId = 5, level = 40 },
                    new TrainerDinoEntry { speciesId = 27, level = 39 },
                    new TrainerDinoEntry { speciesId = 28, level = 42 }
                };
                setup.badgeName = "Badge Zephyr";
                break;

            default:
                return null;
        }

        // Set the first dino as the active enemy
        if (setup.trainerParty != null && setup.trainerParty.Count > 0)
        {
            setup.enemySpeciesId = setup.trainerParty[0].speciesId;
            setup.enemyLevel = setup.trainerParty[0].level;
        }

        return setup;
    }
}

// Generic scene transition data holder
public class SceneTransitionData
{
    public static object Instance { get; set; }
}

// Single entry for a trainer's dino party member
[System.Serializable]
public class TrainerDinoEntry
{
    public int speciesId;
    public int level;
}

// Battle setup data passed between scenes
[System.Serializable]
public class BattleSetupData
{
    public bool isWild;
    public int enemySpeciesId;
    public int enemyLevel;
    public string trainerName;
    public string trainerId;
    public string returnScene;
    public Vector2 returnPosition;

    // Gym leader fields
    public bool isGymLeader;
    public int gymId = -1;
    public string badgeName;
    public List<TrainerDinoEntry> trainerParty; // full party for multi-dino battles
}
