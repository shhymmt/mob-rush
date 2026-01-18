# Boss Battle Plan

**Created:** 2025-01-18
**Status:** Draft

## Overview

### What I Want to Build

参考画像（IMG_2381.jpg）を見ると、左側に巨大なボスがいてHP 1338と表示されている。これがゲームの目玉になりそう。

Phase 10では**ステージ終盤に登場するボス**を実装する。ハイパーカジュアルゲームなので、複雑なパターンは避けてシンプルに。

---

## Boss Battle Design Philosophy

### Hyper-Casual Principles

ハイパーカジュアルゲームのボス戦で守るべき原則：

| 原則 | 説明 |
|------|------|
| **シンプル** | 3-5秒で理解できるメカニクス |
| **短い** | 30秒〜1分で決着 |
| **達成感** | 大きなHPバーが減っていく満足感 |
| **公平** | パターンは固定で予測可能 |

### Boss Design Pattern (Verified)

ゲームデザインのベストプラクティスによると、ボスのパターンは2種類：

1. **Fixed Pattern** - 決まったパターンを繰り返す（予測可能）
2. **Random Pattern** - ランダムに攻撃を選択（難しい）

ハイパーカジュアルでは**Fixed Pattern**が適切。プレイヤーがパターンを覚えて上達できる。

---

## Target Boss Design

### Reference: IMG_2381.jpg

参考画像のボス：
- 画面左側に固定表示
- 大きなHP表示（1338）
- ターゲットマーク付き
- 赤いレーザー/弾を撃ってくる

### My Boss Design

| 要素 | 設計 |
|------|------|
| **位置** | 画面上部に固定（通常の敵より大きい） |
| **HP** | ステージレベル × 基本HP（例: Stage 3 = 150 HP） |
| **表示** | HPバー + 数値 |
| **攻撃** | 弾を撃ってくる（プレイヤーはレーン移動で回避） |
| **出現** | ステージ終盤（残り10秒くらい）に出現 |
| **倒し方** | 通常通り弾を当てる |
| **報酬** | 倒すとボーナスユニット or ステージクリア |

### Boss Attack Pattern (Fixed)

```
Phase 1: "Business as Usual"
├── 3秒間隔で弾を1発撃つ
├── 弾はランダムなレーンに向かう
└── プレイヤーはレーン移動で回避

Phase 2: "Intensity Ramp" (HP 50%以下)
├── 2秒間隔で弾を2発撃つ（両レーン）
├── 片方のレーンだけ安全
└── タイミングを見て移動
```

シンプルに2フェーズ。プレイヤーが混乱しないように。

---

## Technical Approach

### Boss Architecture

```
LaneBoss : MonoBehaviour
├── Health bossHealth
├── BossAttackPattern attackPattern
├── BossUI bossUI
│
├── Initialize(hp, pattern)
├── TakeDamage(amount)
├── StartAttackPattern()
├── OnDeath()
└── OnPhaseChange(phase)

BossSpawner : MonoBehaviour
├── SpawnBoss(stageNumber)
├── GetBossForStage(stage)
└── OnBossDefeated()

BossProjectile : MonoBehaviour
├── Initialize(targetLane, speed)
├── OnHitPlayer()
└── OnMissed()
```

### Integration with Existing Systems

| システム | 統合方法 |
|----------|----------|
| **StageManager** | ステージ終盤にボススポーンをトリガー |
| **GameManager** | ボス撃破で勝利 or ボーナス |
| **Bullet** | 既存の弾システムでボスにダメージ |
| **PlayerUnitGroup** | ボス弾との衝突でダメージ/ゲームオーバー |
| **EffectManager** | ボス撃破時の大爆発エフェクト |
| **SoundManager** | ボス出現音、ボス撃破音 |

---

## Implementation Steps

### Phase 10A: Boss Foundation

1. [ ] Create LaneBoss component
2. [ ] Create boss visual (larger enemy sprite)
3. [ ] Add HP bar UI for boss
4. [ ] Position boss at top of screen
5. [ ] **Test**: Boss appears and displays HP

