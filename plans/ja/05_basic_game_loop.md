# 基本ゲームループ計画

## 目標

コアゲームループを完成させる：リソース管理、敗北条件、勝敗UI画面、リスタート機能。これによりプロトタイプがプレイ可能なゲームに変わる。

---

## 私が作るもの

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│   ┌─────────────────────────────────────────────────────┐   │
│   │              ゲームループ                           │   │
│   │                                                     │   │
│   │   ┌──────────┐    ┌──────────┐    ┌──────────┐     │   │
│   │   │  ゲーム  │───▶│  プレイ  │───▶│  終了    │     │   │
│   │   │  開始    │    │   中     │    │  画面    │     │   │
│   │   └──────────┘    └────┬─────┘    └────┬─────┘     │   │
│   │                        │               │           │   │
│   │                   ┌────┴────┐          │           │   │
│   │                   ▼         ▼          │           │   │
│   │              [勝利]      [敗北]        │           │   │
│   │                   │         │          │           │   │
│   │                   └────┬────┘          │           │   │
│   │                        ▼               │           │   │
│   │                   [リスタート]◀────────┘           │   │
│   │                                                     │   │
│   └─────────────────────────────────────────────────────┘   │
│                                                             │
│   勝利:  すべての敵基地を破壊                              │
│   敗北:  生成リソースが尽きる                              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 参考: ハイパーカジュアルゲームループ

