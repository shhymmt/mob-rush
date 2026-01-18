# サウンドシステム計画

**作成日:** 2025-01-18
**ステータス:** ドラフト

## 概要

### 私が作りたいもの

ビジュアルエフェクトの次はサウンド。今のゲームは完全に無音で、フィードバックが視覚だけに頼っている。

サウンドがあると：
- 弾を撃った感覚が強まる
- 敵を倒した達成感が増す
- ゲーム全体の没入感が上がる

Phase 9では**コードで管理するサウンドシステム**を作る。Asset Storeの音源は使わず、まずはシステムを構築して、後から音源を差し替えられるようにする。

---

## 技術的アプローチ

### Unity Audio System（検証済み）

Unity公式ドキュメントによると：

**2Dサウンドの設定:**
- **Spatial Blend = 2D** に設定すると、AudioListenerの位置に関係なく同じ音量で再生
- 2Dゲームでは基本的にSpatial Blend = 0（完全2D）を使用

**効果音の再生:**
- **PlayOneShot()** を使用すると、同じAudioSourceから複数の音を重ねて再生可能
- 一度きりの効果音（爆発、ヒットなど）に最適

**メモリ最適化:**
- 2Dサウンドではステレオの意味がない
- **Force to Mono** を有効にしてメモリを半減
- ステレオクリップはモノラルの2倍のメモリを使用

**Audio Mixer:**
- BGM、SE、UIなどをグループ分けして音量調整
- エフェクト（リバーブ、フィルターなど）を適用可能

### SoundManager アーキテクチャ

```
SoundManager (Singleton)
├── AudioSource bgmSource
├── AudioSource seSource
├── AudioMixer masterMixer
│
├── PlayBGM(clip)
├── StopBGM()
├── PlaySE(clip)
├── PlaySE(clipName)  // 名前で再生
│
├── SetBGMVolume(0-1)
├── SetSEVolume(0-1)
└── SetMasterVolume(0-1)

SoundLibrary (ScriptableObject)
├── AudioClip[] shootSounds
├── AudioClip[] hitSounds
├── AudioClip[] explosionSounds
├── AudioClip[] gateSounds
├── AudioClip[] uiSounds
└── AudioClip[] bgmTracks
```

---

## 目標サウンド

### 必要な効果音

| カテゴリ | サウンド | 発生タイミング |
|----------|----------|----------------|
| **Combat** | 弾発射 | BulletSpawner.FireBullets() |
| | 弾ヒット | Bullet.OnTriggerEnter2D() |
| | 敵撃破 | LaneEnemy.OnDeath() |
| **Gate** | 加算ゲート | LaneGate.ApplyEffect() (+N) |
| | 減算ゲート | LaneGate.ApplyEffect() (-N) |
| | 乗算ゲート | LaneGate.ApplyEffect() (xN) |
| **UI** | ゲームオーバー | GameManager.TriggerLose() |
| | ステージクリア | GameManager.TriggerWin() |
| | ボタンクリック | UI Button |
| **BGM** | ゲームプレイ | ステージ中 |
| | リザルト | GAME OVER / VICTORY画面 |

### 優先順位

1. **弾発射・ヒット音** - 最も頻繁、ゲームフィールに直結
2. **敵撃破音** - 達成感
3. **ゲート音** - ユニット数変化の聴覚フィードバック
4. **BGM** - 雰囲気作り
5. **UI音** - 補助的

---

## 実装ステップ

### Phase 9A: サウンドマネージャー基盤

1. [ ] SoundManager シングルトンを作成
2. [ ] BGM用AudioSourceを作成（ループ）
3. [ ] SE用AudioSourceを作成（ワンショット）
4. [ ] SoundLibrary ScriptableObjectを作成
5. [ ] **テスト**: SoundManager.Instance.PlaySE() が動作する

### Phase 9B: 戦闘サウンド

1. [ ] プレースホルダー音を追加
2. [ ] 弾発射音をBulletSpawnerに統合
3. [ ] ヒット音をBullet衝突に統合
4. [ ] 爆発音をLaneEnemy死亡に統合
5. [ ] **テスト**: 戦闘中にサウンドが再生される

