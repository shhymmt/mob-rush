# Core Combat Mechanics Plan

**Created:** 2025-01-17
**Status:** Completed

## Overview

### What I Need to Build

I need to implement the core Mob Control experience:
1. Cannon direction control (drag/swipe)
2. Moving red enemy units
3. 1v1 cancellation combat system

This will transform my game from "just clicking" to "strategic decision-making."

---

## Background & Objectives

### Why I Need This

Feedback after implementing Phases 1-5:
> "Frankly, mob-rush is not fun. It's just clicking."

### My Analysis of the Problem

| My Mob Rush | Actual Mob Control |
|-------------|-------------------|
| Units go straight up | Player controls direction |
| Enemy is a static base | Red enemies move toward player |
| Just depleting HP | 1v1 cancellation, numbers battle |
| No strategy | Decide which gates to aim for |

### My Goals

- Require "decisions" from the player
- Create tension through numbers battle
- Reproduce Mob Control's core experience

---

## Related Past Implementations

- [02_basic_unit_movement.md](02_basic_unit_movement.md) - Unit script, object pooling
- [03_multiplier_gates.md](03_multiplier_gates.md) - Gate passage, unit multiplication
- [04_enemy_base.md](04_enemy_base.md) - Enemy base, Health system (partially reusable)
- [05_basic_game_loop.md](05_basic_game_loop.md) - GameManager, win/lose detection

---

## What I'm Building

```
┌─────────────────────────────────────────────────────────┐
│                                                         │
│              [Enemy Tower] HP: ████░░                   │
│                    │                                    │
│                    ▼ (spawns red units)                 │
│               ●●● (red enemies)                         │
│                    │                                    │
│                    ▼                                    │
│   [×2]  ←─────── BATTLE ───────→  [×3]                 │
│                 (1:1 cancellation)                      │
│                    ↑                                    │
│               ○○○ (my blue units)                       │
│                    ↑                                    │
│              [Cannon] ←── Drag to change direction      │
│                                                         │
│   Controls: My units fire in the direction of drag     │
│   Combat: 1 Blue + 1 Red = Both destroyed              │
│   Win: My units reach Enemy Tower and destroy it       │
│   Lose: Red units reach the bottom of screen           │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## My Implementation Approach

### Change Strategy

I want to maximize reuse of existing code and minimize the diff.

| Feature | New/Change | Description |
|---------|-----------|-------------|
| Cannon control | Change | Add drag direction detection to InputHandler |
| Unit direction | Change | Add direction parameter to Unit.cs |
| Red enemies | New | Enemy.cs (based on Unit.cs) |
| Enemy spawner | New | EnemySpawner.cs |
| Combat system | New | Unit/Enemy collision = cancellation |
| Enemy tower | Change | Rename EnemyBase to EnemyTower, add spawn function |
| Lose condition | Change | Lose when enemy reaches bottom |

### Scope of Impact

**Files to Modify:**
- `Assets/Scripts/Systems/InputHandler.cs` - Drag direction detection
- `Assets/Scripts/Entities/Unit.cs` - Direction parameter, enemy collision
- `Assets/Scripts/Systems/UnitSpawner.cs` - Pass direction
- `Assets/Scripts/Entities/EnemyBase.cs` → `EnemyTower.cs` - Add enemy spawn
- `Assets/Scripts/Core/GameManager.cs` - Update lose condition

**New Files:**
- `Assets/Scripts/Entities/Enemy.cs` - Red enemy unit
- `Assets/Scripts/Systems/EnemySpawner.cs` - Enemy spawn management (or integrate into EnemyTower)

---

## Implementation Steps

### Phase 6A: Cannon Control

1. [x] Add drag position tracking to InputHandler
2. [x] Calculate direction vector from drag position to cannon
3. [x] Add direction interface to UnitSpawner
4. [x] Add direction parameter to Unit.Initialize()
5. [x] Set Unit velocity based on direction
6. [x] **Test**: Units move in dragged direction

### Phase 6B: Red Enemy Units

1. [x] Create Enemy.cs (based on Unit.cs, moves downward)
2. [x] Create "Enemy" tag
3. [x] Add enemy spawn function to EnemyTower (formerly EnemyBase)
4. [x] Configure spawn interval
5. [x] Object pooling (same as Unit)
6. [x] **Test**: Red units spawn from tower and move down

### Phase 6C: 1v1 Cancellation Combat

1. [x] Add collision detection between Unit and Enemy
2. [x] On collision, destroy both (return to pool)
3. [x] Visual feedback (particles optional)
4. [x] **Test**: Blue and red units cancel each other

### Phase 6D: Win/Lose Condition Update

1. [x] Set HP on EnemyTower (damage when Unit reaches it)
2. [x] Lose when Enemy reaches defeat line (bottom of screen)
3. [x] Update GameManager win/lose logic
4. [x] Update UI (show lose condition)
5. [x] **Test**: Both win and lose conditions work correctly

### Phase 6E: Balance Adjustment

1. [x] Adjust enemy spawn frequency
2. [x] Adjust unit speed
3. [x] Adjust gate placement
4. [x] Playtest for difficulty

---

## My Concerns

### Performance

- Collision detection increases between blue and red units
- I'll continue using object pooling
- May need to optimize with Physics Layers

### Backward Compatibility

- Scene structure will change significantly
- Need to migrate EnemyBase → EnemyTower
- Existing win/lose UI can be reused

### Unity Editor Work

- Create Enemy prefab
- Reconfigure EnemyTower
- Create new "Enemy" tag
- Place defeat line (invisible trigger)

---

## Test Plan

| Test | Steps | Expected Result |
|------|-------|-----------------|
| Cannon control | Drag and spawn units | Units move in drag direction |
| Enemy spawn | Start game | Red units spawn from tower |
| Cancellation | Collide blue and red | Both disappear |
| Overwhelm | Multiply through gates and attack | Can destroy enemy tower |
| Win | Destroy enemy tower | VICTORY displayed |
| Lose | Get overwhelmed by enemies | DEFEATED displayed |
| Restart | Press button after win/lose | Game resets |

---

## Definition of Done

- [x] Drag changes cannon direction
- [x] Red units spawn from enemy tower
- [x] Blue and red cancel each other 1:1
- [x] Blue units reaching enemy tower deal damage
- [x] Red units reaching bottom causes defeat
- [x] Win/lose/restart work correctly
- [x] Game requires "decisions" and is fun

---

## Notes

- This plan aims to reproduce Mob Control's core experience
- Visual polish (particles, sound, etc.) will be in the next phase
- Issues discovered during implementation will be recorded here

**2025-01-17 Update**: This phase was completed successfully, but I realized afterward that this isn't the game I actually want to build. See [07_lane_runner_mechanics.md](07_lane_runner_mechanics.md) for the new direction.

---

## References

- [Mob Control Strategy Guide - Gamezebo](https://www.gamezebo.com/walkthroughs/mob-control-strategy-guide-take-control-with-these-hints-tips-and-cheats-2/)
- [Mob Control - SilverGames](https://www.silvergames.com/en/mob-control)
- [Mob Control Analysis - MAF](https://maf.ad/en/blog/mob-control-analysis-hybrid-casual/)

---

*Plan created: 2025-01-17*
*Status: Completed (2025-01-17)*
