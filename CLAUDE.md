# CLAUDE.md - Mob Rush

## Project Overview

Mob Rush is a hyper-casual mobile game sample built with Unity 6 LTS, demonstrating the Plan Stack methodology for game development.

## [約束] Convention

**重要**: ユーザーが `[約束]` というプレフィックスを付けて指示した内容は、必ずこのCLAUDE.mdに追記すること。

これにより：
- セッションが変わっても指示が永続化される
- compactが行われても情報が失われない
- ユーザーが同じ指示を繰り返す必要がなくなる

`[約束]` の内容は、このファイルの適切なセクションに追記するか、新しいセクションを作成して記録する。

---

## Information Verification Policy

**All information must be verified via WebSearch before being shared or documented.**

This includes:
- Game engine/framework versions
- Library and package versions
- Technical specifications
- Reference game analysis (e.g., Mob Control mechanics)
- Best practices and recommended approaches
- API changes and deprecations

Do NOT rely solely on training data. Always confirm current information before:
- Creating or updating planning documents
- Providing technical recommendations
- Analyzing reference games or competitors
- Suggesting implementation approaches

## Tech Stack

- **Engine**: Unity 6 LTS (latest: 6.3 LTS)
- **Language**: C#
- **Target Platforms**: iOS, Android

## Project Structure

```
samples/mob-rush/
├── docs/
│   └── plans/           # Planning documents (Plan Stack methodology)
│       ├── 00_game_design_doc.md
│       ├── 01_tech_stack_selection.md
│       ├── 02_basic_unit_movement.md
│       ├── 03_multiplier_gates.md
│       └── ja/          # Japanese versions
├── Assets/              # Unity project assets (to be created)
└── CLAUDE.md            # This file
```

## Planning Documents

All planning documents are in `docs/plans/`. Read these before implementing features:

### Documentation Style Guide

**重要**: すべての計画ドキュメントは**Kさん（開発者）の一人称視点**で書く。

- ✅ 正しい: 「What I Want to Build」「My concern」「Why I chose this」
- ❌ 間違い: 「K-san wants」「The developer needs」

00_game_design_doc.md のスタイルを参照。リアルな開発者の思考プロセスとして書く。
- `00_game_design_doc.md` - Overall game design and core mechanics
- `01_tech_stack_selection.md` - Technology choices and rationale
- `02_basic_unit_movement.md` - Unit spawning and movement system
- `03_multiplier_gates.md` - Gate multiplication mechanics
- `04_enemy_base.md` - Enemy base and Health system
- `05_basic_game_loop.md` - GameManager, win/lose detection
- `06_core_combat_mechanics.md` - Cannon aiming, red enemies, 1v1 combat (Mob Control style - superseded)
- `07_lane_runner_mechanics.md` - ✅ Lane runner redesign (2 lanes, auto-shooting, HP-based combat)
- `08_visual_effects.md` - **NEW**: Visual effects (Particle System + code)
- `09_sound_system.md` - **NEW**: Sound system (SoundManager + AudioSource)
- `10_boss_battle.md` - **NEW**: Boss battle (Fixed pattern, HP scaling)

## Unity 6 API/UI Changes (Verified)

Unity 6では従来のUnityから多くの変更があります。以下は検証済みの変更点：

### メニュー変更
- **Build Settings** → **Build Profiles** (`File > Build Profiles`)

### スクリプトAPI変更
- `rb.velocity` → `rb.linearVelocity` (Rigidbody2D)
- `FindObjectOfType<T>()` → `FindFirstObjectByType<T>()`
- `FindObjectsOfType<T>()` → `FindObjectsByType<T>(FindObjectsSortMode.None)`

### Input System
- Unity 6はデフォルトで新しいInput Systemを使用
- `Input.GetMouseButton()` → `Mouse.current.leftButton.isPressed`
- `using UnityEngine.InputSystem;` が必要

### UI
- Rect Transform の Anchor Presets は従来通り使用可能
- Sprite Renderer の展開（▶クリック）で Color などの項目が表示される
- Order in Layer でスプライトの前後関係を制御

---

## Mob Control Research (Reference Game)

**Last Updated:** 2025-01-17

### Game Overview

| Item | Details |
|------|---------|
| Developer | VOODOO (Mambo Studio) |
| Platforms | iOS, Android, Steam, Web |
| Release | April 2021 (Mobile), May 2024 (Steam) |
| Downloads | 250+ million |

### Core Mechanics (Verified)

#### 1. Cannon Control
- Player drags/swipes to aim cannon direction
- Tap/hold to continuously fire blue units
- Units fire in the aimed direction

#### 2. Unit Types

| Type | Color | Behavior |
|------|-------|----------|
| Player Unit | Blue | Moves forward toward enemy base |
| Enemy Unit | Red | Moves toward player's cannon |
| Giant/Champion | Yellow | Charged by holding fire, stronger |

#### 3. Combat: 1:1 Cancellation
> "Every blue stickman has the power to eliminate a red one, and vice versa."

- Blue + Red collision = both destroyed
- Pure numerical battle - more units wins

#### 4. Gates

| Gate Type | Effect |
|-----------|--------|
| Multiply (x2, x3, x5) | Multiplies passing units |
| Add (+N) | Adds N units |
| Red Gate | Subtracts/destroys units |

#### 5. Win/Lose Conditions

- **Win:** Blue units destroy enemy base
- **Lose:** ANY red unit reaches cannon = immediate game over

> "The player loses the entire match if any red stickmen crosses the defense line near the cannon."

### Mob Rush Implementation Status