### Phase 9C: ゲートサウンド

1. [ ] ゲート通過音を追加（タイプごとに異なる）
   - 加算: ポジティブなチャイム
   - 減算: ネガティブなブザー
   - 乗算: パワーアップ音
2. [ ] LaneGate.ApplyEffect()に統合
3. [ ] **テスト**: ゲートタイプごとに異なる音が鳴る

### Phase 9D: UI & BGM

1. [ ] ゲームオーバー音を追加
2. [ ] 勝利ファンファーレを追加
3. [ ] ボタンクリック音を追加
4. [ ] シンプルなBGMループを追加
5. [ ] 音量コントロールを実装（オプション）
6. [ ] **テスト**: UI音とBGMが動作する

### Phase 9E: 仕上げ

1. [ ] 音量バランスを調整
2. [ ] Audio Mixerでグループ制御を追加
3. [ ] ミュート/ミュート解除機能を実装
4. [ ] **テスト**: サウンドバランスが良い

---

## プレースホルダー音の戦略

アセットを使わないので、最初は**プレースホルダー音**を使う：

| 方法 | 説明 |
|------|------|
| **Unity AudioClip** | AudioClip.Create()でコードから波形生成 |
| **無料音源サイト** | freesound.org, kenney.nl などから取得 |
| **後から差し替え** | SoundLibraryを使えば音源の差し替えが容易 |

最初はシンプルなビープ音やトーンで実装し、後から本格的な音源に差し替える。

---

## 私の懸念

### パフォーマンス

- 大量の弾を発射するので、音が重なりすぎないよう注意
- 同時発音数の制限が必要かも
- AudioSource.PlayOneShot()は軽量だが、限度がある

### 音量バランス

- SE音量とBGM音量のバランス
- 頻繁に鳴る音（弾発射）は控えめに
- 重要な音（敵撃破）は目立つように

---

## テスト計画

| テスト | 操作 | 期待結果 |
|--------|------|----------|
| 発射音 | 弾を発射 | 発射音が鳴る |
| ヒット音 | 弾が敵に当たる | ヒット音が鳴る |
| 爆発音 | 敵を倒す | 爆発音が鳴る |
| 加算ゲート音 | +Nゲートを通過 | ポジティブな音が鳴る |
| 減算ゲート音 | -Nゲートを通過 | ネガティブな音が鳴る |
| 乗算ゲート音 | xNゲートを通過 | パワーアップ音が鳴る |
| ゲームオーバー音 | 敵に衝突 | ゲームオーバー音が鳴る |
| 勝利音 | 30秒生存 | 勝利ファンファーレが鳴る |
| BGM | ゲームをプレイ | BGMがループ再生 |
| 音の重なり | 大量の弾を発射 | 音がクリップ/歪まない |

---

## 完了の定義

- [ ] SoundManager シングルトンが動作
- [ ] 戦闘サウンド（発射、ヒット、爆発）
- [ ] ゲートサウンド（タイプごとに異なる）
- [ ] UIサウンド（ゲームオーバー、勝利）
- [ ] ゲームプレイ中のBGM
- [ ] 音量バランスが良い
- [ ] オーディオのクリップや歪みなし

---

## 参照元

- [Unity Learn: Add game audio](https://learn.unity.com/course/2D-adventure-robot-repair/unit/enhance-your-game/tutorial/add-game-audio?version=6.3)
- [Unity Manual: Audio overview](https://docs.unity3d.com/Manual/AudioOverview.html)
- [Unity Audio and Sound Manager Singleton - Daggerhart Lab](https://www.daggerhartlab.com/unity-audio-and-sound-manager-singleton-script/)
- [10 Unity Audio Optimisation Tips - Game Dev Beginner](https://gamedevbeginner.com/unity-audio-optimisation-tips/)
- [10 Unity Audio Tips - Game Dev Beginner](https://gamedevbeginner.com/10-unity-audio-tips-that-you-wont-find-in-the-tutorials-2/)

---

*計画作成: 2025-01-18*
*ステータス: 承認待ち*