[Unity Learn - Core Game Loop](https://learn.unity.com/course/2D-adventure-robot-repair/unit/game-ui-and-game-loop/tutorial/close-the-core-game-loop)による:

| 要素 | 実装 |
|------|------|
| 明確な目標 | 敵基地を破壊（HPバーで表示） |
| リソース制限 | 限られた生成数またはタイマー |
| 勝利条件 | すべての基地が破壊された |
| 敗北条件 | リソースが尽き、基地が残っている |
| クイックリスタート | ワンタップリスタート、最小の摩擦 |
| フィードバック | 明確な勝敗画面 |

---

## ゲーム状態フロー

```
┌────────────────────────────────────────────────────────┐
│                                                        │
│  GameState.Playing                                     │
│  ├── ユニット生成中（リソースが減少）                  │
│  ├── ユニットがゲートにヒット（増殖）                  │
│  └── ユニットが基地にヒット（ダメージ）                │
│       │                                               │
│       ├── 基地HP = 0 ──────▶ GameState.Won           │
│       │                                               │
│       └── リソース = 0 ────▶ 全ユニットを待機         │
│           （これ以上生成不可）   │                    │
│                                  ▼                    │
│                          全ユニットがdespawn?         │
│                          ├── 基地が生存 ──▶ GameState.Lost
│                          └── 基地が破壊 ───▶ GameState.Won
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## Unityセットアップ

### UI構造

```
UICanvas (Screen Space - Overlay)
├── GameHUD
│   ├── SpawnCounter (TextMeshPro - "ユニット: 50")
│   └── EnemyHPBar (オプション、または基地上のワールドスペース使用)
├── WinScreen (デフォルトで無効)
│   ├── Background (Image - 半透明)
│   ├── WinText (TextMeshPro - "勝利！")
│   ├── StarsDisplay (オプション)
│   └── RestartButton (Button)
└── LoseScreen (デフォルトで無効)
    ├── Background (Image - 半透明)
    ├── LoseText (TextMeshPro - "敗北")
    └── RestartButton (Button)
```

---

## 実装ステップ

### ステップ1: ゲーム状態を定義

```csharp
// Assets/Scripts/Core/GameState.cs
public enum GameState
{
    Playing,
    Won,
    Lost,
    Paused
}
```

---

### ステップ2: GameManagerを拡張

```csharp
// Assets/Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingSpawnCount = 50;

    [Header("References")]
    [SerializeField] private UnitSpawner unitSpawner;

    [Header("Events")]
    public UnityEvent OnGameWin;
    public UnityEvent OnGameLose;
    public UnityEvent<int> OnSpawnCountChanged; // 残り数
    public UnityEvent<GameState> OnGameStateChanged;

    private GameState currentState = GameState.Playing;
    private int remainingSpawns;
    private EnemyBase[] enemyBases;
    private int destroyedBases = 0;

    public GameState CurrentState => currentState;
    public int RemainingSpawns => remainingSpawns;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        currentState = GameState.Playing;
        remainingSpawns = startingSpawnCount;
        destroyedBases = 0;

        // すべての敵基地を検索
        enemyBases = FindObjectsOfType<EnemyBase>();

        // UIに通知
        OnSpawnCountChanged?.Invoke(remainingSpawns);
        OnGameStateChanged?.Invoke(currentState);

        // 時間を再開（一時停止していた場合）
        Time.timeScale = 1f;
    }

    // ユニットが生成されたときにUnitSpawnerから呼ばれる
    public bool TryConsumeSpawn()
    {
        if (currentState != GameState.Playing) return false;
        if (remainingSpawns <= 0) return false;

        remainingSpawns--;
        OnSpawnCountChanged?.Invoke(remainingSpawns);

        // 生成数が尽きたかチェック
        if (remainingSpawns <= 0)
        {
            // 敗北条件のチェックを開始
            StartCoroutine(CheckForLoseCondition());
        }

        return true;
    }

    private System.Collections.IEnumerator CheckForLoseCondition()
    {
        // すべてのアクティブユニットが基地にヒットまたはdespawnするのを待つ
        yield return new WaitForSeconds(0.5f); // 小さな遅延

        while (currentState == GameState.Playing)
        {
            // アクティブユニット数をカウント
            int activeUnits = CountActiveUnits();

            if (activeUnits == 0)
            {
                // ユニットがなく、生成もできない
                if (destroyedBases < enemyBases.Length)
                {
                    TriggerLose();
                }
                // すべての基地が破壊されていれば、勝利はすでにトリガー済み
                yield break;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private int CountActiveUnits()
    {
        // アクティブなユニットGameObjectをカウント
        var units = FindObjectsOfType<Unit>();
        int count = 0;
        foreach (var unit in units)
        {
            if (unit.gameObject.activeInHierarchy)
                count++;
        }
        return count;
    }

    public void OnEnemyBaseDestroyed(EnemyBase destroyedBase)
    {
        destroyedBases++;

        if (destroyedBases >= enemyBases.Length)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Won;
        Debug.Log("勝利！すべての敵基地を破壊！");

        OnGameWin?.Invoke();
        OnGameStateChanged?.Invoke(currentState);

        // オプション：スローモーションエフェクト
        Time.timeScale = 0.5f;
    }

    public void TriggerLose()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Lost;
        Debug.Log("敗北！リソース切れ！");

        OnGameLose?.Invoke();
        OnGameStateChanged?.Invoke(currentState);
    }

    // リスタートボタンから呼ばれる
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // オプション：特定シーンをロード
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
```

---

### ステップ3: リソースチェックのためUnitSpawner更新

```csharp
// UnitSpawner.csを修正 - SpawnUnitメソッドを更新

private void SpawnUnit()
{
    // 生成可能かGameManagerに確認
    if (GameManager.Instance != null && !GameManager.Instance.TryConsumeSpawn())
    {
        return; // 生成不可 - リソース切れまたはゲーム終了
    }

    Vector3 position = spawnPoint.position;
    position.x += Random.Range(-0.2f, 0.2f);

    var unitObj = unitPool.Get();
    unitObj.transform.position = position;
}
```

---

### ステップ4: ゲームUIコントローラー作成

```csharp
// Assets/Scripts/UI/GameUIController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI spawnCountText;

    [Header("Win Screen")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private Button winRestartButton;

    [Header("Lose Screen")]
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private Button loseRestartButton;

    void Start()
    {
        // 終了画面を非表示
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);

        // GameManagerイベントを購読
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSpawnCountChanged.AddListener(UpdateSpawnCount);
            GameManager.Instance.OnGameWin.AddListener(ShowWinScreen);
            GameManager.Instance.OnGameLose.AddListener(ShowLoseScreen);

            // 生成数表示を初期化
            UpdateSpawnCount(GameManager.Instance.RemainingSpawns);
        }

        // リスタートボタンをセットアップ
        if (winRestartButton != null)
            winRestartButton.onClick.AddListener(OnRestartClicked);
        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(OnRestartClicked);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSpawnCountChanged.RemoveListener(UpdateSpawnCount);
            GameManager.Instance.OnGameWin.RemoveListener(ShowWinScreen);
            GameManager.Instance.OnGameLose.RemoveListener(ShowLoseScreen);
        }
    }

    private void UpdateSpawnCount(int count)
    {
        if (spawnCountText != null)
        {
            spawnCountText.text = $"ユニット: {count}";
        }
    }

    private void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
    }

    private void ShowLoseScreen()
    {
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
        }
    }

    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }
}
```

---

### ステップ5: Unity EditorでUI Prefab作成

**Unity Editorで：**

1. **UI Canvas作成**
   - GameObject → UI → Canvas
   - Render Mode: Screen Space - Overlay
   - CanvasScaler追加（Scale With Screen Size, 1080x1920参照）

2. **HUD作成**
   - 空の子"GameHUD"作成
   - 生成数用にTextMeshPro子を追加
   - 画面上部に配置
   - フォントサイズ: 48, 中央揃え

3. **勝利画面作成**
   - 空の子"WinScreen"作成
   - Image追加（全画面、半透明黒 #00000080）
   - TextMeshPro "勝利！"追加（中央、大きいフォント、金色）
   - Button "もう一度プレイ"追加
   - WinScreenをデフォルトで非アクティブに

4. **敗北画面作成**
   - 空の子"LoseScreen"作成
   - Image追加（全画面、半透明黒）
   - TextMeshPro "敗北"追加（中央、大きいフォント、赤色）
   - Button "再挑戦"追加
   - LoseScreenをデフォルトで非アクティブに

5. **GameUIController追加**
   - GameUIController.csをCanvasに追加
   - Inspectorですべての参照を割り当て

---

### ステップ6: リスタート用シーンセットアップ

[Unityシーン管理](https://damiandabrowski.medium.com/scene-management-in-unity-a-comprehensive-guide-to-loading-scenes-sync-and-async-845ae1e129be)による:

**ビルド設定：**
1. File → Build Settings
2. 現在のシーン（GameScene）をビルドに追加
3. シーンインデックスを確認（通常は0）

**シーンセットアップ：**
```
GameScene
├── Main Camera
├── Cannon
│   └── SpawnPoint
├── UnitSpawner
│   ├── UnitSpawner.cs
│   └── InputHandler.cs
├── GameManager（新規）
│   └── GameManager.cs
├── EnemyBase（prefabのインスタンス）
└── UICanvas
    ├── GameHUD
    ├── WinScreen
    └── LoseScreen