| Feature | Mob Control | Mob Rush |
|---------|------------|----------|
| Cannon aiming | ✅ | ✅ Done |
| 1:1 cancellation | ✅ | ✅ Done |
| Gates multiply | ✅ | ✅ Done |
| Red enemies spawn | ✅ | ✅ Done |
| Lose on enemy reaching cannon | ✅ | ✅ Done |
| Giants/Champions | ✅ | ❌ Not yet |
| Red gates (subtract) | ✅ | ❌ Not yet |
| Multiple enemy directions | ✅ | ❌ Not yet |

### Sources
- [Mob Control - SilverGames](https://www.silvergames.com/en/mob-control)
- [Mob Control - Steam](https://store.steampowered.com/app/2736490/Mob_Control/)
- [Mob Control - App Store](https://apps.apple.com/us/app/mob-control/id1562817072)
- [Deconstruction of Mob Control - PlayDucky](https://playducky.com/recentpress/deconstruction-of-mob-control)

---

## Reference Images Analysis (2025-01-17)

### Mob Control App Store Screenshots

**Screenshot 1**: ゲート配置
- x2, x3, x4 ゲートが縦に配置
- 上部に大量の赤ユニット
- 黄色の特殊ユニット（チャンピオン）

**Screenshot 2**: 爽快スマッシュ
- x2, x8 ゲートを通過
- +300 表示（加算ゲート効果）

**Screenshot 3**: 賢く選べ！
- 青と赤の大規模戦闘
- 複数ゲート（x4, x2, x2, x4）
- 数百体のユニットが戦闘

**Screenshot 4**: 壮大なバトル
- **ボス戦**: 巨大な赤い敵キャラ
- HP表示（8/25）
- ハートアイコン（ライフ）

**Screenshot 5**: 自分の基地を築け
- 基地建設モード（メタゲーム）

### K-san's Target Game Image

K-sanが目指すゲームの参考画像（IMG_2381.jpg）:

| 要素 | 詳細 |
|------|------|
| レイアウト | 一本道が奥に伸びる縦長視点 |
| プレイヤーユニット | 青い兵士（3体）が前進 |
| 敵ユニット | 赤い兵士が上から来る |
| **ボス** | 左側に巨大キャラ、ターゲットマーク、HP **1338** |
| **赤ゲート** | **-10** 表示（ユニットを減らす） |
| 戦闘エフェクト | 衝突時に爆発 |
| 敵の攻撃 | 赤いレーザー/弾が飛んでくる |

### Mob Rush 未実装機能（優先度順）

1. **赤ゲート（減算）** - ユニットを減らすゲート
2. **ボス戦** - 巨大な敵（HP付き）
3. **敵の遠距離攻撃** - レーザー/弾
4. **Giants/Champions** - 強力な特殊ユニット
5. **視覚エフェクト** - 爆発、パーティクル

---

## Mob Rush 新ゲームデザイン（2025-01-17 確定）

**重要**: Phase 6までの実装（Mob Control型）から、以下の新デザインに移行する。

### ゲーム構造

```
      ← スクロール方向（敵/ゲートが下に移動）

      レーン1         レーン2
         │               │
         ▼               ▼
       [+10]           [-5]      ← ゲートが下に流れてくる
         │               │
         ▼               ▼
       [敵HP:50]       [敵HP:30]  ← 敵/障害物が下に流れてくる
         │               │
         │      ↑弾      │        ← ユニットが自動で弾を発射
         │               │
   ┌─────────────────────────┐
   │     [青ユニット x5]      │    ← ユニット（レーン間を移動）
   └─────────────────────────┘
```

### 確定仕様

| 要素 | 仕様 |
|------|------|
| レーン数 | **2本**（最初はシンプルに） |
| スクロール | 敵/ゲートが**下に**移動してくる |
| ユニット | 全ユニットが**一緒に移動** |
| 弾の発射 | **移動中は発射停止、移動完了後に自動発射** |
| 操作（モバイル） | **レーンをタップで移動** |
| 操作（PC） | **マウスクリックで移動** |
| ゲート種類 | +N（加算）、-N（減算）、xN（乗算） |
| 敵/障害物 | HPあり、弾でダメージ |
| ゲームオーバー | **ユニット数0** または **敵との衝突** |
| 初期状態 | **3体**でスタート |
| 弾のダメージ | **1発 = 1ダメージ**、ユニット数 × 1（3体なら3ダメージ/発射） |
| スクロール速度 | **ステージレベルで調整**（Stage 1はチュートリアル） |

### Mob Control との違い

| 項目 | Mob Control | Mob Rush（新） |
|------|-------------|---------------|
| カメラ | 固定 | スクロール |
| 戦闘 | 1:1相殺 | 弾で敵HPを削る |
| ユニット操作 | キャノンの向き | レーン移動 |
| 敵の動き | プレイヤーに向かう | 下にスクロール |
| ゲームオーバー | 敵がキャノン到達 | 衝突 or ユニット0 |

### 実装ステータス

| Phase | 内容 | 状態 |
|-------|------|------|
| Phase 1-5 | 基本ゲームループ | ✅ 完了 |
| Phase 6 | Mob Control型（キャノン、1:1相殺） | ✅ 完了したが **目標のゲームではない** |
| Phase 7 | レーンランナー型（このデザイン） | ✅ **完了（2025-01-18）** |
| Phase 8 | ビジュアルエフェクト | 📋 計画済み |
| Phase 9 | サウンドシステム | 📋 計画済み |
| Phase 10 | ボス戦 | 📋 計画済み |

**注記**: Phase 6は正しく実装されたが、ゲームデザインの誤解に基づいていた。Phase 7でK-sanの実際のビジョンに合わせて再設計した。
