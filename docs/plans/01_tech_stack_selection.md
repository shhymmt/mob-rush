# Tech Stack Selection

## Context

I need to choose a game framework for building Mob Rush. As a web developer, I'm comfortable with TypeScript and npm, but I've never used game-specific libraries or engines.

Key question: Should I use a web-based framework (familiar territory) or a full game engine like Unity (what the pros use)?

---

## Requirements

What I need from my tech stack:

| Requirement | Why |
|-------------|-----|
| Handle 100+ moving units | Core gameplay involves many entities |
| Good performance on mobile | Target platform is smartphones |
| Active community | Need to find answers when I'm stuck |
| AI (Claude) friendly | Claude should be able to write the code effectively |
| Path to app stores | Eventually want to publish on iOS/Android |

---

## Options Considered

### Option A: Vanilla Canvas API

**Pros:**
- No dependencies, smallest bundle
- Full control over everything
- Browser-native

**Cons:**
- Have to build everything from scratch
- No built-in sprite handling, animation, collision
- Performance ceiling for many objects

**Verdict**: Too much work, performance concerns for 100+ units

---

### Option B: PixiJS

**Pros:**
- Excellent 2D rendering performance (WebGL)
- Lightweight, focused on rendering
- TypeScript support

**Cons:**
- Rendering only - no game logic, physics
- Need additional libraries for collision, sound
- Still browser-based (mobile performance limits)

**Verdict**: Good renderer, but missing too many game features

---

### Option C: Phaser 3

**Pros:**
- Full game framework (physics, input, audio, scenes)
- Huge community, tons of examples
- Built-in arcade physics
- TypeScript support
- Familiar web development workflow (npm, Vite)

**Cons:**
- Browser-based = performance ceiling on mobile
- Bundle size (~1MB)
- Not the same engine as Mob Control (harder to reference)
- Web-to-mobile wrappers (Capacitor) add complexity

**Verdict**: Good for web games, but not ideal for mobile-first hyper-casual

---

### Option D: Unity (Recommended)

**Pros:**
- **Same engine as Mob Control** - can directly reference how the original works
- Native mobile performance (compiles to iOS/Android)
- Handles 1000+ objects easily
- Built-in physics, particles, audio, UI
- Massive community, endless tutorials
- Direct path to App Store / Google Play
- Claude Code can write C# effectively

**Cons:**
- Need to install Unity Editor (several GB)
- C# instead of TypeScript
- Scene editing requires Unity GUI
- Heavier project structure (.meta files, etc.)

**Verdict**: Best fit for mobile hyper-casual games

---

### Option E: Godot

**Pros:**
- Lightweight, open source
- GDScript is beginner-friendly
- Good 2D support

**Cons:**
- Smaller community than Unity
- Less familiar to Claude compared to Unity/C#
- Fewer mobile game references

**Verdict**: Good engine, but Unity has more resources for this genre

---

## Decision: Why Unity?

### The Realization

Initially, I leaned toward Phaser because it uses TypeScript (my comfort zone). But then I considered:

1. **Mob Control was built with Unity** - All tutorials, breakdowns, and clone guides reference Unity
2. **Claude Code writes the code** - The C# barrier is much lower when AI handles implementation
3. **Mobile is the goal** - Hyper-casual games live on mobile, not browsers
4. **Performance matters** - 100+ units with effects needs native performance

### What Changed My Mind

| Concern | Resolution |
|---------|------------|
| "I don't know C#" | Claude writes the code, I review and learn |
| "Unity is complex" | For a 2D game, we use a small subset of features |
| "Setup is heavy" | One-time install, then straightforward workflow |
| "Can't preview in browser" | Unity Editor has instant play mode |

---

## Final Decision

### Game Engine: Unity 6 LTS

```
┌─────────────────────────────────────────┐
│              Unity 6 LTS                │
├─────────────────────────────────────────┤
│  Rendering     │  Universal Render Pipeline (2D) │
│  Physics       │  Unity Physics 2D        │
│  Input         │  New Input System        │
│  Audio         │  Built-in Audio          │
│  UI            │  Unity UI / UI Toolkit   │
│  Particles     │  Particle System         │
│  Platform      │  iOS, Android, WebGL     │
└─────────────────────────────────────────┘
```

Why Unity for Mob Rush specifically:

| Need | Unity Feature |
|------|---------------|
| Spawn many units | Object pooling with GameObject.Instantiate |
| Units move upward | Rigidbody2D velocity |
| Collision with gates | Collider2D triggers |
| Touch/click input | Input System (pointer support) |
| Multiple stages | Scene management |
| Particles & effects | Particle System |
| Mobile deployment | Build to iOS/Android directly |

---

### Language: C#

- Strongly typed, similar structure to TypeScript
- Claude generates reliable C# code
- Excellent Unity integration
- Huge ecosystem of examples

---

### Version Control

Unity-specific considerations:
- Use `.gitignore` for Library/, Temp/, Logs/
- Track .meta files (required for Unity)
- Use text-based asset serialization

