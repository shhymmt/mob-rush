# Basic Game Loop Plan

## Goal

Complete the core game loop: resource management, lose condition, win/lose UI screens, and restart functionality. This transforms the prototype into a playable game.

---

## What I'm Building

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│   ┌─────────────────────────────────────────────────────┐   │
│   │              GAME LOOP                              │   │
│   │                                                     │   │
│   │   ┌──────────┐    ┌──────────┐    ┌──────────┐     │   │
│   │   │  START   │───▶│  PLAY    │───▶│   END    │     │   │
│   │   │  GAME    │    │  GAME    │    │  SCREEN  │     │   │
│   │   └──────────┘    └────┬─────┘    └────┬─────┘     │   │
│   │                        │               │           │   │
│   │                   ┌────┴────┐          │           │   │
│   │                   ▼         ▼          │           │   │
│   │              [WIN]      [LOSE]         │           │   │
│   │                   │         │          │           │   │
│   │                   └────┬────┘          │           │   │
│   │                        ▼               │           │   │
│   │                   [RESTART]◀───────────┘           │   │
│   │                                                     │   │
│   └─────────────────────────────────────────────────────┘   │
│                                                             │
│   Win:  Destroy all enemy bases                            │
│   Lose: Run out of spawn resources                         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Reference: Hyper-Casual Game Loop

