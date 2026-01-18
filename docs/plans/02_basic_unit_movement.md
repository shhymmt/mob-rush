# Basic Unit Movement Plan

## Goal

Create the core gameplay mechanic: player taps/holds to spawn units from a cannon, and units move upward toward the enemy.

---

## Current State

Unity project created with 2D (URP) template. Now implementing the first gameplay feature.

---

## What I'm Building

```
┌─────────────────────────────────────────┐
│                                         │
│              ↑ ↑ ↑                       │
│             (units moving up)           │
│              ↑ ↑ ↑                       │
│                                         │
│                                         │
│                                         │
│               ___                       │
│              /   \                      │
│             |     |  ← Cannon           │
│             └─────┘                     │
│                                         │
│  [ Tap/Hold anywhere to spawn units ]   │
│                                         │
└─────────────────────────────────────────┘
```

---

## User Stories

| As a... | I want to... | So that... |
|---------|--------------|------------|
| Player | Tap to spawn a unit | I can send units toward the enemy |
| Player | Hold to spawn continuously | I can create a stream of units |
| Player | See units moving | I get feedback that my input worked |
| Player | Watch units disappear off-screen | The game doesn't slow down |

---

## Game Design Decisions

### Unit Behavior

| Property | Value | Rationale |
|----------|-------|-----------|
| Speed | 5 units/sec | Fast enough to feel responsive, slow enough to see |
| Size | 0.5 units | Small enough to have many on screen |
| Spawn rate | 10 units/sec (while holding) | Feels like a stream without overwhelming |
| Direction | Straight up (Vector2.up) | Simple for v1, can add aiming later |

### Cannon Behavior

| Property | Value | Rationale |
|----------|-------|-----------|
| Position | Bottom center (0, -4) | Standard for this genre |
| Visual | Simple sprite or shape | Placeholder, can improve later |
| Aim | Fixed upward | Simplest implementation first |

---

## Unity Setup Requirements

### Scene Hierarchy

```
GameScene
├── Main Camera (Orthographic, size 5)
├── Cannon
│   └── SpawnPoint (empty GameObject for spawn position)
├── UnitSpawner (script holder)
└── UI Canvas (optional for now)
```

### Required Components

| GameObject | Components |
|------------|------------|
| Unit (Prefab) | SpriteRenderer, Rigidbody2D, CircleCollider2D, Unit.cs |
| Cannon | SpriteRenderer, Cannon.cs |
| UnitSpawner | UnitSpawner.cs |

### Physics Settings

```
Edit → Project Settings → Physics 2D
- Gravity: (0, 0)  ← No gravity, units move by velocity only
```

---

## Implementation Steps

### Step 1: Create Unit Prefab

Create the basic unit that will be spawned.