---

## Project Structure

```
samples/mob-rush/
├── docs/
│   └── plans/                    # Planning documents
├── Assets/
│   ├── Scenes/
│   │   ├── BootScene.unity       # Loading screen
│   │   ├── MenuScene.unity       # Main menu
│   │   └── GameScene.unity       # Main gameplay
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs    # Game state management
│   │   │   └── ObjectPool.cs     # Object pooling utility
│   │   ├── Entities/
│   │   │   ├── Unit.cs           # Player unit
│   │   │   ├── Gate.cs           # Multiplier gate
│   │   │   └── EnemyBase.cs      # Target to destroy
│   │   ├── Systems/
│   │   │   ├── SpawnSystem.cs    # Unit spawning
│   │   │   └── StageManager.cs   # Stage progression
│   │   └── UI/
│   │       └── GameUI.cs         # HUD elements
│   ├── Prefabs/
│   │   ├── Unit.prefab
│   │   ├── Gate.prefab
│   │   └── EnemyBase.prefab
│   ├── Art/
│   │   └── Sprites/
│   └── Audio/
│       └── SFX/
├── ProjectSettings/
├── Packages/
└── README.md
```

---

## Setup Steps

### 1. Install Unity

1. Download [Unity Hub](https://unity.com/download)
2. Install Unity 6 LTS (Long Term Support) - latest is 6.3 LTS
3. Include modules:
   - iOS Build Support (if on Mac)
   - Android Build Support
   - WebGL Build Support (for browser testing)

### 2. Create Project

```
Unity Hub → New Project → 2D (URP) → "mob-rush"
```

Choose **2D (URP)** template for optimized 2D rendering.

### 3. Project Settings

```csharp
// Edit → Project Settings → Player
Company Name: "PlanStack"
Product Name: "Mob Rush"
Default Orientation: Portrait
```

### 4. Recommended Packages

```
Window → Package Manager → Add:
- Input System (modern input handling)
- TextMeshPro (better text rendering)
- 2D Sprite (already included in 2D template)
```

### 5. Git Setup

```bash
# .gitignore for Unity
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Mm]emoryCaptures/
*.csproj
*.sln
*.suo
*.user
*.pidb
*.booproj
```

---

## Basic Script Structure

### MonoBehaviour Pattern

```csharp
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Move upward
        rb.velocity = Vector2.up * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Gate"))
        {
            // Handle gate collision
        }
    }
}
```

### Object Pooling

```csharp
using UnityEngine;
using UnityEngine.Pool;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int poolSize = 200;

    private ObjectPool<GameObject> unitPool;

    void Awake()
    {
        unitPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(unitPrefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            maxSize: poolSize
        );
    }

    public GameObject SpawnUnit(Vector3 position)
    {
        var unit = unitPool.Get();
        unit.transform.position = position;
        return unit;
    }

    public void DespawnUnit(GameObject unit)
    {
        unitPool.Release(unit);
    }
}
```

---

## Development Workflow

### With Claude Code

1. **Describe what you need** → Claude generates C# script
2. **Create script file** in Unity (Assets/Scripts/)
3. **Attach to GameObject** in Unity Editor
4. **Configure in Inspector** (adjust serialized fields)
5. **Test with Play button**
6. **Iterate** based on results

### Scene Setup (Manual in Editor)

Some things require Unity Editor:
- Creating GameObjects and hierarchy
- Attaching scripts to objects
- Setting up Prefabs
- Configuring physics layers
- Scene layout

Claude can guide these steps, but you'll click in the Editor.

---

## Performance Considerations

| Technique | Purpose |
|-----------|---------|
| Object Pooling | Reuse units instead of Instantiate/Destroy |
| Sprite Atlasing | Reduce draw calls |
| Physics Layers | Only check relevant collisions |
| Fixed Timestep | Consistent physics (0.02s default) |

Unity handles most optimization automatically for 2D games.

---

## Comparison Summary

| Aspect | Phaser 3 | Unity |
|--------|----------|-------|
| Language | TypeScript | C# |
| Setup | npm install | Unity Hub + Editor |
| Preview | Browser (localhost) | Unity Play button |
| Mobile | Capacitor wrapper | Native build |
| Performance | Good (WebGL) | Excellent (Native) |
| Mob Control reference | Indirect | Direct (same engine) |
| Claude compatibility | High | High |

**Winner for this project: Unity**

---

## Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Unity Editor learning curve | Focus on 2D subset, let Claude guide |
| C# unfamiliarity | Claude writes code, I learn by reviewing |
| Heavy project size | Use .gitignore properly, don't track Library/ |
| Scene editing is manual | Document steps clearly in plans |

---

## Next Steps

1. Install Unity Hub and Unity 6 LTS
2. Create new 2D (URP) project
3. Set up basic scene with camera
4. Create first script (Unit.cs)
5. Test spawning a single unit
6. Move to `02_basic_unit_movement.md`