Based on [Unity Learn - Core Game Loop](https://learn.unity.com/course/2D-adventure-robot-repair/unit/game-ui-and-game-loop/tutorial/close-the-core-game-loop):

| Element | Implementation |
|---------|----------------|
| Clear Objective | Destroy enemy base (shown via HP bar) |
| Resource Limit | Limited spawns or timer |
| Win Condition | All bases destroyed |
| Lose Condition | Resources depleted, bases remain |
| Quick Restart | One-tap restart, minimal friction |
| Feedback | Clear win/lose screens |

---

## Game State Flow

```
┌────────────────────────────────────────────────────────┐
│                                                        │
│  GameState.Playing                                     │
│  ├── Units spawning (resource decreasing)             │
│  ├── Units hitting gates (multiplying)                │
│  └── Units hitting base (damage dealing)              │
│       │                                               │
│       ├── Base HP = 0 ──────▶ GameState.Won          │
│       │                                               │
│       └── Resources = 0 ────▶ Wait for all units     │
│           (can't spawn more)     │                    │
│                                  ▼                    │
│                          All units despawned?         │
│                          ├── Base alive ──▶ GameState.Lost
│                          └── Base dead ───▶ GameState.Won
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## Unity Setup

### UI Structure

```
UICanvas (Screen Space - Overlay)
├── GameHUD
│   ├── SpawnCounter (TextMeshPro - "Units: 50")
│   └── EnemyHPBar (optional, or use world space on base)
├── WinScreen (disabled by default)
│   ├── Background (Image - semi-transparent)
│   ├── WinText (TextMeshPro - "VICTORY!")
│   ├── StarsDisplay (optional)
│   └── RestartButton (Button)
└── LoseScreen (disabled by default)
    ├── Background (Image - semi-transparent)
    ├── LoseText (TextMeshPro - "DEFEATED")
    └── RestartButton (Button)
```

---

## Implementation Steps

### Step 1: Define Game States

```csharp
// Assets/Scripts/Core/GameState.cs
public enum GameState
{
    Playing,
    Won,
    Lost,
    Paused
}
```

---

### Step 2: Expand GameManager

```csharp
// Assets/Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingSpawnCount = 50;

    [Header("References")]
    [SerializeField] private UnitSpawner unitSpawner;

    [Header("Events")]
    public UnityEvent OnGameWin;
    public UnityEvent OnGameLose;
    public UnityEvent<int> OnSpawnCountChanged; // remaining count
    public UnityEvent<GameState> OnGameStateChanged;

    private GameState currentState = GameState.Playing;
    private int remainingSpawns;
    private EnemyBase[] enemyBases;
    private int destroyedBases = 0;

    public GameState CurrentState => currentState;
    public int RemainingSpawns => remainingSpawns;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        currentState = GameState.Playing;
        remainingSpawns = startingSpawnCount;
        destroyedBases = 0;

        // Find all enemy bases
        enemyBases = FindObjectsOfType<EnemyBase>();

        // Notify UI
        OnSpawnCountChanged?.Invoke(remainingSpawns);
        OnGameStateChanged?.Invoke(currentState);

        // Resume time (in case it was paused)
        Time.timeScale = 1f;
    }

    // Called by UnitSpawner when a unit is spawned
    public bool TryConsumeSpawn()
    {
        if (currentState != GameState.Playing) return false;
        if (remainingSpawns <= 0) return false;

        remainingSpawns--;
        OnSpawnCountChanged?.Invoke(remainingSpawns);

        // Check if out of spawns
        if (remainingSpawns <= 0)
        {
            // Start checking for lose condition
            StartCoroutine(CheckForLoseCondition());
        }

        return true;
    }

    private System.Collections.IEnumerator CheckForLoseCondition()
    {
        // Wait for all active units to either hit base or despawn
        yield return new WaitForSeconds(0.5f); // Small delay

        while (currentState == GameState.Playing)
        {
            // Count active units
            int activeUnits = CountActiveUnits();

            if (activeUnits == 0)
            {
                // No more units and no more spawns
                if (destroyedBases < enemyBases.Length)
                {
                    TriggerLose();
                }
                // If all bases destroyed, win was already triggered
                yield break;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private int CountActiveUnits()
    {
        // Count active unit GameObjects
        var units = FindObjectsOfType<Unit>();
        int count = 0;
        foreach (var unit in units)
        {
            if (unit.gameObject.activeInHierarchy)
                count++;
        }
        return count;
    }

    public void OnEnemyBaseDestroyed(EnemyBase destroyedBase)
    {
        destroyedBases++;

        if (destroyedBases >= enemyBases.Length)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Won;
        Debug.Log("WIN! All enemy bases destroyed!");

        OnGameWin?.Invoke();
        OnGameStateChanged?.Invoke(currentState);

        // Optional: slow motion effect
        Time.timeScale = 0.5f;
    }

    public void TriggerLose()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Lost;
        Debug.Log("LOSE! Out of resources!");

        OnGameLose?.Invoke();
        OnGameStateChanged?.Invoke(currentState);
    }

    // Called by restart button
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Optional: Load specific scene
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
```

---

### Step 3: Update UnitSpawner for Resource Check

```csharp
// Modify UnitSpawner.cs - Update the SpawnUnit method

private void SpawnUnit()
{
    // Check with GameManager if we can spawn
    if (GameManager.Instance != null && !GameManager.Instance.TryConsumeSpawn())
    {
        return; // Can't spawn - out of resources or game ended
    }

    Vector3 position = spawnPoint.position;
    position.x += Random.Range(-0.2f, 0.2f);

    var unitObj = unitPool.Get();
    unitObj.transform.position = position;
}
```

---

### Step 4: Create Game UI Controller

```csharp
// Assets/Scripts/UI/GameUIController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI spawnCountText;

    [Header("Win Screen")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private Button winRestartButton;

    [Header("Lose Screen")]
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private Button loseRestartButton;

    void Start()
    {
        // Hide end screens
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);

        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSpawnCountChanged.AddListener(UpdateSpawnCount);
            GameManager.Instance.OnGameWin.AddListener(ShowWinScreen);
            GameManager.Instance.OnGameLose.AddListener(ShowLoseScreen);

            // Initialize spawn count display
            UpdateSpawnCount(GameManager.Instance.RemainingSpawns);
        }

        // Setup restart buttons
        if (winRestartButton != null)
            winRestartButton.onClick.AddListener(OnRestartClicked);
        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(OnRestartClicked);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSpawnCountChanged.RemoveListener(UpdateSpawnCount);
            GameManager.Instance.OnGameWin.RemoveListener(ShowWinScreen);
            GameManager.Instance.OnGameLose.RemoveListener(ShowLoseScreen);
        }
    }

    private void UpdateSpawnCount(int count)
    {
        if (spawnCountText != null)
        {
            spawnCountText.text = $"Units: {count}";
        }
    }

    private void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
    }

    private void ShowLoseScreen()
    {
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
        }
    }

    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }
}
```

---

### Step 5: Create UI Prefabs in Unity Editor

**In Unity Editor:**

1. **Create UI Canvas**
   - GameObject → UI → Canvas
   - Render Mode: Screen Space - Overlay
   - Add CanvasScaler (Scale With Screen Size, 1080x1920 reference)

2. **Create HUD**
   - Create empty child "GameHUD"
   - Add TextMeshPro child for spawn count
   - Position at top of screen
   - Font size: 48, Center aligned

3. **Create Win Screen**
   - Create empty child "WinScreen"
   - Add Image (full screen, semi-transparent black #00000080)
   - Add TextMeshPro "VICTORY!" (center, large font, gold color)
   - Add Button "Play Again"
   - Set WinScreen inactive by default

4. **Create Lose Screen**
   - Create empty child "LoseScreen"
   - Add Image (full screen, semi-transparent black)
   - Add TextMeshPro "DEFEATED" (center, large font, red color)
   - Add Button "Try Again"
   - Set LoseScreen inactive by default

5. **Add GameUIController**
   - Add GameUIController.cs to Canvas
   - Assign all references in Inspector

---

### Step 6: Scene Setup for Restart

Based on [Unity Scene Management](https://damiandabrowski.medium.com/scene-management-in-unity-a-comprehensive-guide-to-loading-scenes-sync-and-async-845ae1e129be):

**Build Settings:**
1. File → Build Settings
2. Add current scene (GameScene) to build
3. Note the scene index (usually 0)

**Scene Setup:**
```
GameScene
├── Main Camera
├── Cannon
│   └── SpawnPoint
├── UnitSpawner
│   ├── UnitSpawner.cs
│   └── InputHandler.cs
├── GameManager (NEW)
│   └── GameManager.cs
├── EnemyBase (instance of prefab)
└── UICanvas
    ├── GameHUD
    ├── WinScreen
    └── LoseScreen
