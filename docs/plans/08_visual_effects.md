# Visual Effects Plan

**Created:** 2025-01-18
**Status:** Draft

## Overview

### What I Want to Build

Phase 7でゲームの基本メカニクスが完成した。でも今のゲームは視覚的なフィードバックが足りない。敵を倒しても「パッ」と消えるだけ、ゲートを通っても何も起きない。

参考画像（IMG_2381.jpg）を見返すと、衝突時に爆発エフェクトがある。これがあると「倒した感」が全然違う。

Phase 8では**Particle System + コード**で簡易エフェクトを追加する。アセットは使わず、プログラムで生成する方針。

---

## Why Particle System + Code

### My Decision

| 選択肢 | 判断 | 理由 |
|--------|------|------|
| Asset Store | ❌ | コスト、アートスタイルの不一致リスク |
| Visual Effect Graph | ❌ | GPUベースで大規模向け、今回はオーバースペック |
| **Built-in Particle System** | ✅ | C# APIでコントロール可能、軽量、2Dに適している |

Unityには2つのパーティクルシステムがある：
1. **Built-in Particle System** - CPU処理、C# APIで完全制御可能
2. **Visual Effect Graph** - GPU処理、大規模エフェクト向け

ハイパーカジュアルゲームでは**Built-in Particle System**で十分。コードから動的に生成・制御できるのが大きなメリット。

---

## Target Effects

### What's Missing Now

| シーン | 現状 | 目標 |
|--------|------|------|
| 敵を倒す | 即座に消える | 爆発パーティクル |
| 弾が敵に当たる | 見た目の反応なし | ヒットスパーク |
| ゲート通過 | 見た目の反応なし | フラッシュ + 数字ポップアップ |
| ユニット増加 | 数字が変わるだけ | スケールアニメ + パーティクル |
| ユニット減少 | 数字が変わるだけ | 赤フラッシュ |

### Priority

1. **敵撃破エフェクト** - 最も頻繁に発生、達成感に直結
2. **ヒットエフェクト** - 弾が当たった感覚
3. **ゲート通過エフェクト** - ユニット数変化の視覚化
4. **UI演出** - 数字の変化を強調

---

## Technical Approach

### Particle System for 2D (Verified)

Unity公式ドキュメントおよびベストプラクティスによると：

**2D向けの設定:**
- Shape: Box（Z=0）、Circle、Edge が有効
- Render Mode: Stretched Billboard でスピード感を表現可能
- Sorting Layer: 2D用のSorting Layerを使用して描画順を制御

**モバイル最適化:**
- パーティクル数を制限
- シンプルな形状を使用
- Emission Rateを抑える
- 画面外のパーティクルはCulling

**停止方法:**
- `Destroy()`ではなく`Stop()`を使う
- `Stop()`は新しいパーティクルの生成を停止し、既存のパーティクルは自然に消える

### Effect Manager Architecture

```
EffectManager (Singleton)
├── CreateExplosion(position, color)
├── CreateHitSpark(position)
├── CreateGateFlash(position, gateType)
└── CreateNumberPopup(position, text, color)

ParticlePool
├── Pool<ParticleSystem> explosions
├── Pool<ParticleSystem> hitSparks
└── Pool<ParticleSystem> flashes
```

プーリングで毎回Instantiateを避ける。

---

## Implementation Steps

### Phase 8A: Effect Manager Foundation

1. [ ] Create EffectManager singleton
2. [ ] Create ParticlePool for reusing particle systems
3. [ ] Create base particle system prefab (code-generated)
4. [ ] **Test**: EffectManager.Instance exists

### Phase 8B: Enemy Destruction Effect

1. [ ] Create explosion particle effect (code-generated)
   - Shape: Circle
   - Color: Red/Orange gradient
   - Particles: 10-15
   - Lifetime: 0.3-0.5s
2. [ ] Integrate with LaneEnemy.OnDeath()
3. [ ] Add screen shake (optional)
4. [ ] **Test**: Explosion appears when enemy dies

### Phase 8C: Hit Effect

1. [ ] Create hit spark particle effect
   - Shape: Point
   - Color: White/Yellow
   - Particles: 3-5
   - Lifetime: 0.1-0.2s
2. [ ] Integrate with Bullet collision
3. [ ] **Test**: Spark appears when bullet hits enemy

### Phase 8D: Gate Effect

1. [ ] Create gate flash effect
   - Full-width flash
   - Color based on gate type (Green/Red/Orange)
   - Duration: 0.2s
2. [ ] Create number popup (floating text)
   - Shows "+5" or "x2" etc
   - Floats up and fades
3. [ ] Integrate with LaneGate.ApplyEffect()
4. [ ] **Test**: Flash and popup appear when passing gate

### Phase 8E: Unit Count Visual Feedback

1. [ ] Player unit scale pulse on count change
   - Scale up briefly, then return
2. [ ] Red flash when units decrease
3. [ ] Green particle burst when units increase
4. [ ] **Test**: Visual feedback on unit count changes

---

## My Concerns

### Performance

- モバイル向けなのでパーティクル数は最小限に
- 同時に大量のエフェクトが出る可能性（複数の敵を同時に倒す）
- プーリングで対応

### Visual Consistency

- 今のゲームはシンプルな見た目（丸と四角）
- パーティクルもシンプルに保つ（複雑なテクスチャは使わない）
- 色で意味を伝える（緑=良い、赤=悪い、黄=ヒット）

---

## Test Plan

| Test | What I Do | What Should Happen |
|------|-----------|-------------------|
| Enemy explosion | Shoot enemy until HP=0 | Red/Orange explosion appears |
| Hit spark | Bullet hits enemy | White spark appears |
| Gate flash (Add) | Pass through +N gate | Green flash, "+N" popup |
| Gate flash (Subtract) | Pass through -N gate | Red flash, "-N" popup |
| Gate flash (Multiply) | Pass through xN gate | Orange flash, "xN" popup |
| Unit increase | Pass through +N gate | Player scales up briefly |
| Unit decrease | Pass through -N gate | Red flash on player |
| Multiple effects | Kill multiple enemies | All explosions appear, no lag |

---

## Definition of Done

- [ ] Explosion effect on enemy death
- [ ] Hit spark on bullet collision
- [ ] Gate flash effect with color coding
- [ ] Number popup on gate pass
- [ ] Visual feedback on unit count change
- [ ] No noticeable performance impact on mobile
- [ ] Effects feel satisfying and responsive

---

## Sources

- [Unity Learn: Create 2D particle effects](https://learn.unity.com/course/2D-adventure-robot-repair/unit/enhance-your-game/tutorial/create-2d-particle-effects?version=6.3)
- [Unity Manual: Particle effects](https://docs.unity3d.com/Manual/ParticleSystems.html)
- [Unity Manual: Choosing your particle system solution](https://docs.unity3d.com/Manual/ChoosingYourParticleSystem.html)
- [Unity Learn: Optimizing Particle Effects for Mobile](https://learn.unity.com/tutorial/optimizing-particle-effects-for-mobile-applications)
- [Creating 2D Particle Effects in Unity3D - Game Developer](https://www.gamedeveloper.com/design/creating-2d-particle-effects-in-unity3d)

---

*Plan created: 2025-01-18*
*Status: Awaiting approval*