### Phase 10B: Boss Takes Damage

1. [ ] Integrate with existing Bullet system
2. [ ] HP bar updates on damage
3. [ ] Add hit effect on boss
4. [ ] Boss death triggers victory or bonus
5. [ ] **Test**: Shooting boss reduces HP, death works

### Phase 10C: Boss Attack Pattern

1. [ ] Create BossProjectile (moves down toward player)
2. [ ] Implement Phase 1 attack (single shot)
3. [ ] Implement Phase 2 attack (double shot at HP 50%)
4. [ ] Player collision with projectile = damage or game over
5. [ ] **Test**: Boss shoots, player can dodge by lane switching

### Phase 10D: Boss Spawning

1. [ ] Create BossSpawner
2. [ ] Integrate with StageManager (spawn at stage end)
3. [ ] Stop normal enemy spawning when boss appears
4. [ ] Configure boss HP per stage
5. [ ] **Test**: Boss spawns at correct time per stage

### Phase 10E: Boss Effects & Sound

1. [ ] Boss entrance effect (screen shake, flash)
2. [ ] Boss death explosion (big effect)
3. [ ] Boss BGM change (optional)
4. [ ] Boss sounds (attack, hit, death)
5. [ ] **Test**: Boss feels impactful

---

## Boss Per Stage

| Stage | Boss HP | Attack Speed | Notes |
|-------|---------|--------------|-------|
| 1 | 50 | 3秒 | チュートリアル、倒しやすい |
| 2 | 100 | 2.5秒 | 少し強い |
| 3 | 150 | 2秒 | 本格的 |
| 4+ | 150 + (stage-3)*50 | 1.5秒 | エンドレス用 |

---

## My Concerns

### Difficulty Balance

- ボスが強すぎるとフラストレーション
- 弱すぎると達成感がない
- ユニット数がボス到達時にどれくらいか次第

### Visual Clarity

- ボスの弾とプレイヤーの弾を区別できるか
- ボスが大きすぎて画面を圧迫しないか
- HPバーが見やすいか

### Performance

- ボス + 大量の弾 + エフェクト = 負荷が心配
- ボス弾のプーリングが必要

---

## Test Plan

| Test | What I Do | What Should Happen |
|------|-----------|-------------------|
| Boss appears | Wait until stage end | Boss appears at top |
| Boss HP display | Boss appears | HP bar shows correct HP |
| Damage boss | Shoot boss | HP decreases, bar updates |
| Kill boss | Reduce HP to 0 | Boss explodes, victory |
| Boss attack P1 | Boss attacks | Single projectile fires |
| Boss attack P2 | Damage boss to 50% HP | Double projectile fires |
| Dodge attack | Lane switch when attack comes | Avoid damage |
| Hit by attack | Stay in attack lane | Game over or unit loss |
| Boss per stage | Play different stages | Different boss HP |

---

## Definition of Done

- [ ] Boss appears at stage end
- [ ] Boss has HP bar with visual feedback
- [ ] Player bullets damage boss
- [ ] Boss death triggers victory
- [ ] Boss attacks with projectiles
- [ ] Player can dodge by lane switching
- [ ] Phase 2 activates at 50% HP
- [ ] Boss HP scales with stage
- [ ] Boss effects and sounds (if Phase 8-9 complete)

---

## Sources

- [Boss Battle Design and Structure - Game Developer](https://www.gamedeveloper.com/design/boss-battle-design-and-structure)
- [The Evolution of Boss Designs in Video Games - Game Developer](https://www.gamedeveloper.com/design/the-evolution-of-boss-designs-in-video-games)
- [The Ultimate Guide to Hyper Casual Game Design - Moloco](https://www.moloco.com/blog/hyper-casual-games-design)
- [Top 10 Hyper-Casual Game Mechanics - EJAW](https://ejaw.net/top-10-hyper-casual-mechanics/)

---

*Plan created: 2025-01-18*
*Status: Awaiting approval*