```

---

## テストフロー

```
1. ゲーム開始
   - 生成数が"ユニット: 50"と表示
   - プレイヤーがユニットを生成可能

2. プレイヤーがユニット生成
   - カウンターが生成ごとに減少
   - ユニットがゲートにヒット、増殖
   - ユニットが敵基地にヒット、ダメージ

3. 勝利パス
   - 敵基地HPが0に達する
   - "勝利！"画面が表示
   - リスタートボタンが動作

4. 敗北パス
   - 生成数が0に達する
   - すべてのユニットがdespawnするのを待機
   - 基地がまだHPを持っている
   - "敗北"画面が表示
   - リスタートボタンが動作
```

---

## 作成/修正するファイル

```
Assets/Scripts/
├── Core/
│   ├── GameState.cs        # 新規: ゲーム状態用Enum
│   └── GameManager.cs      # 修正: 完全実装
├── Systems/
│   └── UnitSpawner.cs      # 修正: リソースチェック
└── UI/
    └── GameUIController.cs # 新規

Assets/Scenes/
└── GameScene.unity         # 修正: GameManager、UI追加
```

---

## テストケース

| テスト | 手順 | 期待結果 |
|--------|------|----------|
| 生成カウンター | ユニットを生成 | カウンターが50から減少 |
| 勝利条件 | 生成が尽きる前に基地を破壊 | 勝利画面が表示 |
| 敗北条件 | 基地を破壊せずにすべての生成を使用 | ユニット消失後に敗北画面 |
| 勝利からリスタート | 勝利画面でリスタートをクリック | ゲームリセット、カウンターが50に |
| 敗北からリスタート | 敗北画面でリスタートをクリック | ゲームリセット、カウンターが50に |
| 0の後は生成不可 | すべての生成を使用、生成を試みる | 新しいユニットが出現しない |
| 終了時にゲーム停止 | 勝利または敗北 | これ以上の生成が不可能 |

---

## エッジケース

| シナリオ | 処理 |
|----------|------|
| 最後のユニットが基地を破壊 | 勝利（敗北前にチェック） |
| 複数の基地、一部破壊済み | カウントを追跡、すべて破壊で勝利 |
| プレイヤーがユニットを一つも生成しない | 最終的に敗北（0生成、基地生存） |
| 非常に速いリスタートクリック | SceneManagerが適切に処理 |
| ゲーム終了時に飛行中のユニット | 継続するが状態に影響しない |

---

## 完了の定義

- [ ] 生成カウンターが正しく表示・更新される
- [ ] ゲームが勝利条件を検出（すべての基地破壊）
- [ ] ゲームが敗北条件を検出（生成切れ、基地生存）
- [ ] "勝利！"とリスタートボタン付きの勝利画面が表示
- [ ] "敗北"とリスタートボタン付きの敗北画面が表示
- [ ] リスタートボタンがシーンを正しくリロード
- [ ] リスタート時にTime.timeScaleがリセット
- [ ] 問題なく複数ラウンドをプレイ可能

---

## ポリッシュアイデア（P1）

| 機能 | 実装 |
|------|------|
| スコア表示 | 使用ユニット数、経過時間を追跡 |
| 星評価 | 効率に基づいて3つ星 |
| 勝利時のスローモーション | Time.timeScale = 0.3f を一瞬 |
| 効果音 | 勝敗の音声キュー |
| アニメーション | UIがスライドイン/アウト |

---

## 次のステップ（フェーズ6+）

基本ゲームループの後 → `06_stage_system.md`（複数レベル、ステージ選択、進行）

これでMVP完成！プレイヤーができること：
1. ユニットを生成（リソース制限あり）
2. ゲートを通過（増殖）
3. 敵基地を攻撃（ダメージ）
4. 勝敗（明確なフィードバック）
5. リスタート（再挑戦）

---

*情報はWeb検索で検証済み - 2025年1月*

出典:
- [Unity Learn - Core Game Loop](https://learn.unity.com/course/2D-adventure-robot-repair/unit/game-ui-and-game-loop/tutorial/close-the-core-game-loop)
- [Scene Management Best Practices](https://damiandabrowski.medium.com/scene-management-in-unity-a-comprehensive-guide-to-loading-scenes-sync-and-async-845ae1e129be)
- [GameManager Pattern](https://frankgwarman.medium.com/intro-to-scene-management-and-restarting-the-game-acfe91f75370)
