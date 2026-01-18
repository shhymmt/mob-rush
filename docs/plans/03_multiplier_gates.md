# Multiplier Gates Plan

## Goal

Implement gates that multiply units passing through them. This is the core "numbers go up" mechanic that makes the game satisfying.

---

## What I'm Building

```
┌─────────────────────────────────────────┐
│                                         │
│    ┌─────┐         ┌─────┐              │
│    │ ×3  │         │ ×2  │   ← Gates    │
│    └──┬──┘         └──┬──┘              │
│       │               │                 │
│       ↓               ↓                 │
│      ●●●             ●●    ← Output     │
│       ↑               ↑                 │
│       ●               ●     ← Input     │
│                                         │
│              ___                        │
│             /   \                       │
│            | (◯) |  ← Cannon            │
│            └─────┘                      │
│                                         │
└─────────────────────────────────────────┘

1 unit enters ×3 gate → 3 units come out
```

---

## Gate Types

For the first version:

| Gate | Effect | Visual |
|------|--------|--------|
| ×2 | Doubles units | Green gate |
| ×3 | Triples units | Blue gate |
| ×5 | 5× units | Purple gate |
| +10 | Adds 10 units | Yellow gate |

---

## Unity Setup

### Gate Prefab Structure

```
Gate (Prefab)
├── Visual (SpriteRenderer - the colored bar)
├── Label (TextMeshPro - "×2", "+10", etc.)
└── TriggerZone (BoxCollider2D - Is Trigger: true)
```

### Required Components

| GameObject | Components |
|------------|------------|
| Gate | Gate.cs, BoxCollider2D (trigger) |
| Visual | SpriteRenderer |
| Label | TextMeshProUGUI or TextMeshPro |

### Tags & Layers

```
Edit → Tags and Layers:
- Create tag: "Gate"
- Create tag: "Unit"
```

---

## Implementation Steps

### Step 1: Create Gate Script

```csharp
// Assets/Scripts/Entities/Gate.cs
using UnityEngine;
using TMPro;

public enum GateType { Multiply, Add }

public class Gate : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GateType gateType = GateType.Multiply;
    [SerializeField] private int value = 2;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private TextMeshPro label;

    [Header("Colors")]
    [SerializeField] private Color multiplyColor = Color.green;
    [SerializeField] private Color addColor = Color.yellow;

    private UnitSpawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<UnitSpawner>();
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // Set color based on type and value
        if (gateType == GateType.Multiply)
        {
            visual.color = value switch
            {
                2 => new Color(0, 1, 0),      // Green
                3 => new Color(0, 0.5f, 1),   // Blue
                5 => new Color(0.6f, 0, 1),   // Purple
                _ => multiplyColor
            };
            label.text = $"×{value}";
        }
        else
        {
            visual.color = addColor;
            label.text = $"+{value}";
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Unit")) return;

        var unit = other.GetComponent<Unit>();
        if (unit == null) return;

        // Check if unit already passed this gate
        if (unit.HasPassedGate(this)) return;

        // Mark as passed
        unit.MarkGatePassed(this);

        // Process the gate effect
        ProcessGateEffect(unit);
    }

    private void ProcessGateEffect(Unit unit)
    {
        int unitsToSpawn = gateType == GateType.Multiply
            ? value - 1  // Original continues, spawn extras
            : value;     // Add spawns exactly this many

        Vector3 basePos = unit.transform.position;

        for (int i = 0; i < unitsToSpawn; i++)
        {
            SpawnExtraUnit(basePos);
        }

        // Visual feedback
        ShowPopup(basePos);
        PulseGate();
    }

    private void SpawnExtraUnit(Vector3 basePosition)
    {
        if (spawner == null) return;

        Vector3 offset = new Vector3(
            Random.Range(-0.3f, 0.3f),
            0.2f,
            0
        );

        var newUnit = spawner.SpawnUnitAtPosition(basePosition + offset);

        // Copy passed gates to new unit so it doesn't re-trigger same gate
        if (newUnit != null)
        {
            var unitComponent = newUnit.GetComponent<Unit>();
            unitComponent?.MarkGatePassed(this);
        }
    }

    private void ShowPopup(Vector3 position)
    {
        // TODO: Implement popup text effect
        // For now, just log
        Debug.Log($"Gate triggered: {(gateType == GateType.Multiply ? "×" : "+")}{value}");
    }

    private void PulseGate()
    {
        // Simple scale pulse animation
        StartCoroutine(PulseAnimation());
    }

    private System.Collections.IEnumerator PulseAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        float duration = 0.1f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;

        // Scale down
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
```

---

### Step 2: Update Unit Script

Add gate tracking to Unit.cs:

```csharp
// Assets/Scripts/Entities/Unit.cs
using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float despawnY = 6f;

    private Rigidbody2D rb;
    private UnitSpawner spawner;
    private HashSet<Gate> passedGates = new HashSet<Gate>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        spawner = FindObjectOfType<UnitSpawner>();
    }

    public void Initialize()
    {
        rb.velocity = Vector2.up * speed;
        passedGates.Clear(); // Reset when reused from pool
    }

    void Update()
    {
        if (transform.position.y > despawnY)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        passedGates.Clear();

        if (spawner != null)
        {
            spawner.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public bool HasPassedGate(Gate gate)
    {
        return passedGates.Contains(gate);
    }

    public void MarkGatePassed(Gate gate)
    {
        passedGates.Add(gate);
    }
}
```