**In Unity Editor:**
1. Create empty GameObject, name it "Unit"
2. Add SpriteRenderer (use Unity's default circle sprite)
3. Add Rigidbody2D (Body Type: Dynamic, Gravity Scale: 0)
4. Add CircleCollider2D (Is Trigger: true)
5. Create script Unit.cs and attach

```csharp
// Assets/Scripts/Entities/Unit.cs
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize()
    {
        // Start moving upward
        rb.velocity = Vector2.up * speed;
    }

    public void Deactivate()
    {
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}
```

6. Drag to Assets/Prefabs folder to create prefab
7. Delete from scene (we'll spawn via script)

**Success criteria**: Unit prefab exists in Prefabs folder

---

### Step 2: Create Cannon Visual

**In Unity Editor:**
1. Create empty GameObject at (0, -4), name it "Cannon"
2. Add SpriteRenderer (use rectangle sprite, scale to look like cannon)
3. Create child empty GameObject at (0, 0.5), name it "SpawnPoint"

```csharp
// Assets/Scripts/Entities/Cannon.cs
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public Transform spawnPoint;

    void Awake()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }
}
```

**Success criteria**: Cannon visible at bottom of game view

---

### Step 3: Input Detection

Create input handler that detects tap/hold.

```csharp
// Assets/Scripts/Systems/InputHandler.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public bool IsSpawning { get; private set; }

    void Update()
    {
        // Works for both mouse and touch
        if (Mouse.current != null)
        {
            IsSpawning = Mouse.current.leftButton.isPressed;
        }

        // Alternative: Touch input
        if (Touchscreen.current != null)
        {
            IsSpawning = Touchscreen.current.primaryTouch.press.isPressed;
        }
    }
}
```

Or simpler version using old Input system:

```csharp
// Assets/Scripts/Systems/InputHandler.cs
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public bool IsSpawning { get; private set; }

    void Update()
    {
        // Works for mouse click and touch
        IsSpawning = Input.GetMouseButton(0);
    }
}
```

**Success criteria**: Can detect when screen is being pressed

---

### Step 4: Unit Spawner with Object Pooling

```csharp
// Assets/Scripts/Systems/UnitSpawner.cs
using UnityEngine;
using UnityEngine.Pool;

public class UnitSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private InputHandler inputHandler;

    [Header("Settings")]
    [SerializeField] private float spawnRate = 10f; // units per second
    [SerializeField] private int poolSize = 200;

    private ObjectPool<GameObject> unitPool;
    private float spawnTimer;
    private float spawnInterval;

    void Awake()
    {
        spawnInterval = 1f / spawnRate;

        unitPool = new ObjectPool<GameObject>(
            createFunc: CreateUnit,
            actionOnGet: OnGetUnit,
            actionOnRelease: OnReleaseUnit,
            actionOnDestroy: OnDestroyUnit,
            defaultCapacity: poolSize,
            maxSize: poolSize
        );
    }

    void Update()
    {
        if (inputHandler.IsSpawning)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                SpawnUnit();
                spawnTimer = 0f;
            }
        }
        else
        {
            spawnTimer = spawnInterval; // Ready to spawn immediately on next press
        }
    }

    private void SpawnUnit()
    {
        Vector3 position = spawnPoint.position;
        // Add slight random spread
        position.x += Random.Range(-0.2f, 0.2f);

        var unitObj = unitPool.Get();
        unitObj.transform.position = position;
    }

    private GameObject CreateUnit()
    {
        var obj = Instantiate(unitPrefab);
        obj.SetActive(false);
        return obj;
    }

    private void OnGetUnit(GameObject obj)
    {
        obj.SetActive(true);
        var unit = obj.GetComponent<Unit>();
        unit.Initialize();
    }

    private void OnReleaseUnit(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyUnit(GameObject obj)
    {
        Destroy(obj);
    }

    // Called by units when they go off screen
    public void ReturnToPool(GameObject unitObj)
    {
        unitPool.Release(unitObj);
    }
}
```

**Success criteria**: Tap spawns units, hold spawns continuous stream

---

### Step 5: Off-Screen Detection

Update Unit.cs to return to pool when off-screen:

```csharp
// Assets/Scripts/Entities/Unit.cs
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float despawnY = 6f; // Off the top of screen

    private Rigidbody2D rb;
    private UnitSpawner spawner;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Find spawner (or inject via Initialize)
        spawner = FindObjectOfType<UnitSpawner>();
    }

    public void Initialize()
    {
        rb.velocity = Vector2.up * speed;
    }

    void Update()
    {
        // Check if off screen
        if (transform.position.y > despawnY)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        if (spawner != null)
        {
            spawner.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
```

**Success criteria**: Units disappear when going off-screen, can spawn 100+ without lag

---

### Step 6: Visual Polish (Optional)

Add spawn animation:

```csharp
// In Unit.cs Initialize()
public void Initialize()
{
    rb.velocity = Vector2.up * speed;

    // Scale-up animation
    transform.localScale = Vector3.one * 0.5f;
    StartCoroutine(ScaleUp());
}

private IEnumerator ScaleUp()
{
    float duration = 0.1f;
    float elapsed = 0f;
    Vector3 startScale = Vector3.one * 0.5f;
    Vector3 endScale = Vector3.one;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        transform.localScale = Vector3.Lerp(startScale, endScale, t);
        yield return null;
    }

    transform.localScale = endScale;
}
```

---

## Scene Setup Checklist

**In Unity Editor, set up:**

1. **Main Camera**
   - Projection: Orthographic
   - Size: 5
   - Position: (0, 0, -10)
   - Background: Dark color (#1a1a2e)

2. **Cannon**
   - Position: (0, -4, 0)
   - Add Cannon.cs script
   - Child "SpawnPoint" at (0, 0.5, 0)

3. **UnitSpawner (Empty GameObject)**
   - Add UnitSpawner.cs
   - Add InputHandler.cs
   - Assign references in Inspector:
     - Unit Prefab: drag from Prefabs folder
     - Spawn Point: drag Cannon's SpawnPoint
     - Input Handler: drag self

4. **Unit Prefab**
   - Scale: (0.3, 0.3, 1)
   - SpriteRenderer: Circle sprite, green color
   - Rigidbody2D: Dynamic, Gravity Scale 0
   - CircleCollider2D: Is Trigger true
   - Unit.cs attached

---

## Files to Create

```
Assets/
├── Scenes/
│   └── GameScene.unity
├── Scripts/
│   ├── Entities/
│   │   ├── Unit.cs
│   │   └── Cannon.cs
│   └── Systems/
│       ├── UnitSpawner.cs
│       └── InputHandler.cs
├── Prefabs/
│   └── Unit.prefab
└── Art/
    └── Sprites/
        └── (use Unity defaults for now)
```

---

## Test Cases

| Test | Steps | Expected |
|------|-------|----------|
| Single spawn | Click once | One unit appears and moves up |
| Continuous spawn | Hold down | Stream of units while held |
| Release stops spawning | Release after holding | No new units appear |
| Off-screen cleanup | Spawn many units | Units disappear when off-screen |
| Performance | Hold for 10 seconds | No lag, maintain 60 FPS |

---

## Edge Cases

| Scenario | Handling |
|----------|----------|
| Very fast clicking | Rate-limited by spawnInterval |
| Tab out while spawning | Unity pauses when not focused (default) |
| Pool exhausted | ObjectPool blocks until one is returned |
| Touch vs Mouse | Input.GetMouseButton works for both |

---

## Definition of Done

- [ ] Cannon visible at bottom of screen
- [ ] Tap spawns single unit
- [ ] Hold spawns continuous stream
- [ ] Units move upward at consistent speed
- [ ] Off-screen units are recycled to pool
- [ ] 100+ units on screen without frame drops
- [ ] Works on desktop (mouse) and mobile (touch)

---

## Next Steps

Once units spawn and move → `03_multiplier_gates.md` (gates that multiply units)
