# Game Design Document: Mob Rush

## Overview

### What I Want to Build

A mobile hyper-casual game inspired by Mob Control. Players launch units from a cannon, pass them through multiplier gates to grow their army, and overwhelm the enemy base.

### Background

- **Me**: 27 years old, web engineer (TypeScript/React for 3 years)
- **Game dev experience**: Zero. Played lots of games, never made one.
- **Why this game**: Played Mob Control on my phone and thought "this looks simple enough to build." Famous last words, probably.

---

## Reference Game: Mob Control (Verified Information)

### Basic Information

| Item | Details | Source |
|------|---------|--------|
| Developer | Mambo Studio (part of Voodoo) | [MAF Analysis](https://maf.ad/en/blog/mob-control-analysis-hybrid-casual/) |
| Publisher | Voodoo | [App Store](https://apps.apple.com/us/app/mob-control/id1562817072) |
| Release | April 13, 2021 (iOS/Android), May 20, 2024 (Steam) | [Udonis](https://www.blog.udonis.co/mobile-marketing/mobile-games/mob-control) |
| Engine | Unity | [Voodoo Job Posting](https://builtin.com/job/senior-game-developer-player-experience-game-feel-mob-control/7328392) |
| Genre | Started as hyper-casual, evolved to hybrid-casual | [Gamigion](https://www.gamigion.com/from-hyper-to-hybrid-casual-mob-control-playbook/) |

### Performance (2025)

| Metric | Value | Source |
|--------|-------|--------|
| Total Downloads | 250+ million | [Udonis](https://www.blog.udonis.co/mobile-marketing/mobile-games/mob-control) |
| Daily Active Users | ~1.5 million | [Udonis](https://www.blog.udonis.co/mobile-marketing/mobile-games/mob-control) |
| Monthly Active Users | ~8 million | [Udonis](https://www.blog.udonis.co/mobile-marketing/mobile-games/mob-control) |
| Total Revenue | $68+ million | [MAF Analysis](https://maf.ad/en/blog/mob-control-analysis-hybrid-casual/) |
| Monthly Revenue | $2-2.5 million | [MAF Analysis](https://maf.ad/en/blog/mob-control-analysis-hybrid-casual/) |
| Revenue Split | 85% Ads, 15% IAP | [PocketGamer](https://www.pocketgamer.biz/mob-control-hybridising-a-hypercasual-hit/) |

### Core Mechanics

Players shoot stickman units through multiplier gates (×2, ×3, etc.) to grow their crowd, then attack enemy bases. Red enemies deplete player units and must be dodged or pushed back.

### Evolution (2021 → 2024)

The game evolved significantly from its hyper-casual origins:

| Aspect | 2021 (Launch) | 2024 (Current) |
|--------|---------------|----------------|
| Visuals | Generic, low-quality | Polished, unique assets |
| Gameplay | Simple, repetitive | New modes, elements gradually introduced |
| Live Events | None | Tournaments, seasonal events, IP collabs (Transformers) |
| Meta | Basic upgrades | Card collector metagame (Cannons, Champions, Mobs, Ultimates) |
| Team Size | 4 people | 12 people |
| Monthly IAP | ~$10K | $300K-400K+ |

### Key Features to Reference

1. **Multiplier Gates**: ×2, ×3, ×5 etc. - the core "numbers go up" mechanic
2. **Champions System**: Special units that break through enemy mobs
3. **Cannon Upgrades**: Different firing patterns and abilities
4. **Base Builder**: Added in 2024 for ~20% LTV increase
5. **Fake PvP**: AI opponents with convincing avatars and emoji reactions

---

## Why Mob Control as Reference?

What makes it a good first game project:

| Aspect | Why It's Good for Learning |
|--------|---------------------------|
| Simple controls | Just tap/hold to shoot - no complex input handling |
| Clear feedback loop | Numbers go up = dopamine. Easy to make satisfying |
| Minimal art needed | Stick figures or colored circles work fine |
| Incremental complexity | Can start with 1 unit, add features later |
| Same engine | Mob Control uses Unity, we use Unity - can reference directly |
| Proven success | 250M+ downloads validates the core mechanic |

### The Core Loop

> **⚠️ 2025-01-17 Revision**: The original design only included a static enemy base.
> After researching Mob Control's actual gameplay, we discovered the core mechanics were different.
> Below is the corrected core loop.

```
┌─────────────────────────────────────────────────────────┐
│                                                         │
│               [Enemy Tower/Base]                        │
│                      │                                  │
│                      ▼ (spawns red enemies)             │
│                   ●●●●● (red)                           │
│                      │                                  │
│                      ▼                                  │
│   [×2 Gate]  ←─── BATTLE ZONE ───→  [×3 Gate]          │
│       │         (blue vs red)            │              │
│       │          1:1 cancellation        │              │
│       ▼              ↑                   ▼              │
│   [Blue Units] ─────┘                [Blue Units]       │
│        ↑                                  ↑             │
│        └──────────┬───────────────────────┘             │
│                   │                                     │
│              [Cannon] ← Player controls direction       │
│                                                         │
│   Win:  Blue units reach enemy tower and destroy it    │
│   Lose: Red enemies reach the bottom / player base     │
│                                                         │
└─────────────────────────────────────────────────────────┘

Combat: 1 Blue + 1 Red = Both destroyed (cancellation)
Strategy: Multiply through gates, overwhelm with numbers
```

---

## Core Features

### P0: Minimum Playable Game

> **⚠️ 2025-01-17 Revision**: The original P0 did not include "enemy units" or "cannon aiming",
> but these are essential to Mob Control's core experience.

| Feature | Description |
|---------|-------------|
| Unit spawning | Tap/hold to spawn units from cannon |
| **Cannon aiming** | **Drag/swipe to change cannon direction** |
| Unit movement | Units move in the aimed direction |
| Multiplier gates | ×2, ×3, etc. gates that duplicate units |
| **Red enemies** | **Red units spawn from enemy tower and move toward player** |
| **1v1 combat** | **Blue vs Red = both destroyed (cancellation)** |
| Enemy tower | Source of enemy units, destroy to win |
| Win/lose state | Win by destroying tower, lose if enemies reach bottom |

### P1: Game Feel

| Feature | Description |
|---------|-------------|
| Visual feedback | Particles, screen shake, number popups |
| Sound effects | Satisfying audio for spawning, multiplying |
| Stage progression | Multiple levels with increasing difficulty |
| Score system | Track performance across stages |

### P2: Polish (Stretch)

| Feature | Description |
|---------|-------------|
| Obstacles | Walls, rotating barriers, enemy cannons |
| Unit types | Different unit classes with special abilities |
| Upgrades | Permanent progression between runs |
| Champions | Special units like Mob Control's champion system |

---

## Development Phases

### Phase 1: Hello Game World

**Goal**: Get something moving on screen

- [ ] Set up Unity 6 LTS project with 2D (URP) template
- [ ] Render a simple shape (the "unit")
- [ ] Make it move across the screen
- [ ] Test in Unity Editor play mode

**Success criteria**: I can see a circle moving on my screen

---

### Phase 2: Cannon & Spawning

**Goal**: Player can spawn units

- [ ] Create cannon sprite at bottom of screen
- [ ] Detect tap/hold input (Input System)
- [ ] Spawn units at cannon position with object pooling
- [ ] Units move upward automatically (Rigidbody2D velocity)

**Success criteria**: Tap the screen → units appear and move up

---

### Phase 3: Multiplier Gates

**Goal**: Units can be multiplied

- [ ] Create gate objects with multiplier values
- [ ] Detect collision between units and gates (Collider2D triggers)
- [ ] Spawn additional units on collision
- [ ] Visual feedback (number popup, particle effect)

**Success criteria**: 1 unit enters ×2 gate → 2 units come out

---

### Phase 4: Enemy Base & Win Condition

**Goal**: Game has an objective

- [ ] Create enemy base at top of screen
- [ ] Base has HP that decreases when hit by units
- [ ] Units disappear after hitting base
- [ ] Win screen when base HP reaches 0

**Success criteria**: I can actually "win" a level

---

### Phase 5: Basic Game Loop

**Goal**: Playable game with restart

- [ ] Add resource limit (e.g., spawn count or timer)
- [ ] Lose condition when resources run out and base survives
- [ ] Restart button
- [ ] Basic UI (HP bar, spawn count remaining)

**Success criteria**: Complete gameplay loop from start → win/lose → restart

---

### Phase 6: Stage System

**Goal**: Multiple levels to play

- [ ] Stage data format (ScriptableObject or JSON)
- [ ] Stage 1: Tutorial (easy)
- [ ] Stage 2-5: Progressive difficulty
- [ ] Stage selection screen

**Success criteria**: Can play through 5 different stages

---

### Phase 7: Polish & Effects

**Goal**: Make it feel good to play

- [ ] Particle effects for impacts (Unity Particle System)
- [ ] Screen shake on big multiplies
- [ ] Sound effects (Unity Audio)
- [ ] Smooth animations
- [ ] Mobile build test (iOS/Android)

**Success criteria**: Friends say "this is actually fun"

---

## Technical Decisions to Make

These will be documented in separate plan files:

1. **Tech stack selection** → `01_tech_stack_selection.md`
   - Decision: Unity 6 LTS with C#

2. **Unit movement & physics** → `02_basic_unit_movement.md`
   - Unity Physics 2D, Rigidbody2D, object pooling

3. **Multiplier gates** → `03_multiplier_gates.md`
   - Gate mechanics, Collider2D triggers, unit spawning logic

4. **Enemy base** → `04_enemy_base.md`
   - Base HP system, unit collision handling

5. **Stage data format** → `05_stage_system.md`
   - How to define and load stage configurations

---

## Technical Challenges I'm Worried About

| Challenge | My Concern | Potential Solution |
|-----------|------------|-------------------|
| Performance with many units | 100+ sprites might lag | Object pooling (Unity ObjectPool), spatial hashing |
| Collision detection | N² comparisons = slow | Unity handles with Physics2D layers |
| "Game feel" | Web engineer, not game designer | Copy what Mob Control does, iterate on feedback |
| Mobile performance | Need native performance | Unity compiles to native iOS/Android |
| Scope creep | I'll want to add "one more feature" | Stick to P0, ship first |

---

## What Success Looks Like

### Minimum Viable Game (4-6 weekends)

- Playable on iOS/Android
- 5 stages
- Core loop: spawn → multiply → destroy base
- TestFlight / internal testing build

### Stretch Goals (if I don't burn out)

- App Store / Google Play release
- Leaderboard
- More unit types
- Level editor

---

## Why Plan Stack for Game Dev?

As a web developer trying game dev for the first time:

| Problem | How Plan Stack Helps |
|---------|---------------------|
| "Where do I even start?" | Phased roadmap breaks it down |
| "I don't know game architecture" | Each plan documents the decisions |
| "I forgot what I did last weekend" | Plans = persistent memory |
| "AI suggests over-engineered solutions" | Plans constrain scope |
| "I want to add ALL the features" | P0/P1/P2 prioritization |

---

## Notes

- All plans created with Claude Code's plan mode
- This is my first game, mistakes will happen
- Prioritizing "playable" over "perfect"
- Will update plans as I learn what works

---

## Inspiration & References

### Primary Reference

- **Mob Control** by Voodoo/Mambo Studio
  - [App Store](https://apps.apple.com/us/app/mob-control/id1562817072)
  - [Google Play](https://play.google.com/store/apps/details?id=com.vincentb.MobControl)
  - [MAF Analysis](https://maf.ad/en/blog/mob-control-analysis-hybrid-casual/)
  - [Gamigion Playbook](https://www.gamigion.com/from-hyper-to-hybrid-casual-mob-control-playbook/)

### Similar Games

- **Crowd City** by Voodoo - Similar crowd mechanics
- **Hole.io** by Voodoo - Simple controls, satisfying growth

---

## Design Revision History

### 2025-01-17: Core Mechanics Correction

**Problem**: After implementing Phases 1-5, feedback indicated the game was "not fun."
It was just clicking with no strategy, and the gameplay differed from Mob Control.

**Root Cause Analysis**:
- Original design had units hitting a "static enemy base"
- Re-investigation of Mob Control's actual gameplay revealed:
  - Red enemy units spawn from enemy tower and move toward player
  - Combat system: 1 Blue vs 1 Red = both destroyed (cancellation)
  - Player controls cannon direction to aim
  - Core experience is the push-and-pull of numbers

**Changes Made**:
1. Updated core loop diagram (bidirectional combat)
2. Added "cannon aiming," "red enemies," and "1v1 combat" to P0
3. Created new implementation plan: `06_core_combat_mechanics.md`

**Lessons Learned**:
- Superficial understanding of reference games leads to missing core experiences
- Even in MVP, elements essential to "fun" must not be omitted
- Plan Stack records design errors as history, preventing the same mistakes

---

*Last updated: January 2025*
*Information verified via web search*
