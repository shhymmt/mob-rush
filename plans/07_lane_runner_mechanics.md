# Lane Runner Mechanics Plan

**Created:** 2025-01-17
**Status:** Completed (2025-01-18)

## Overview

### What I Actually Want to Build

After completing Phase 6, I realized the game I built isn't what I had in mind. I was trying to clone Mob Control, but that's not the game I want to make.

What I actually want:
- A lane-based runner where enemies scroll toward me
- My units auto-shoot bullets to destroy enemies
- I switch between lanes to collect good gates and avoid bad ones

This is fundamentally different from Mob Control's cannon-aiming mechanics.

---

## Why This Redesign Is Needed

### The Problem

Phase 6 implemented Mob Control mechanics:
- Drag to aim cannon
- Units collide with enemies 1:1
- Fixed camera

But looking at my reference image again, I want:
- Tap to switch lanes
- Units shoot bullets that damage enemy HP
- Enemies/gates scroll down toward me

### My Reference Image (IMG_2381.jpg)

What I see in the image:
- 2 lanes with enemies scrolling down
- Blue units at bottom, shooting bullets upward
- A boss on the left with HP: 1338
- A gate showing -10 (subtracts units)
- Explosion effects when bullets hit enemies

This is the game I want to build.

---

## Target Game Design

```
      ← Scroll direction (enemies/gates move DOWN)

      Lane 1           Lane 2
         │               │
         ▼               ▼
       [+10]           [-5]      ← Gates scroll down
         │               │
         ▼               ▼
      [Enemy           [Enemy
       HP:50]          HP:30]    ← Enemies scroll down
         │               │
         │      ↑        │
         │    bullets    │        ← My units auto-fire
         │               │
   ┌─────────────────────────┐
   │   [My Units x3]         │    ← Tap lane to move
   └─────────────────────────┘
```

### My Specifications

| Element | What I Want |
|---------|-------------|
| Lanes | 2 (keep it simple for now) |
| Initial units | 3 |
| Scroll | Enemies/gates move DOWN toward me |
| Unit movement | All my units move together |
| Shooting | Auto-fire when not moving, stop while moving |
| Controls (Mobile) | Tap lane to move there |
| Controls (PC) | Click lane to move there |
| Damage | My unit count × 1 damage per volley |
| Gate types | +N (add), -N (subtract), xN (multiply) |
| Enemies | Have HP, take bullet damage |
| Game Over | Unit count = 0 OR collision with enemy |
| Scroll speed | Adjusts by stage (Stage 1 = slow tutorial) |

---

## What I Can Reuse from Phase 6

| Component | Decision | Reason |
|-----------|----------|--------|
| Object Pooling | **Keep** | Still useful for bullets, enemies |
| Health.cs | **Keep** | Enemy HP system works |
| GameManager | **Modify** | Need new win/lose conditions |
| Gate.cs | **Modify** | Add subtract gate, change trigger |
| Unit.cs | **Replace** | Completely different behavior |
| Enemy.cs | **Replace** | Now has HP and scrolls |
| InputHandler.cs | **Replace** | Lane-based input now |

### New Components I Need

| Component | Purpose |
|-----------|---------|
| LaneManager.cs | Manages 2 lanes, unit positions |
| Bullet.cs | Projectile fired by units |
| BulletSpawner.cs | Handles bullet pooling and firing |
| ScrollManager.cs | Controls scroll speed |
| LaneInputHandler.cs | Detects lane tap/click |

---

## Implementation Steps

### Phase 7A: Lane System Foundation

1. [ ] Create LaneManager with 2 lanes
2. [ ] Create lane visual indicators
3. [ ] Implement LaneInputHandler (tap/click detection)
4. [ ] Create unit group that moves between lanes
5. [ ] **Test**: Tap/click moves units between lanes

### Phase 7B: Scrolling System

1. [ ] Create ScrollManager to control scroll speed
2. [ ] Implement object scrolling (move down over time)
3. [ ] Add off-screen detection and recycling
4. [ ] Configure scroll speed parameter
5. [ ] **Test**: Objects scroll down and disappear

### Phase 7C: Bullet System

1. [ ] Create Bullet.cs (moves up, deals damage)
2. [ ] Create BulletSpawner with object pooling
3. [ ] Implement auto-fire logic (fire when not moving)
4. [ ] Fire rate = 1 volley per X seconds
5. [ ] Damage = unit count × 1
6. [ ] **Test**: Units fire bullets, bullets move up

### Phase 7D: Enemy System Redesign

1. [ ] Modify Enemy.cs to have HP (use Health component)
2. [ ] Enemy takes damage from bullets
3. [ ] Enemy scrolls down with ScrollManager
4. [ ] Collision with my units = game over
5. [ ] Enemy destroyed when HP = 0
6. [ ] **Test**: Bullets damage enemies, enemies scroll down

### Phase 7E: Gate System Redesign

1. [ ] Modify Gate.cs for lane-based positioning
2. [ ] Add subtract gate type (-N)
3. [ ] Gates scroll down with ScrollManager
4. [ ] Trigger when my units pass through
5. [ ] **Test**: Gates add/subtract/multiply my units

### Phase 7F: Game Loop Update

1. [ ] Update GameManager for new game over conditions
2. [ ] Unit count = 0 → Game Over
3. [ ] Collision with enemy → Game Over
4. [ ] Stage completion condition
5. [ ] Update UI for new game state
6. [ ] **Test**: Win and lose conditions work

### Phase 7G: Stage System

1. [ ] Create stage configuration (scroll speed, enemy patterns)
2. [ ] Stage 1 = Tutorial (slow, simple)
3. [ ] Progressive difficulty
4. [ ] **Test**: Multiple stages playable

---

## My Concerns

### Performance

- Lots of bullets on screen = need pooling
- Need to recycle enemies/gates that scroll off-screen
- Bullet collision detection might get expensive

### What Could Go Wrong

- Lane switching might feel sluggish
- Bullet firing rate might make game too easy or hard
- Scroll speed balance will need iteration

---

## Test Plan

| Test | What I Do | What Should Happen |
|------|-----------|-------------------|
| Lane switch | Tap other lane | All my units move there |
| Shooting | Wait after moving | Bullets fire automatically |
| No shoot while moving | Tap lane | Shooting stops during move |
| Enemy damage | Let bullets hit enemy | Enemy HP goes down |
| Enemy death | Reduce HP to 0 | Enemy disappears |
| Gate +N | Go through +10 gate | My unit count: 3 → 13 |
| Gate -N | Go through -5 gate | My unit count: 13 → 8 |
| Game over (collision) | Let enemy reach me | Game over screen |
| Game over (zero units) | Hit -N gates until 0 | Game over screen |

---

## Definition of Done

- [ ] 2 lanes with tap/click switching
- [ ] My units auto-fire bullets when stationary
- [ ] Enemies scroll down and take bullet damage
- [ ] Gates (+N, -N, xN) affect my unit count
- [ ] Game over on collision or zero units
- [ ] Stage 1 (tutorial) playable
- [ ] Feels like my reference image

---

## Notes

- This is a big redesign, not a small tweak
- Some Phase 6 code can be reused, but most will change
- Focus on core mechanics first, polish later
- If something doesn't work, I'll iterate

---

*Plan created: 2025-01-17*
*Status: Awaiting approval*
