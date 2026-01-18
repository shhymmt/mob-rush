# ビジュアルエフェクト計画

**作成日:** 2025-01-18
**ステータス:** ドラフト

## 概要

### 私が作りたいもの

Phase 7でゲームの基本メカニクスが完成した。でも今のゲームは視覚的なフィードバックが足りない。敵を倒しても「パッ」と消えるだけ、ゲートを通っても何も起きない。

参考画像（IMG_2381.jpg）を見返すと、衝突時に爆発エフェクトがある。これがあると「倒した感」が全然違う。

Phase 8では**Particle System + コード**で簡易エフェクトを追加する。アセットは使わず、プログラムで生成する方針。

---

## なぜ Particle System + コード なのか

### 私の判断

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

## 目標エフェクト

### 今足りないもの

| シーン | 現状 | 目標 |
|--------|------|------|
| 敵を倒す | 即座に消える | 爆発パーティクル |
| 弾が敵に当たる | 見た目の反応なし | ヒットスパーク |
| ゲート通過 | 見た目の反応なし | フラッシュ + 数字ポップアップ |
| ユニット増加 | 数字が変わるだけ | スケールアニメ + パーティクル |
| ユニット減少 | 数字が変わるだけ | 赤フラッシュ |

### 優先順位

1. **敵撃破エフェクト** - 最も頻繁に発生、達成感に直結
2. **ヒットエフェクト** - 弾が当たった感覚
3. **ゲート通過エフェクト** - ユニット数変化の視覚化
4. **UI演出** - 数字の変化を強調

---

## 技術的アプローチ

### 2D向けParticle System（検証済み）

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

### EffectManager アーキテクチャ

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

## 実装ステップ

### Phase 8A: エフェクトマネージャー基盤

1. [ ] EffectManager シングルトンを作成
2. [ ] ParticlePool を作成（パーティクルシステムの再利用用）
3. [ ] 基本パーティクルシステムプレファブを作成（コード生成）
4. [ ] **テスト**: EffectManager.Instance が存在する

### Phase 8B: 敵撃破エフェクト

1. [ ] 爆発パーティクルエフェクトを作成（コード生成）
   - Shape: Circle
   - Color: 赤/オレンジのグラデーション
   - パーティクル数: 10-15
   - 寿命: 0.3-0.5秒
2. [ ] LaneEnemy.OnDeath() に統合
3. [ ] 画面揺れを追加（オプション）
4. [ ] **テスト**: 敵が死んだ時に爆発が表示される

### Phase 8C: ヒットエフェクト

1. [ ] ヒットスパークパーティクルエフェクトを作成
   - Shape: Point
   - Color: 白/黄色
   - パーティクル数: 3-5
   - 寿命: 0.1-0.2秒
2. [ ] Bullet の衝突に統合
3. [ ] **テスト**: 弾が敵に当たった時にスパークが表示される

### Phase 8D: ゲートエフェクト

1. [ ] ゲートフラッシュエフェクトを作成
   - 全幅フラッシュ
   - ゲートタイプに基づく色（緑/赤/オレンジ）
   - 持続時間: 0.2秒
2. [ ] 数字ポップアップを作成（浮かぶテキスト）
   - "+5" や "x2" などを表示
   - 上に浮いてフェードアウト
3. [ ] LaneGate.ApplyEffect() に統合
4. [ ] **テスト**: ゲート通過時にフラッシュとポップアップが表示される

### Phase 8E: ユニット数変化の視覚フィードバック

1. [ ] ユニット数変化時のスケールパルス
   - 一時的に拡大して元に戻る
2. [ ] ユニット減少時の赤フラッシュ
3. [ ] ユニット増加時の緑パーティクルバースト
4. [ ] **テスト**: ユニット数変化時に視覚フィードバックがある

---

## 私の懸念

### パフォーマンス

- モバイル向けなのでパーティクル数は最小限に
- 同時に大量のエフェクトが出る可能性（複数の敵を同時に倒す）
- プーリングで対応

### ビジュアルの一貫性

- 今のゲームはシンプルな見た目（丸と四角）
- パーティクルもシンプルに保つ（複雑なテクスチャは使わない）
- 色で意味を伝える（緑=良い、赤=悪い、黄=ヒット）

---

## テスト計画

| テスト | 操作 | 期待結果 |
|--------|------|----------|
| 敵の爆発 | 敵のHPを0にする | 赤/オレンジの爆発が表示 |
| ヒットスパーク | 弾が敵に当たる | 白いスパークが表示 |
| ゲートフラッシュ（加算） | +Nゲートを通過 | 緑のフラッシュ、"+N"ポップアップ |
| ゲートフラッシュ（減算） | -Nゲートを通過 | 赤のフラッシュ、"-N"ポップアップ |
| ゲートフラッシュ（乗算） | xNゲートを通過 | オレンジのフラッシュ、"xN"ポップアップ |
| ユニット増加 | +Nゲートを通過 | プレイヤーが一時的に拡大 |
| ユニット減少 | -Nゲートを通過 | プレイヤーに赤フラッシュ |
| 複数エフェクト | 複数の敵を倒す | 全ての爆発が表示、ラグなし |

---

## 完了の定義

- [ ] 敵死亡時の爆発エフェクト
- [ ] 弾衝突時のヒットスパーク
- [ ] ゲートフラッシュエフェクト（色分け）
- [ ] ゲート通過時の数字ポップアップ
- [ ] ユニット数変化時の視覚フィードバック
- [ ] モバイルでのパフォーマンス影響なし
- [ ] エフェクトが満足感があり、レスポンシブ

---

## 参照元

- [Unity Learn: Create 2D particle effects](https://learn.unity.com/course/2D-adventure-robot-repair/unit/enhance-your-game/tutorial/create-2d-particle-effects?version=6.3)
- [Unity Manual: Particle effects](https://docs.unity3d.com/Manual/ParticleSystems.html)
- [Unity Manual: Choosing your particle system solution](https://docs.unity3d.com/Manual/ChoosingYourParticleSystem.html)
- [Unity Learn: Optimizing Particle Effects for Mobile](https://learn.unity.com/tutorial/optimizing-particle-effects-for-mobile-applications)
- [Creating 2D Particle Effects in Unity3D - Game Developer](https://www.gamedeveloper.com/design/creating-2d-particle-effects-in-unity3d)

---

*計画作成: 2025-01-18*
*ステータス: 承認待ち*