```

---

## Test Flow

```
1. Game starts
   - Spawn count shows "Units: 50"
   - Player can spawn units

2. Player spawns units
   - Counter decreases with each spawn
   - Units hit gates, multiply
   - Units hit enemy base, deal damage

3. Win Path
   - Enemy base HP reaches 0
   - "VICTORY!" screen appears
   - Restart button works

4. Lose Path
   - Spawn count reaches 0
   - Wait for all units to despawn
   - Base still has HP
   - "DEFEATED" screen appears
   - Restart button works
```

---

## Files to Create/Modify

```
Assets/Scripts/
├── Core/
│   ├── GameState.cs        # New: Enum for game states
│   └── GameManager.cs      # Modified: Full implementation
├── Systems/
│   └── UnitSpawner.cs      # Modified: Resource check
└── UI/
    └── GameUIController.cs # New

Assets/Scenes/
└── GameScene.unity         # Modified: Add GameManager, UI
```

---

## Test Cases

| Test | Steps | Expected |
|------|-------|----------|
| Spawn counter | Spawn units | Counter decreases from 50 |
| Win condition | Destroy base before spawns run out | Win screen appears |
| Lose condition | Use all spawns without destroying base | Lose screen appears after units gone |
| Restart from win | Click restart on win screen | Game resets, counter at 50 |
| Restart from lose | Click restart on lose screen | Game resets, counter at 50 |
| Can't spawn after 0 | Use all spawns, try to spawn | No new units appear |
| Game paused on end | Win or lose | No more spawning possible |

---

## Edge Cases

| Scenario | Handling |
|----------|----------|
| Last unit destroys base | Win (checked before lose) |
| Multiple bases, some destroyed | Track count, win when all gone |
| Player doesn't spawn any units | Eventually lose (0 spawns, base alive) |
| Very fast restart clicking | SceneManager handles gracefully |
| Units in flight when game ends | They continue but don't affect state |

---

## Definition of Done

- [ ] Spawn counter displays and updates correctly
- [ ] Game detects win condition (all bases destroyed)
- [ ] Game detects lose condition (out of spawns, base survives)
- [ ] Win screen appears with "VICTORY!" and restart button
- [ ] Lose screen appears with "DEFEATED" and restart button
- [ ] Restart button reloads the scene correctly
- [ ] Time scale resets on restart
- [ ] Can play multiple rounds without issues

---

## Polish Ideas (P1)

| Feature | Implementation |
|---------|----------------|
| Score display | Track units used, time taken |
| Star rating | 3 stars based on efficiency |
| Slow-mo on win | Time.timeScale = 0.3f briefly |
| Sound effects | Win/lose audio cues |
| Animations | UI slides in/out |

---

## Next Steps (Phase 6+)

After basic game loop → `06_stage_system.md` (multiple levels, stage selection, progression)

This completes the MVP! Player can now:
1. Spawn units (limited resource)
2. Pass through gates (multiply)
3. Attack enemy base (deal damage)
4. Win or lose (clear feedback)
5. Restart (try again)

---

*Information verified via web search - January 2025*

Sources:
- [Unity Learn - Core Game Loop](https://learn.unity.com/course/2D-adventure-robot-repair/unit/game-ui-and-game-loop/tutorial/close-the-core-game-loop)
- [Scene Management Best Practices](https://damiandabrowski.medium.com/scene-management-in-unity-a-comprehensive-guide-to-loading-scenes-sync-and-async-845ae1e129be)
- [GameManager Pattern](https://frankgwarman.medium.com/intro-to-scene-management-and-restarting-the-game-acfe91f75370)
