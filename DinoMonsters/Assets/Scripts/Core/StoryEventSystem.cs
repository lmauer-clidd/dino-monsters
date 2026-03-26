// ============================================================
// Dino Monsters -- Story Event System
// Manages story progression through events triggered by
// location, flags, and NPC interactions.
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryEventSystem : MonoBehaviour
{
    public static StoryEventSystem Instance { get; private set; }

    // ---------------------------------------------------------------
    // Event Data Structures
    // ---------------------------------------------------------------

    public enum TriggerType
    {
        EnterMap,
        StepOnTile,
        TalkToNpc
    }

    public enum ActionType
    {
        Dialogue,
        Choice,
        Battle,
        SetFlag,
        GiveItem,
        GiveMoney,
        MoveTo,
        FadeOut,
        FadeIn,
        Heal
    }

    [System.Serializable]
    public class EventAction
    {
        public ActionType type;
        public string speaker;
        public string text;
        public string[] choiceOptions;
        public string[] choiceFlagKeys;     // flag to set per choice
        public string trainerName;
        public string trainerId;
        public List<TrainerDinoEntry> trainerParty;
        public bool winOrLose;              // if true, event continues even on loss
        public string flagKey;
        public bool flagValue = true;
        public int itemId;
        public int itemCount = 1;
        public int moneyAmount;
        public string npcName;
        public int targetX;
        public int targetY;
    }

    [System.Serializable]
    public class StoryEvent
    {
        public string id;
        public string mapId;
        public TriggerType triggerType;
        public int triggerX = -1;           // for StepOnTile
        public int triggerY = -1;
        public string triggerNpcName;       // for TalkToNpc
        public string requiredFlag;         // must be set to trigger
        public string blockingFlag;         // if set, event won't trigger
        public int requiredBadgeCount = -1; // minimum badges needed
        public List<EventAction> actions;
    }

    // ---------------------------------------------------------------
    // State
    // ---------------------------------------------------------------

    private List<StoryEvent> allEvents = new List<StoryEvent>();
    private bool isRunningEvent = false;
    private Coroutine eventCoroutine;

    // ---------------------------------------------------------------
    // Singleton
    // ---------------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        RegisterAllEvents();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ---------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------

    /// <summary>
    /// Check and trigger events for the current map position.
    /// Called on player step and map entry.
    /// </summary>
    public void CheckEvents(string mapId, int playerX, int playerY)
    {
        if (isRunningEvent) return;

        foreach (var evt in allEvents)
        {
            if (evt.mapId != mapId) continue;
            if (!CanTrigger(evt, playerX, playerY)) continue;

            // Found a triggerable event
            StartEvent(evt);
            return; // Only one event at a time
        }
    }

    /// <summary>
    /// Check events triggered by entering a map (enter_map type).
    /// </summary>
    public void CheckMapEntryEvents(string mapId)
    {
        if (isRunningEvent) return;

        foreach (var evt in allEvents)
        {
            if (evt.mapId != mapId) continue;
            if (evt.triggerType != TriggerType.EnterMap) continue;
            if (!CanTriggerBase(evt)) continue;

            StartEvent(evt);
            return;
        }
    }

    /// <summary>
    /// Check if an NPC interaction triggers a story event.
    /// Returns true if an event was triggered (caller should skip normal dialogue).
    /// </summary>
    public bool CheckNpcEvent(string mapId, string npcName)
    {
        if (isRunningEvent) return false;

        foreach (var evt in allEvents)
        {
            if (evt.mapId != mapId) continue;
            if (evt.triggerType != TriggerType.TalkToNpc) continue;
            if (evt.triggerNpcName != npcName) continue;
            if (!CanTriggerBase(evt)) continue;

            StartEvent(evt);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if there is an event at a given tile (for visual indicators).
    /// </summary>
    public bool HasEventAt(string mapId, int x, int y)
    {
        foreach (var evt in allEvents)
        {
            if (evt.mapId != mapId) continue;
            if (evt.triggerType != TriggerType.StepOnTile) continue;
            if (evt.triggerX != x || evt.triggerY != y) continue;
            if (!CanTriggerBase(evt)) continue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Is a story event currently running?
    /// </summary>
    public bool IsEventRunning()
    {
        return isRunningEvent;
    }

    // ---------------------------------------------------------------
    // Trigger Checks
    // ---------------------------------------------------------------

    private bool CanTriggerBase(StoryEvent evt)
    {
        var gs = GameState.Instance;

        // Check blocking flag
        if (!string.IsNullOrEmpty(evt.blockingFlag) && gs.HasFlag(evt.blockingFlag))
            return false;

        // Check required flag
        if (!string.IsNullOrEmpty(evt.requiredFlag) && !gs.HasFlag(evt.requiredFlag))
            return false;

        // Check badge count
        if (evt.requiredBadgeCount >= 0 && gs.GetBadgeCount() < evt.requiredBadgeCount)
            return false;

        return true;
    }

    private bool CanTrigger(StoryEvent evt, int playerX, int playerY)
    {
        if (!CanTriggerBase(evt)) return false;

        switch (evt.triggerType)
        {
            case TriggerType.EnterMap:
                return true; // Checked on map entry
            case TriggerType.StepOnTile:
                return evt.triggerX == playerX && evt.triggerY == playerY;
            case TriggerType.TalkToNpc:
                return false; // Handled separately via CheckNpcEvent
        }
        return false;
    }

    // ---------------------------------------------------------------
    // Event Execution
    // ---------------------------------------------------------------

    private void StartEvent(StoryEvent evt)
    {
        SetupDynamicParties(evt);
        isRunningEvent = true;
        eventCoroutine = StartCoroutine(RunEventCoroutine(evt));
    }

    private IEnumerator RunEventCoroutine(StoryEvent evt)
    {
        Debug.Log($"[StoryEvent] Starting event: {evt.id}");

        foreach (var action in evt.actions)
        {
            yield return ExecuteAction(action);
        }

        isRunningEvent = false;
        eventCoroutine = null;

        Debug.Log($"[StoryEvent] Completed event: {evt.id}");
    }

    private IEnumerator ExecuteAction(EventAction action)
    {
        switch (action.type)
        {
            case ActionType.Dialogue:
                yield return ShowDialogue(action.speaker, action.text);
                break;

            case ActionType.Choice:
                yield return ShowChoice(action.text, action.choiceOptions, action.choiceFlagKeys);
                break;

            case ActionType.Battle:
                yield return StartStoryBattle(action);
                break;

            case ActionType.SetFlag:
                GameState.Instance.SetFlag(action.flagKey, action.flagValue);
                Debug.Log($"[StoryEvent] Flag set: {action.flagKey} = {action.flagValue}");
                break;

            case ActionType.GiveItem:
                GameState.Instance.Inventory.AddItem(action.itemId, action.itemCount);
                var itemData = DataLoader.Instance?.GetItem(action.itemId);
                string itemName = itemData != null ? itemData.name : $"Objet #{action.itemId}";
                yield return ShowDialogue(null, $"Vous avez recu {action.itemCount}x {itemName} !");
                break;

            case ActionType.GiveMoney:
                GameState.Instance.AddMoney(action.moneyAmount);
                yield return ShowDialogue(null, $"Vous avez recu {action.moneyAmount} pieces !");
                break;

            case ActionType.FadeOut:
                yield return DoFade(true);
                break;

            case ActionType.FadeIn:
                yield return DoFade(false);
                break;

            case ActionType.Heal:
                foreach (var dino in GameState.Instance.Party)
                {
                    dino.FullHeal();
                }
                yield return ShowDialogue(null, "Vos Dinomonstres ont ete soignes !");
                break;
        }
    }

    // ---------------------------------------------------------------
    // Dialogue Helpers
    // ---------------------------------------------------------------

    private IEnumerator ShowDialogue(string speaker, string text)
    {
        if (DialogueUI.Instance == null)
        {
            Debug.Log($"[StoryEvent] {speaker ?? "???"}: {text}");
            yield break;
        }

        bool done = false;
        DialogueUI.Instance.ShowText(text, speaker, () => { done = true; });

        while (!done) yield return null;
    }

    private IEnumerator ShowChoice(string prompt, string[] options, string[] flagKeys)
    {
        if (DialogueUI.Instance == null)
        {
            Debug.Log($"[StoryEvent] Choice: {prompt}");
            yield break;
        }

        int chosenIndex = -1;
        DialogueUI.Instance.ShowChoices(prompt, options, (idx) => { chosenIndex = idx; });

        while (chosenIndex < 0) yield return null;

        // Set flag for the chosen option
        if (flagKeys != null && chosenIndex < flagKeys.Length && !string.IsNullOrEmpty(flagKeys[chosenIndex]))
        {
            GameState.Instance.SetFlag(flagKeys[chosenIndex], true);
        }
    }

    // ---------------------------------------------------------------
    // Battle Helper
    // ---------------------------------------------------------------

    private IEnumerator StartStoryBattle(EventAction action)
    {
        // Build the battle setup
        var setup = new BattleSetupData
        {
            isWild = false,
            trainerName = action.trainerName,
            trainerId = action.trainerId ?? $"STORY_{action.trainerName}",
            trainerParty = action.trainerParty,
            returnScene = GameState.Instance.CurrentMapId,
            returnPosition = new Vector2(GameState.Instance.PlayerX, GameState.Instance.PlayerY)
        };

        if (setup.trainerParty != null && setup.trainerParty.Count > 0)
        {
            setup.enemySpeciesId = setup.trainerParty[0].speciesId;
            setup.enemyLevel = setup.trainerParty[0].level;
        }

        // If win or lose, mark the event flag before battle
        // The battle result will be checked on return

        // Start the battle
        isRunningEvent = false; // Allow the battle scene to run
        GameManager.Instance.StartBattle(setup);

        // The coroutine ends here; the rest of the event is handled
        // by the blocking flag being set before the battle action
        yield break;
    }

    // ---------------------------------------------------------------
    // Fade Helper
    // ---------------------------------------------------------------

    private IEnumerator DoFade(bool fadeOut)
    {
        // Simple screen fade using a full-screen overlay
        var fadeGO = new GameObject("StoryFade");
        var canvas = fadeGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var img = new GameObject("FadeImage").AddComponent<UnityEngine.UI.Image>();
        img.transform.SetParent(fadeGO.transform, false);
        img.color = fadeOut ? new Color(0, 0, 0, 0) : Color.black;
        var rect = img.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = fadeOut ? t : (1f - t);
            img.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (!fadeOut)
        {
            Destroy(fadeGO);
        }
        else
        {
            // Keep the black screen; FadeIn will remove it
            // Store reference for cleanup
            fadeGO.name = "StoryFade_Active";
        }
    }

    // ---------------------------------------------------------------
    // Helper: Build a rival party based on the player's starter
    // ---------------------------------------------------------------

    /// <summary>
    /// Get the rival's starter species based on the player's starter.
    /// Rival picks the type-advantage starter.
    /// Starters: 1=Pyrex(Fire), 4=Aquadon(Water), 7=Florasaur(Grass)
    /// </summary>
    private int GetRivalStarterId()
    {
        var lead = GameState.Instance.GetLeadDino();
        if (lead == null) return 4; // Default to water

        // Check the original starter (first party member's base species line)
        int playerStarter = GetPlayerStarterBase();

        switch (playerStarter)
        {
            case 1: // Player chose Fire -> Rival has Water
            case 2:
            case 3:
                return 4; // Aquadon
            case 4: // Player chose Water -> Rival has Grass
            case 5:
            case 6:
                return 7; // Florasaur
            case 7: // Player chose Grass -> Rival has Fire
            case 8:
            case 9:
                return 1; // Pyrex
            default:
                return 4;
        }
    }

    /// <summary>
    /// Get the evolution of the rival's starter at a given stage.
    /// Stage 0 = base, 1 = mid, 2 = final
    /// </summary>
    private int GetRivalStarterAtStage(int stage)
    {
        int baseId = GetRivalStarterId();
        // Starters evolve in groups of 3: base, mid, final
        // 1->2->3 (Fire), 4->5->6 (Water), 7->8->9 (Grass)
        return baseId + Mathf.Clamp(stage, 0, 2);
    }

    private int GetPlayerStarterBase()
    {
        // Check flags or examine party to determine which starter was chosen
        if (GameState.Instance.HasFlag("starter_fire")) return 1;
        if (GameState.Instance.HasFlag("starter_water")) return 4;
        if (GameState.Instance.HasFlag("starter_grass")) return 7;

        // Fallback: check first party member's species
        var party = GameState.Instance.Party;
        if (party.Count > 0)
        {
            int sid = party[0].speciesId;
            if (sid >= 1 && sid <= 3) return 1;
            if (sid >= 4 && sid <= 6) return 4;
            if (sid >= 7 && sid <= 9) return 7;
        }
        return 1; // Default
    }

    // ===============================================================
    // EVENT DEFINITIONS
    // ===============================================================

    private void RegisterAllEvents()
    {
        allEvents.Clear();

        // ---- BOURG-NID ----
        RegisterBourgNidEvents();

        // ---- ROUTE 1 ----
        RegisterRoute1Events();

        // ---- VILLE-FOUGERE ----
        RegisterVilleFougereEvents();

        // ---- ROUTE 3 ----
        RegisterRoute3Events();

        // ---- PORT-COQUILLE ----
        RegisterPortCoquilleEvents();

        // ---- ROCHE-HAUTE ----
        RegisterRocheHauteEvents();

        // ---- VOLCANVILLE ----
        RegisterVolcanvilleEvents();

        // ---- MARAIS-NOIR ----
        RegisterMaraisNoirEvents();

        // ---- VICTORY ROAD ----
        RegisterVictoryRoadEvents();

        // ---- PALEO CAPITAL ----
        RegisterPaleoCapitalEvents();

        Debug.Log($"[StoryEvent] Registered {allEvents.Count} story events");
    }

    // ---------------------------------------------------------------
    // BOURG-NID Events
    // ---------------------------------------------------------------

    private void RegisterBourgNidEvents()
    {
        // EVENT 1: INTRO — Post-starter dialogue when returning to Bourg-Nid
        allEvents.Add(new StoryEvent
        {
            id = "INTRO_POST_STARTER",
            mapId = "BOURG_NID",
            triggerType = TriggerType.EnterMap,
            requiredFlag = "has_starter",
            blockingFlag = "intro_complete",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Bravo ! Tu as choisi ton premier Dinomonstre !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Prends bien soin de lui. Ensemble, vous irez loin !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Ah, j'allais oublier ! Prends aussi ces objets pour la route."
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "intro_complete"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Dirige-toi vers le nord pour atteindre la Route 1. Bonne chance !"
                }
            }
        });

        // EVENT 2: RIVAL_INTRO — Rex appears after getting starter
        allEvents.Add(new StoryEvent
        {
            id = "RIVAL_INTRO",
            mapId = "BOURG_NID",
            triggerType = TriggerType.StepOnTile,
            triggerX = 12,
            triggerY = 18,
            requiredFlag = "intro_complete",
            blockingFlag = "rival_intro_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "He ! Attends une seconde !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Alors c'est toi le nouveau dresseur dont parle mon grand-pere ?"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Moi c'est Rex ! Et j'ai aussi recu mon premier Dinomonstre !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Voyons lequel de nous deux est le plus fort ! En garde !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "REX",
                    trainerId = "RIVAL_1",
                    winOrLose = true,
                    trainerParty = new List<TrainerDinoEntry>()
                    // Party set dynamically in StartEvent based on rival starter
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "rival_intro_done"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Pas mal du tout ! T'es plutot doue !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Tiens, prends ces Jurassic Balls. Tu en auras besoin pour capturer des Dinomonstres !"
                },
                new EventAction
                {
                    type = ActionType.GiveItem,
                    itemId = 16,   // Jurassic Ball
                    itemCount = 5
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Un conseil : les dinos sauvages sont plus faciles a capturer quand leurs PV sont bas."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "On se reverra ! La prochaine fois, je serai plus fort !"
                }
            }
        });
    }

    // ---------------------------------------------------------------
    // ROUTE 1 Events
    // ---------------------------------------------------------------

    private void RegisterRoute1Events()
    {
        // EVENT 3: FIRST_CATCH_TUTORIAL
        allEvents.Add(new StoryEvent
        {
            id = "FIRST_CATCH_TUTORIAL",
            mapId = "ROUTE_1",
            triggerType = TriggerType.EnterMap,
            requiredFlag = "has_starter",
            blockingFlag = "catch_tutorial_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Attends ! C'est moi, Prof. Saule, par l'intercom !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Tu vois ces hautes herbes ? Des Dinomonstres sauvages s'y cachent !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Pour en capturer un, affaiblis-le d'abord au combat, puis lance une Jurassic Ball !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Plus ses PV sont bas, plus tu as de chances de le capturer."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Tiens, prends aussi ces Potions. Ca pourrait t'etre utile !"
                },
                new EventAction
                {
                    type = ActionType.GiveItem,
                    itemId = 1,    // Potion
                    itemCount = 3
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "catch_tutorial_done"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Bonne chance ! Ville-Fougere t'attend au bout de cette route !"
                }
            }
        });
    }

    // ---------------------------------------------------------------
    // VILLE-FOUGERE Events
    // ---------------------------------------------------------------

    private void RegisterVilleFougereEvents()
    {
        // EVENT 4: RIVAL_VF — Rex waiting at Ville-Fougere entrance
        allEvents.Add(new StoryEvent
        {
            id = "RIVAL_VF",
            mapId = "VILLE_FOUGERE",
            triggerType = TriggerType.EnterMap,
            requiredFlag = "rival_intro_done",
            blockingFlag = "rival_vf_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Tiens, tiens ! Tu es arrive jusqu'ici aussi ?"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Il y a une arene ici. La championne s'appelle Flora et elle utilise des dinos Plante."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Un petit conseil : les dinos de type Feu sont tres efficaces contre le type Plante !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Moi j'ai deja battu Flora ! A toi de jouer maintenant !"
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "rival_vf_done"
                }
            }
        });

        // EVENT 5: GYM1_VICTORY — After beating Flora
        allEvents.Add(new StoryEvent
        {
            id = "GYM1_VICTORY",
            mapId = "VILLE_FOUGERE",
            triggerType = TriggerType.EnterMap,
            requiredBadgeCount = 1,
            blockingFlag = "gym1_victory_event_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Bravo ! Tu as battu Flora ! Je savais que tu en etais capable !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "La prochaine arene se trouve a Port-Coquille, au sud-est."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Passe par la Route 2 puis la Route 3 pour y arriver. Bonne route !"
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "gym1_victory_event_done"
                }
            }
        });
    }

    // ---------------------------------------------------------------
    // ROUTE 3 Events
    // ---------------------------------------------------------------

    private void RegisterRoute3Events()
    {
        // EVENT 6: TEAM_METEORE_INTRO — Grunts blocking the path
        allEvents.Add(new StoryEvent
        {
            id = "TEAM_METEORE_INTRO",
            mapId = "ROUTE_3",
            triggerType = TriggerType.StepOnTile,
            triggerX = 10,
            triggerY = 20,
            requiredBadgeCount = 1,
            blockingFlag = "meteore_intro_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "SBIRE METEORE",
                    text = "He toi ! On ne passe pas ! La Team Meteore controle cette zone !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "SBIRE METEORE",
                    text = "Tu veux jouer au heros ? Tres bien, on va te montrer !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "SBIRE METEORE",
                    trainerId = "METEORE_GRUNT_1",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 16, level = 10 },
                        new TrainerDinoEntry { speciesId = 52, level = 10 }
                    }
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "SBIRE METEORE 2",
                    text = "Mon camarade a perdu ? C'est a mon tour !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "SBIRE METEORE",
                    trainerId = "METEORE_GRUNT_2",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 10, level = 11 },
                        new TrainerDinoEntry { speciesId = 58, level = 11 }
                    }
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "SBIRE METEORE",
                    text = "Grrr... Ce gamin est trop fort ! Replions-nous !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "SBIRE METEORE 2",
                    text = "Le Commandant va entendre parler de toi... Tu n'as pas fini avec la Team Meteore !"
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "meteore_intro_done"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = null,
                    text = "Les sbires de la Team Meteore ont fui. Le chemin est libre !"
                }
            }
        });
    }

    // ---------------------------------------------------------------
    // PORT-COQUILLE Events
    // ---------------------------------------------------------------

    private void RegisterPortCoquilleEvents()
    {
        // EVENT 7: RIVAL_PC — Rex at the dock, rematch
        allEvents.Add(new StoryEvent
        {
            id = "RIVAL_PC",
            mapId = "PORT_COQUILLE",
            triggerType = TriggerType.EnterMap,
            requiredBadgeCount = 1,
            blockingFlag = "rival_pc_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Salut ! Je t'attendais au port."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "J'ai beaucoup entraine mes dinos depuis la derniere fois. Voyons ou tu en es !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "REX",
                    trainerId = "RIVAL_2",
                    winOrLose = true,
                    trainerParty = new List<TrainerDinoEntry>()
                    // Dynamically set: rival starter Lv14 + Compso Lv12
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "rival_pc_done"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Tu t'ameliores vite ! L'arene de Marin est dans ce port, prepare-toi !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "A la prochaine !"
                }
            }
        });

        // EVENT 8: STOLEN_DINO — Side quest
        allEvents.Add(new StoryEvent
        {
            id = "STOLEN_DINO",
            mapId = "PORT_COQUILLE",
            triggerType = TriggerType.StepOnTile,
            triggerX = 12,
            triggerY = 10,
            requiredFlag = "meteore_intro_done",
            blockingFlag = "stolen_dino_quest",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "CHERCHEUR",
                    text = "Au secours ! La Team Meteore a vole nos Dinomonstres de recherche !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "CHERCHEUR",
                    text = "Ils les ont emmenes vers la mine de Roche-Haute... Ces pauvres creatures !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "CHERCHEUR",
                    text = "Si tu les retrouves, je te recompenserai genereuseument !"
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "stolen_dino_quest"
                }
            }
        });
    }

    // ---------------------------------------------------------------
    // ROCHE-HAUTE Events
    // ---------------------------------------------------------------

    private void RegisterRocheHauteEvents()
    {
        // EVENT 9: MINE_EVENT — Team Meteore in the mine
        allEvents.Add(new StoryEvent
        {
            id = "MINE_EVENT",
            mapId = "ROCHE_HAUTE",
            triggerType = TriggerType.StepOnTile,
            triggerX = 10,
            triggerY = 8,
            requiredFlag = "stolen_dino_quest",
            blockingFlag = "mine_event_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = null,
                    text = "Vous entendez des bruits venant de la mine..."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "ADMIN METEORE",
                    text = "Qui ose interrompre nos operations ? Tu n'aurais pas du venir ici, gamin !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "ADMIN METEORE",
                    text = "Ces dinos nous appartiennent maintenant. La Team Meteore prendra le controle de tous les dinos !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "ADMIN SILEX",
                    trainerId = "METEORE_ADMIN_1",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 17, level = 20 },
                        new TrainerDinoEntry { speciesId = 11, level = 18 },
                        new TrainerDinoEntry { speciesId = 58, level = 20 }
                    }
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "ADMIN SILEX",
                    text = "Impossible... Vaincu par un gamin !?"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "ADMIN SILEX",
                    text = "Pfff... Garde tes precieux dinos. On se reverra !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = null,
                    text = "Les dinos captures ont ete liberes ! Ils retournent dans la nature."
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "mine_event_done"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = null,
                    text = "Vous trouvez un fossile rare parmi les debris !"
                },
                new EventAction
                {
                    type = ActionType.GiveItem,
                    itemId = 15,   // Fossil item (key item)
                    itemCount = 1
                },
                new EventAction
                {
                    type = ActionType.GiveMoney,
                    moneyAmount = 2000
                }
            }
        });
    }

    // ---------------------------------------------------------------
    // VOLCANVILLE Events
    // ---------------------------------------------------------------

    private void RegisterVolcanvilleEvents()
    {
        // EVENT 10: RIVAL_VC — Rex at hot springs
        allEvents.Add(new StoryEvent
        {
            id = "RIVAL_VC",
            mapId = "VOLCANVILLE",
            triggerType = TriggerType.EnterMap,
            requiredBadgeCount = 3,
            blockingFlag = "rival_vc_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Ah, te voila ! Les sources chaudes de Volcanville sont geniales !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Mais je ne suis pas la pour me relaxer. J'ai entraine mes dinos encore plus dur !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Montre-moi tes progres !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "REX",
                    trainerId = "RIVAL_3",
                    winOrLose = true,
                    trainerParty = new List<TrainerDinoEntry>()
                    // Dynamically set: evolved starter Lv24 + Compso Lv22 + Voltex Lv22
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "rival_vc_done"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Tu es vraiment devenu fort ! L'arene de Brazier est ici. Fais attention, le feu brule !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Ah, et j'ai entendu parler d'un dino legendaire quelque part... Primordius. Interessant, non ?"
                }
            }
        });

        // EVENT 11: LEGEND_HINT — NPC tells about Primordius
        allEvents.Add(new StoryEvent
        {
            id = "LEGEND_HINT",
            mapId = "VOLCANVILLE",
            triggerType = TriggerType.StepOnTile,
            triggerX = 15,
            triggerY = 5,
            requiredFlag = "rival_vc_done",
            blockingFlag = "legend_hint_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "ANCIEN",
                    text = "Tu as l'air d'un dresseur courageux... Laisse-moi te raconter une legende."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "ANCIEN",
                    text = "Il y a tres longtemps, un Dinomonstre immense regnait sur cette terre : Primordius."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "ANCIEN",
                    text = "On dit qu'il dort quelque part dans les profondeurs... La ou le feu rencontre la terre."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "ANCIEN",
                    text = "La Team Meteore le cherche aussi. Ils veulent utiliser son pouvoir pour controler tous les Dinomonstres !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "ANCIEN",
                    text = "Seul un dresseur au coeur pur pourra l'approcher... Peut-etre es-tu celui-la ?"
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "legend_hint_done"
                }
            }
        });
    }

    // ---------------------------------------------------------------
    // MARAIS-NOIR Events (Team Meteore HQ)
    // ---------------------------------------------------------------

    private void RegisterMaraisNoirEvents()
    {
        // EVENT 12: TEAM_METEORE_HQ — Storm the base
        allEvents.Add(new StoryEvent
        {
            id = "TEAM_METEORE_HQ",
            mapId = "MARAIS_NOIR",
            triggerType = TriggerType.StepOnTile,
            triggerX = 12,
            triggerY = 10,
            requiredFlag = "legend_hint_done",
            blockingFlag = "meteore_hq_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = null,
                    text = "L'entree secrete de la base de la Team Meteore est devant vous..."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "SBIRE METEORE",
                    text = "Un intrus ! Arretez-le !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "SBIRE METEORE",
                    trainerId = "METEORE_HQ_GRUNT_1",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 17, level = 28 },
                        new TrainerDinoEntry { speciesId = 52, level = 28 }
                    }
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "SBIRE METEORE",
                    text = "Vous n'irez pas plus loin !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "SBIRE METEORE",
                    trainerId = "METEORE_HQ_GRUNT_2",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 58, level = 29 },
                        new TrainerDinoEntry { speciesId = 11, level = 29 },
                        new TrainerDinoEntry { speciesId = 16, level = 30 }
                    }
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "COMMANDANT NOVA",
                    text = "Alors c'est toi qui deranges mes plans depuis le debut..."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "COMMANDANT NOVA",
                    text = "La Team Meteore va eveiller Primordius et controler tous les Dinomonstres du monde !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "COMMANDANT NOVA",
                    text = "Mais d'abord, je vais t'eliminer moi-meme !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "COMMANDANT NOVA",
                    trainerId = "METEORE_COMMANDER",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 18, level = 32 },   // Pandemonium
                        new TrainerDinoEntry { speciesId = 60, level = 32 },   // Nocturnyx
                        new TrainerDinoEntry { speciesId = 30, level = 34 },   // Magmatops
                        new TrainerDinoEntry { speciesId = 17, level = 33 }    // Venomex
                    }
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "COMMANDANT NOVA",
                    text = "Non... C'est impossible ! Mes plans... Ruines !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = null,
                    text = "Les Dinomonstres captures sont liberes ! La Team Meteore est en deroute !"
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "meteore_hq_done"
                },
                new EventAction
                {
                    type = ActionType.Heal
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "PROF. SAULE",
                    text = "Merci d'avoir sauve ces Dinomonstres ! Tu es un vrai heros !"
                },
                new EventAction
                {
                    type = ActionType.GiveMoney,
                    moneyAmount = 5000
                }
            }
        });
    }

    // ---------------------------------------------------------------
    // VICTORY ROAD Events
    // ---------------------------------------------------------------

    private void RegisterVictoryRoadEvents()
    {
        // EVENT 13: RIVAL_FINAL — Rex blocks Victory Road
        allEvents.Add(new StoryEvent
        {
            id = "RIVAL_FINAL",
            mapId = "VICTORY_ROAD",
            triggerType = TriggerType.StepOnTile,
            triggerX = 10,
            triggerY = 20,
            requiredBadgeCount = 8,
            blockingFlag = "rival_final_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Arrete-toi la !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Avant d'affronter le Conseil des Quatre, tu dois me battre une derniere fois !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "J'ai reuni les 8 badges aussi. Mes dinos sont au sommet de leur puissance !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "C'est notre combat ultime ! Que le meilleur gagne !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "REX",
                    trainerId = "RIVAL_FINAL",
                    trainerParty = new List<TrainerDinoEntry>()
                    // Dynamically set: full team Lv40+
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "rival_final_done"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "... Incroyable. Tu m'as battu avec tout ce que j'avais."
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Tu es pret pour le Conseil des Quatre. Va leur montrer de quoi tu es capable !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "REX",
                    text = "Bonne chance, mon ami. Je suis fier de t'avoir comme rival."
                },
                new EventAction
                {
                    type = ActionType.Heal
                }
            }
        });
    }

    // ---------------------------------------------------------------
    // PALEO CAPITAL Events (Elite Four)
    // ---------------------------------------------------------------

    private void RegisterPaleoCapitalEvents()
    {
        // EVENT 14: ELITE_FOUR — Must have 8 badges
        allEvents.Add(new StoryEvent
        {
            id = "ELITE_FOUR",
            mapId = "PALEO_CAPITAL",
            triggerType = TriggerType.StepOnTile,
            triggerX = 12,
            triggerY = 15,
            requiredBadgeCount = 8,
            requiredFlag = "rival_final_done",
            blockingFlag = "elite_four_done",
            actions = new List<EventAction>
            {
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "GARDE ROYAL",
                    text = "Bienvenue au Conseil des Quatre de Paleo Capital !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "GARDE ROYAL",
                    text = "Tu as les 8 badges. Tu es digne de defier les meilleurs dresseurs du monde !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "GARDE ROYAL",
                    text = "Prepare-toi : 4 combats consecutifs, puis le Champion. Pas de retour en arriere !"
                },
                // Elite 1 — Fossil specialist
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "FOSSILIA",
                    text = "Je suis Fossilia, premiere du Conseil. Mes fossiles ont traverse les millenaires !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "FOSSILIA",
                    trainerId = "ELITE_1",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 64, level = 44 },   // Relicor
                        new TrainerDinoEntry { speciesId = 12, level = 44 },   // Monolisaure
                        new TrainerDinoEntry { speciesId = 31, level = 45 },   // Stalagmor
                        new TrainerDinoEntry { speciesId = 66, level = 46 }    // Primordax
                    }
                },
                // Elite 2 — Ice specialist
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "GLACIUS",
                    text = "Le froid eternel vous attend. Je suis Glacius !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "GLACIUS",
                    trainerId = "ELITE_2",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 45, level = 45 },   // Glaciadon
                        new TrainerDinoEntry { speciesId = 79, level = 45 },   // Cryotitan
                        new TrainerDinoEntry { speciesId = 77, level = 46 },   // Glaciaire
                        new TrainerDinoEntry { speciesId = 132, level = 47 }   // Glaciornyx
                    }
                },
                // Elite 3 — Dark specialist
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "OMBRAGE",
                    text = "Les tenebres sont mon domaine. Tremble devant Ombrage !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "OMBRAGE",
                    trainerId = "ELITE_3",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 60, level = 46 },   // Nocturnyx
                        new TrainerDinoEntry { speciesId = 82, level = 46 },   // Eclipsadon
                        new TrainerDinoEntry { speciesId = 54, level = 47 },   // Abyssinthe
                        new TrainerDinoEntry { speciesId = 148, level = 48 }   // Tyranombre
                    }
                },
                // Elite 4 — Dragon specialist
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "DRACONIA",
                    text = "Tu as atteint le sommet ! Je suis Draconia. Mes dragons sont invincibles !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "DRACONIA",
                    trainerId = "ELITE_4",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 146, level = 47 },  // Draconyx
                        new TrainerDinoEntry { speciesId = 147, level = 48 },  // Wyverion
                        new TrainerDinoEntry { speciesId = 39, level = 48 },   // Maelstrom
                        new TrainerDinoEntry { speciesId = 150, level = 50 }   // Pangaeon
                    }
                },
                // Champion
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "CHAMPION PALEO",
                    text = "Felicitations, tu as vaincu le Conseil des Quatre !"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "CHAMPION PALEO",
                    text = "Mais il te reste un dernier defi : moi, le Champion de la region !"
                },
                new EventAction
                {
                    type = ActionType.Battle,
                    trainerName = "CHAMPION PALEO",
                    trainerId = "CHAMPION",
                    trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = 3, level = 50 },    // Volcanorex
                        new TrainerDinoEntry { speciesId = 6, level = 50 },    // Abyssaure
                        new TrainerDinoEntry { speciesId = 9, level = 50 },    // Titanarbore
                        new TrainerDinoEntry { speciesId = 130, level = 52 },  // Eternadon
                        new TrainerDinoEntry { speciesId = 127, level = 52 },  // Megalithon
                        new TrainerDinoEntry { speciesId = 150, level = 55 }   // Pangaeon
                    }
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = "CHAMPION PALEO",
                    text = "... Extraordinaire. Tu es le nouveau Champion de la region !"
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "elite_four_done"
                },
                new EventAction
                {
                    type = ActionType.SetFlag,
                    flagKey = "is_champion"
                },
                new EventAction
                {
                    type = ActionType.Dialogue,
                    speaker = null,
                    text = "Felicitations ! Vous etes devenu le Champion de la region !"
                },
                new EventAction
                {
                    type = ActionType.GiveMoney,
                    moneyAmount = 20000
                }
            }
        });
    }

    // ---------------------------------------------------------------
    // Dynamic Party Setup
    // ---------------------------------------------------------------

    /// <summary>
    /// Before starting a rival battle, fill in the dynamic party.
    /// </summary>
    private void SetupDynamicParties(StoryEvent evt)
    {
        foreach (var action in evt.actions)
        {
            if (action.type != ActionType.Battle) continue;
            if (action.trainerParty != null && action.trainerParty.Count > 0) continue;

            // This is a rival battle with empty party — fill dynamically
            switch (action.trainerId)
            {
                case "RIVAL_1":
                    action.trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = GetRivalStarterAtStage(0), level = 5 }
                    };
                    break;

                case "RIVAL_2":
                    action.trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = GetRivalStarterAtStage(0), level = 14 },
                        new TrainerDinoEntry { speciesId = 13, level = 12 }  // Plumex
                    };
                    break;

                case "RIVAL_3":
                    action.trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = GetRivalStarterAtStage(1), level = 24 },
                        new TrainerDinoEntry { speciesId = 14, level = 22 },  // Aeroraptor
                        new TrainerDinoEntry { speciesId = 40, level = 22 }   // Fulgure
                    };
                    break;

                case "RIVAL_FINAL":
                    action.trainerParty = new List<TrainerDinoEntry>
                    {
                        new TrainerDinoEntry { speciesId = GetRivalStarterAtStage(2), level = 45 },
                        new TrainerDinoEntry { speciesId = 15, level = 42 },  // Cieloptere
                        new TrainerDinoEntry { speciesId = 42, level = 42 },  // Oragriffe
                        new TrainerDinoEntry { speciesId = 36, level = 43 },  // Titanacier
                        new TrainerDinoEntry { speciesId = 30, level = 43 },  // Magmatops
                        new TrainerDinoEntry { speciesId = 66, level = 44 }   // Primordax
                    };
                    break;
            }

            // Set lead dino for battle setup
            if (action.trainerParty != null && action.trainerParty.Count > 0)
            {
                // These will be used by StartStoryBattle
            }
        }
    }

}
