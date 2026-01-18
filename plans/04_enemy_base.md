# Enemy Base Plan

## Goal

Implement the enemy base - the target players must destroy with their accumulated units. This gives the game an objective and win condition.

---

## What I'm Building

```
┌─────────────────────────────────────────┐
│                                         │
│         ┌─────────────────┐             │
│         │   ENEMY BASE    │             │
│         │   HP: ████░░    │  ← Target   │
│         └─────────────────┘             │
│                  ↑                      │
│              ●●●●●●●                    │
│              (units hit base)           │
│                  ↑                      │
│              ●●●●●●●●●●                 │
│                  ↑                      │
│              ┌─────┐                    │
│              │ ×3  │                    │
│              └─────┘                    │
│                  ↑                      │
│                 ●●●                     │
│                                         │
│              ___                        │
│             /   \\                       │
│            | (◯) |  ← Cannon            │
│            └─────┘                      │
│                                         │
└─────────────────────────────────────────┘

Units hit enemy base → Base HP decreases → HP reaches 0 → WIN!
```

---

## Reference: Mob Control's Enemy Base

Based on [MAF Analysis](https://maf.ad/en/blog/mob-control-analysis-hybrid-casual/):

| Feature | Mob Control Implementation |
|---------|---------------------------|
| Visual | Building/fortress with HP bar |
| Damage | Each unit deals 1 damage on contact |
| Feedback | Visual hit effect, HP bar updates |
| Destruction | Base explodes with particles when HP = 0 |
| Multiple Bases | Later levels have multiple bases |

For MVP, we'll implement a single base with simple HP mechanics.

---

## Unity Setup

### EnemyBase Prefab Structure

```
EnemyBase (Prefab)
├── Visual (SpriteRenderer - the base building)
├── HPBar (Canvas + Slider or Image fill)
│   ├── Background (Image)
│   └── Fill (Image - decreases as HP drops)
├── HitZone (BoxCollider2D - Is Trigger: true)
└── DestructionEffect (ParticleSystem - disabled, plays on death)
```

### Required Components

| GameObject | Components |
|------------|------------|
| EnemyBase | EnemyBase.cs, BoxCollider2D (trigger) |
| Visual | SpriteRenderer |
| HPBar | Canvas (World Space), Slider or Image |
| DestructionEffect | ParticleSystem |

### Tags & Layers

```
Edit → Tags and Layers:
- Tag: "EnemyBase" (for win condition check)
- Ensure "Unit" tag exists (from previous plans)
```

---

## Implementation Steps

### Step 1: Create Health System

A modular health system that can be reused ([reference](https://medium.com/nerd-for-tech/tip-of-the-day-modular-health-system-unity-8f5d2f187027)):

```csharp
// Assets/Scripts/Core/Health.cs
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxHealth = 100;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged; // current, max
    public UnityEvent OnDeath;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercent => (float)currentHealth / maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // Already dead

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(int newMax)
    {
        maxHealth = newMax;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
```

---

### Step 2: Create EnemyBase Script

```csharp
// Assets/Scripts/Entities/EnemyBase.cs
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private ParticleSystem destructionEffect;

    [Header("Settings")]
    [SerializeField] private int damagePerUnit = 1;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    private Color originalColor;
    private bool isDestroyed = false;

    void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (visual != null)
            originalColor = visual.color;
    }

    void Start()
    {
        // Subscribe to health events
        health.OnHealthChanged.AddListener(OnHealthChanged);
        health.OnDeath.AddListener(OnBaseDestroyed);
    }

    void OnDestroy()
    {
        health.OnHealthChanged.RemoveListener(OnHealthChanged);
        health.OnDeath.RemoveListener(OnBaseDestroyed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;
        if (!other.CompareTag("Unit")) return;

        // Deal damage to base
        health.TakeDamage(damagePerUnit);

        // Visual feedback
        StartCoroutine(FlashHit());

        // Return unit to pool
        var unit = other.GetComponent<Unit>();
        if (unit != null)
        {
            unit.ReturnToPool();
        }
    }

    private System.Collections.IEnumerator FlashHit()
    {
        if (visual != null)
        {
            visual.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            visual.color = originalColor;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        // HPBar updates via its own listener or direct reference
        // Can add screen shake or other effects here
    }

    private void OnBaseDestroyed()
    {
        isDestroyed = true;

        // Play destruction effect
        if (destructionEffect != null)
        {
            destructionEffect.transform.SetParent(null); // Unparent so it survives
            destructionEffect.Play();
            Destroy(destructionEffect.gameObject, destructionEffect.main.duration);
        }

        // Notify GameManager
        GameManager.Instance?.OnEnemyBaseDestroyed(this);

        // Hide or destroy the base
        visual.enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    // For stage setup
    public void Initialize(int hp)
    {
        health.SetMaxHealth(hp);
        isDestroyed = false;
        visual.enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }
}
```

---

### Step 3: Create HP Bar UI

```csharp
// Assets/Scripts/UI/HPBar.cs
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health targetHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private Slider slider; // Alternative to Image

    [Header("Settings")]
    [SerializeField] private Gradient colorGradient; // Green to Red
    [SerializeField] private bool useGradient = true;

    void Start()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.AddListener(UpdateBar);
            UpdateBar(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }

    void OnDestroy()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.RemoveListener(UpdateBar);
        }
    }

    private void UpdateBar(int current, int max)
    {
        float percent = (float)current / max;

        if (slider != null)
        {
            slider.value = percent;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = percent;

            if (useGradient && colorGradient != null)
            {
                fillImage.color = colorGradient.Evaluate(percent);
            }
        }
    }

    // For external assignment
    public void SetTarget(Health health)
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.RemoveListener(UpdateBar);
        }

        targetHealth = health;

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.AddListener(UpdateBar);
            UpdateBar(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }
}
```

---

### Step 4: Create GameManager (Basic Version)

```csharp
// Assets/Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Events")]
    public UnityEvent OnGameWin;
    public UnityEvent OnGameLose;

    private EnemyBase[] enemyBases;
    private int destroyedBases = 0;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Find all enemy bases in the scene
        enemyBases = FindObjectsOfType<EnemyBase>();
        destroyedBases = 0;
    }

    public void OnEnemyBaseDestroyed(EnemyBase destroyedBase)
    {
        destroyedBases++;

        // Check if all bases are destroyed
        if (destroyedBases >= enemyBases.Length)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        Debug.Log("WIN! All enemy bases destroyed!");
        OnGameWin?.Invoke();
        // Will be expanded in 05_basic_game_loop.md
    }

    public void TriggerLose()
    {
        Debug.Log("LOSE! Out of resources!");
        OnGameLose?.Invoke();
        // Will be expanded in 05_basic_game_loop.md
    }
}
```

---

### Step 5: Create EnemyBase Prefab

**In Unity Editor:**

1. Create empty GameObject, name it "EnemyBase"
2. Add BoxCollider2D (Is Trigger: true, size: 2 x 1)
3. Add Health.cs component
4. Add EnemyBase.cs component
5. Set tag to "EnemyBase"

6. Create child "Visual":
   - Add SpriteRenderer (use rectangle sprite)
   - Scale: (2, 1, 1)
   - Color: Red or enemy color

7. Create child "HPBar":
   - Add Canvas component (Render Mode: World Space)
   - Set Canvas size to fit above base
   - Add child "Background" with Image (gray)
   - Add child "Fill" with Image (green, Image Type: Filled)
   - Add HPBar.cs to HPBar object

8. Create child "DestructionEffect":
   - Add ParticleSystem
   - Configure for explosion effect
   - Set Play On Awake: false

9. Assign references in Inspector
10. Save as Prefab

---

### Step 6: Update Unit for Pool Return

Ensure Unit.cs has proper pool return (from 02_basic_unit_movement.md):

```csharp
// Already in Unit.cs, but verify this method exists:
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
```

---

## Scene Setup Checklist

1. **GameManager (Empty GameObject)**
   - Add GameManager.cs
   - Position doesn't matter

2. **EnemyBase Prefab Instance**
   - Position at top of screen: (0, 4, 0)
   - Set HP via Health component (e.g., 50 for testing)

3. **Verify Tags**
   - Unit prefab has "Unit" tag
   - EnemyBase prefab has "EnemyBase" tag

4. **Physics Settings**
   - Ensure Units and EnemyBase can interact (Layer Collision Matrix)

---

## Files to Create/Modify

```
Assets/Scripts/
├── Core/
│   ├── Health.cs           # New: Modular health system
│   └── GameManager.cs      # New: Game state management
├── Entities/
│   ├── Unit.cs             # Verify: ReturnToPool exists
│   └── EnemyBase.cs        # New
└── UI/
    └── HPBar.cs            # New

Assets/Prefabs/
├── Unit.prefab             # Verify: "Unit" tag
└── EnemyBase.prefab        # New
```

---

## Test Cases

| Test | Steps | Expected |
|------|-------|----------|
| Unit hits base | Spawn units, let them hit base | Base HP decreases, unit disappears |
| HP bar updates | Deal damage to base | HP bar fill decreases |
| Visual feedback | Unit hits base | Base flashes red briefly |
| Base destruction | Reduce HP to 0 | Destruction effect plays, base hidden |
| Win condition | Destroy all bases | GameManager.OnGameWin invoked |
| Multiple units | Many units hit base simultaneously | Each deals damage, no errors |

---

## Edge Cases

| Scenario | Handling |
|----------|----------|
| Unit hits already destroyed base | isDestroyed flag prevents processing |
| Multiple bases in level | Track destroyed count, win when all gone |
| Very high damage | Clamp HP to 0, don't go negative |
| Base HP set to 0 or negative | SetMaxHealth ensures minimum of 1 |

---

## Definition of Done

- [ ] Enemy base renders with visible HP bar
- [ ] Units hitting base deal damage (HP decreases)
- [ ] HP bar visually updates when damaged
- [ ] Base flashes or shows hit feedback
- [ ] Base destruction plays particle effect
- [ ] Win condition triggers when base HP reaches 0
- [ ] GameManager receives win notification
- [ ] Units return to pool after hitting base

---

## Performance Considerations

| Technique | Purpose |
|-----------|---------|
| Event-driven HP updates | No per-frame HP checks |
| Trigger collisions | Efficient collision detection |
| Object pooling for units | Units return to pool, not destroyed |

---

## Next Steps

After enemy base works → `05_basic_game_loop.md` (resource limits, lose condition, restart, UI)

---

*Information verified via web search - January 2025*

Sources:
- [Modular Health System](https://medium.com/nerd-for-tech/tip-of-the-day-modular-health-system-unity-8f5d2f187027)
- [Health Bar Implementation](https://www.codemahal.com/adding-a-health-bar-to-a-2d-game-in-unity)
- [Unity Discussions - Health System](https://discussions.unity.com/t/health-system/805432)