---

### Step 3: Update UnitSpawner

Add method to spawn at specific position:

```csharp
// Add to UnitSpawner.cs

public GameObject SpawnUnitAtPosition(Vector3 position)
{
    var unitObj = unitPool.Get();
    if (unitObj != null)
    {
        unitObj.transform.position = position;
    }
    return unitObj;
}
```

---

### Step 4: Create Gate Prefab

**In Unity Editor:**

1. Create empty GameObject, name it "Gate"
2. Add BoxCollider2D (Is Trigger: true, size: 1.5 x 0.3)
3. Set tag to "Gate"

4. Create child "Visual":
   - Add SpriteRenderer (use square sprite)
   - Scale: (1.5, 0.2, 1)

5. Create child "Label":
   - Add TextMeshPro component (or use TextMesh)
   - Anchor: center
   - Font size: 0.5 (world space)

6. Add Gate.cs script to root
7. Assign references in Inspector
8. Save as Prefab

---

### Step 5: Place Gates in Scene

Create a simple GateManager or place manually:

```csharp
// Assets/Scripts/Systems/GateManager.cs
using UnityEngine;

public class GateManager : MonoBehaviour
{
    [SerializeField] private GameObject gatePrefab;

    void Start()
    {
        // Example stage layout
        CreateGate(GateType.Multiply, 2, new Vector3(-1.5f, 0f, 0f));
        CreateGate(GateType.Multiply, 3, new Vector3(1.5f, 0f, 0f));
        CreateGate(GateType.Multiply, 2, new Vector3(0f, 2f, 0f));
        CreateGate(GateType.Add, 5, new Vector3(-1f, 3.5f, 0f));
        CreateGate(GateType.Multiply, 5, new Vector3(1f, 3.5f, 0f));
    }

    private void CreateGate(GateType type, int value, Vector3 position)
    {
        var gateObj = Instantiate(gatePrefab, position, Quaternion.identity);
        var gate = gateObj.GetComponent<Gate>();
        // Note: Would need to expose SetGateType method or use SerializeField
    }
}
```

Or place gates manually in the scene and configure via Inspector.

---

### Step 6: Popup Effect (Optional)

```csharp
// Assets/Scripts/Effects/PopupText.cs
using UnityEngine;
using TMPro;
using System.Collections;

public class PopupText : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeDuration = 0.5f;

    public void Show(string message, Color color)
    {
        text.text = message;
        text.color = color;
        StartCoroutine(AnimateAndDestroy());
    }

    private IEnumerator AnimateAndDestroy()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Color startColor = text.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Float up
            transform.position = startPos + Vector3.up * (floatSpeed * t);

            // Fade out
            Color c = startColor;
            c.a = 1f - t;
            text.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}
```

---

## Scene Setup Checklist

1. **Unit Prefab** - Add "Unit" tag
2. **Gate Prefab** - Add "Gate" tag, BoxCollider2D (trigger)
3. **GateManager** - Empty GameObject with GateManager.cs
4. **Physics Settings** - Ensure Units can collide with Gates

**Layer Collision Matrix:**
```
Edit → Project Settings → Physics 2D → Layer Collision Matrix
Ensure Units and Gates can interact
```

---

## Files to Create/Modify

```
Assets/Scripts/
├── Entities/
│   ├── Unit.cs         # Modified: add gate tracking
│   └── Gate.cs         # New
├── Systems/
│   ├── UnitSpawner.cs  # Modified: add SpawnUnitAtPosition
│   └── GateManager.cs  # New (optional)
└── Effects/
    └── PopupText.cs    # New (optional)

Assets/Prefabs/
├── Unit.prefab         # Modified: add tag
└── Gate.prefab         # New
```

---

## Test Cases

| Test | Steps | Expected |
|------|-------|----------|
| Basic multiply | 1 unit hits ×2 gate | 2 units continue upward |
| Triple multiply | 1 unit hits ×3 gate | 3 units continue upward |
| Add gate | 1 unit hits +5 gate | 6 units total (1 + 5) |
| Chain gates | 1 unit hits ×2, then ×3 | 6 units after both gates |
| No double trigger | Unit passes same gate | Only multiplies once |
| Visual feedback | Unit hits gate | Gate pulses, popup shows |

---

## Edge Cases

| Scenario | Handling |
|----------|----------|
| Many units hit same gate simultaneously | Each triggers independently |
| Pool exhausted during multiply | Spawn as many as possible |
| Unit spawned by gate hits another gate | Works normally (chain multiplying) |
| Overlapping gates | Unit triggers both (design decision) |

---

## Definition of Done

- [ ] Gates render with correct colors and labels
- [ ] Units passing through gates multiply correctly
- [ ] ×2, ×3, ×5, and +N gates all work
- [ ] Visual feedback (pulse, optional popup)
- [ ] Units don't trigger same gate twice
- [ ] Chain multiplying works (gate → gate)
- [ ] Multiple gates in stage layout

---

## Next Steps

After gates work → `04_enemy_base.md` (target to destroy with accumulated units)
