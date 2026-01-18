# Sound System Plan

**Created:** 2025-01-18
**Status:** Draft

## Overview

### What I Want to Build

ビジュアルエフェクトの次はサウンド。今のゲームは完全に無音で、フィードバックが視覚だけに頼っている。

サウンドがあると：
- 弾を撃った感覚が強まる
- 敵を倒した達成感が増す
- ゲーム全体の没入感が上がる

Phase 9では**コードで管理するサウンドシステム**を作る。Asset Storeの音源は使わず、まずはシステムを構築して、後から音源を差し替えられるようにする。

---

## Technical Approach

### Unity Audio System (Verified)

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

### Sound Manager Architecture

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

## Target Sounds

### Sound Effects Needed

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

### Priority

1. **弾発射・ヒット音** - 最も頻繁、ゲームフィールに直結
2. **敵撃破音** - 達成感
3. **ゲート音** - ユニット数変化の聴覚フィードバック
4. **BGM** - 雰囲気作り
5. **UI音** - 補助的

---

## Implementation Steps

### Phase 9A: Sound Manager Foundation

1. [ ] Create SoundManager singleton
2. [ ] Create AudioSource for BGM (loop)
3. [ ] Create AudioSource for SE (one-shot)
4. [ ] Create SoundLibrary ScriptableObject
5. [ ] **Test**: SoundManager.Instance.PlaySE() works

### Phase 9B: Combat Sounds

1. [ ] Add placeholder sounds (can use Unity's built-in or generate simple tones)
2. [ ] Integrate shoot sound with BulletSpawner
3. [ ] Integrate hit sound with Bullet collision
4. [ ] Integrate explosion sound with LaneEnemy death
5. [ ] **Test**: Sounds play during combat

### Phase 9C: Gate Sounds

1. [ ] Add gate pass sounds (different for each type)
   - Add: Positive chime
   - Subtract: Negative buzz
   - Multiply: Power-up sound
2. [ ] Integrate with LaneGate.ApplyEffect()
3. [ ] **Test**: Different sounds for each gate type

### Phase 9D: UI & BGM

1. [ ] Add game over sound
2. [ ] Add victory fanfare
3. [ ] Add button click sound
4. [ ] Add simple BGM loop
5. [ ] Integrate volume controls (optional)
6. [ ] **Test**: UI sounds and BGM work

### Phase 9E: Polish

1. [ ] Adjust volumes for balance
2. [ ] Add Audio Mixer for grouped control
3. [ ] Implement mute/unmute functionality
4. [ ] **Test**: Sound balance feels good

---

## Placeholder Sound Strategy

アセットを使わないので、最初は**プレースホルダー音**を使う：

| 方法 | 説明 |
|------|------|
| **Unity AudioClip** | AudioClip.Create()でコードから波形生成 |
| **無料音源サイト** | freesound.org, kenney.nl などから取得 |
| **後から差し替え** | SoundLibraryを使えば音源の差し替えが容易 |

最初はシンプルなビープ音やトーンで実装し、後から本格的な音源に差し替える。

---

## My Concerns

### Performance

- 大量の弾を発射するので、音が重なりすぎないよう注意
- 同時発音数の制限が必要かも
- AudioSource.PlayOneShot()は軽量だが、限度がある

### Volume Balance

- SE音量とBGM音量のバランス
- 頻繁に鳴る音（弾発射）は控えめに
- 重要な音（敵撃破）は目立つように

---

## Test Plan

| Test | What I Do | What Should Happen |
|------|-----------|-------------------|
| Shoot sound | Fire bullets | Shooting sound plays |
| Hit sound | Bullet hits enemy | Hit sound plays |
| Explosion sound | Kill enemy | Explosion sound plays |
| Add gate sound | Pass +N gate | Positive chime plays |
| Subtract gate sound | Pass -N gate | Negative sound plays |
| Multiply gate sound | Pass xN gate | Power-up sound plays |
| Game over sound | Collide with enemy | Game over sound plays |
| Victory sound | Survive 30 seconds | Victory fanfare plays |
| BGM | Play game | BGM loops continuously |
| Sound overlap | Fire many bullets | Sounds don't clip/distort |

---

## Definition of Done

- [ ] SoundManager singleton working
- [ ] Combat sounds (shoot, hit, explosion)
- [ ] Gate sounds (different for each type)
- [ ] UI sounds (game over, victory)
- [ ] BGM playing during gameplay
- [ ] Volume balance feels good
- [ ] No audio clipping or distortion

---

## Sources

- [Unity Learn: Add game audio](https://learn.unity.com/course/2D-adventure-robot-repair/unit/enhance-your-game/tutorial/add-game-audio?version=6.3)
- [Unity Manual: Audio overview](https://docs.unity3d.com/Manual/AudioOverview.html)
- [Unity Audio and Sound Manager Singleton - Daggerhart Lab](https://www.daggerhartlab.com/unity-audio-and-sound-manager-singleton-script/)
- [10 Unity Audio Optimisation Tips - Game Dev Beginner](https://gamedevbeginner.com/unity-audio-optimisation-tips/)
- [10 Unity Audio Tips - Game Dev Beginner](https://gamedevbeginner.com/10-unity-audio-tips-that-you-wont-find-in-the-tutorials-2/)

---

*Plan created: 2025-01-18*
*Status: Awaiting approval*
