# 基本ユニット移動計画

## 目標

コアゲームプレイメカニクスを作成：プレイヤーがタップ/長押しで大砲からユニットを生成し、ユニットが敵に向かって上に移動する。

---

## 現状

Unity 2D (URP)テンプレートでプロジェクト作成済み。最初のゲームプレイ機能を実装する。

---

## 私が作るもの

```
┌─────────────────────────────────────────┐
│                                         │
│              ↑ ↑ ↑                       │
│             (ユニットが上に移動)         │
│              ↑ ↑ ↑                       │
│                                         │
│                                         │
│                                         │
│               ___                       │
│              /   \                      │
│             |     |  ← 大砲             │
│             └─────┘                     │
│                                         │
│  [ どこでもタップ/長押しでユニット生成 ] │
│                                         │
└─────────────────────────────────────────┘
```

---

## ユーザーストーリー

| ユーザー | したいこと | 理由 |
|---------|-----------|------|
| プレイヤー | タップでユニットを生成 | 敵に向けてユニットを送りたい |
| プレイヤー | 長押しで連続生成 | ユニットの流れを作りたい |
| プレイヤー | ユニットが動くのを見る | 入力が効いたフィードバックが欲しい |
| プレイヤー | ユニットが画面外で消える | ゲームが遅くならないように |

---

## ゲームデザインの決定

### ユニットの振る舞い

| プロパティ | 値 | 理由 |
|-----------|-----|------|
| 速度 | 5 units/秒 | レスポンシブに感じるほど速く、見えるほど遅く |
| サイズ | 0.5 units | 画面に多数表示できるほど小さく |
| 生成レート | 10ユニット/秒（長押し時） | 圧倒されない流れに感じる |
| 方向 | まっすぐ上 (Vector2.up) | v1はシンプルに、後でエイム追加可能 |

### 大砲の振る舞い

| プロパティ | 値 | 理由 |
|-----------|-----|------|
| 位置 | 下部中央 (0, -4) | このジャンルの標準 |
| 見た目 | シンプルなスプライト | プレースホルダー、後で改善可能 |
| 狙い | 上向き固定 | 最もシンプルな実装から |

---

## Unityセットアップ要件

### シーンヒエラルキー

```
GameScene
├── Main Camera (Orthographic, size 5)
├── Cannon
│   └── SpawnPoint (生成位置用の空GameObjec)
├── UnitSpawner (スクリプト保持用)
└── UI Canvas (今はオプション)
```

### 必要なコンポーネント

| GameObject | コンポーネント |
|------------|---------------|
| Unit (Prefab) | SpriteRenderer, Rigidbody2D, CircleCollider2D, Unit.cs |
| Cannon | SpriteRenderer, Cannon.cs |
| UnitSpawner | UnitSpawner.cs |

### 物理設定

```
Edit → Project Settings → Physics 2D
- Gravity: (0, 0)  ← 重力なし、ユニットは速度のみで移動
```

---

## 実装ステップ

### ステップ1: Unit Prefab作成

生成される基本ユニットを作成。

**Unity Editorで：**
1. 空のGameObjectを作成、"Unit"と命名
2. SpriteRenderer追加（Unityのデフォルト円スプライト使用）
3. Rigidbody2D追加（Body Type: Dynamic, Gravity Scale: 0）
4. CircleCollider2D追加（Is Trigger: true）
5. Unit.csスクリプト作成してアタッチ

```csharp
// Assets/Scripts/Entities/Unit.cs
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize()
    {
        // 上に移動開始
        rb.velocity = Vector2.up * speed;
    }

    public void Deactivate()
    {
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
    }
}
```

6. Assets/Prefabsフォルダにドラッグしてprefab作成
7. シーンから削除（スクリプト経由で生成するため）

**成功基準**: PrefabsフォルダにUnit prefabが存在

---

### ステップ2: 大砲の表示作成

**Unity Editorで：**
1. (0, -4)に空のGameObject作成、"Cannon"と命名
2. SpriteRenderer追加（四角スプライト使用、大砲に見えるようスケール）
3. 子の空GameObject作成(0, 0.5)、"SpawnPoint"と命名

```csharp
// Assets/Scripts/Entities/Cannon.cs
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public Transform spawnPoint;

    void Awake()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }
}
```

**成功基準**: ゲームビュー下部に大砲が表示

---

### ステップ3: 入力検出

タップ/長押しを検出するInputHandler作成。

```csharp
// Assets/Scripts/Systems/InputHandler.cs
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public bool IsSpawning { get; private set; }

    void Update()
    {
        // マウスクリックとタッチ両方で動作
        IsSpawning = Input.GetMouseButton(0);
    }
}
```

**成功基準**: 画面が押されていることを検出できる

---

### ステップ4: オブジェクトプーリング付きUnit Spawner

```csharp
// Assets/Scripts/Systems/UnitSpawner.cs
using UnityEngine;
using UnityEngine.Pool;

public class UnitSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private InputHandler inputHandler;

    [Header("Settings")]
    [SerializeField] private float spawnRate = 10f; // ユニット/秒
    [SerializeField] private int poolSize = 200;

    private ObjectPool<GameObject> unitPool;
    private float spawnTimer;
    private float spawnInterval;

    void Awake()
    {
        spawnInterval = 1f / spawnRate;

        unitPool = new ObjectPool<GameObject>(
            createFunc: CreateUnit,
            actionOnGet: OnGetUnit,
            actionOnRelease: OnReleaseUnit,
            actionOnDestroy: OnDestroyUnit,
            defaultCapacity: poolSize,
            maxSize: poolSize
        );
    }

    void Update()
    {
        if (inputHandler.IsSpawning)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                SpawnUnit();
                spawnTimer = 0f;
            }
        }
        else
        {
            spawnTimer = spawnInterval; // 次のプレスで即座に生成できるように
        }
    }

    private void SpawnUnit()
    {
        Vector3 position = spawnPoint.position;
        // わずかなランダム散布を追加
        position.x += Random.Range(-0.2f, 0.2f);

        var unitObj = unitPool.Get();
        unitObj.transform.position = position;
    }

    private GameObject CreateUnit()
    {
        var obj = Instantiate(unitPrefab);
        obj.SetActive(false);
        return obj;
    }

    private void OnGetUnit(GameObject obj)
    {
        obj.SetActive(true);
        var unit = obj.GetComponent<Unit>();
        unit.Initialize();
    }

    private void OnReleaseUnit(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyUnit(GameObject obj)
    {
        Destroy(obj);
    }

    // ユニットが画面外に出た時に呼ばれる
    public void ReturnToPool(GameObject unitObj)
    {
        unitPool.Release(unitObj);
    }
}
```

**成功基準**: タップでユニット生成、長押しで連続的な流れ

---

### ステップ5: 画面外検出

画面外に出たらプールに戻るようUnit.csを更新：

```csharp
// Assets/Scripts/Entities/Unit.cs
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float despawnY = 6f; // 画面上端より上

    private Rigidbody2D rb;
    private UnitSpawner spawner;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Spawnerを見つける（またはInitialize経由で注入）
        spawner = FindObjectOfType<UnitSpawner>();
    }

    public void Initialize()
    {
        rb.velocity = Vector2.up * speed;
    }

    void Update()
    {
        // 画面外かチェック
        if (transform.position.y > despawnY)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        if (spawner != null)
        {
            spawner.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
```

**成功基準**: ユニットが画面外で消える、100体以上生成してもラグなし

---

## シーンセットアップチェックリスト

**Unity Editorでセットアップ：**

1. **Main Camera**
   - Projection: Orthographic
   - Size: 5
   - Position: (0, 0, -10)
   - Background: ダークカラー (#1a1a2e)

2. **Cannon**
   - Position: (0, -4, 0)
   - Cannon.csスクリプト追加
   - 子"SpawnPoint"を(0, 0.5, 0)に

3. **UnitSpawner (空のGameObject)**
   - UnitSpawner.cs追加
   - InputHandler.cs追加
   - Inspectorで参照を割り当て：
     - Unit Prefab: Prefabsフォルダからドラッグ
     - Spawn Point: CannonのSpawnPointをドラッグ
     - Input Handler: 自身をドラッグ

4. **Unit Prefab**
   - Scale: (0.3, 0.3, 1)
   - SpriteRenderer: 円スプライト、緑色
   - Rigidbody2D: Dynamic, Gravity Scale 0
   - CircleCollider2D: Is Trigger true
   - Unit.csアタッチ済み

---

## 作成するファイル

```
Assets/
├── Scenes/
│   └── GameScene.unity
├── Scripts/
│   ├── Entities/
│   │   ├── Unit.cs
│   │   └── Cannon.cs
│   └── Systems/
│       ├── UnitSpawner.cs
│       └── InputHandler.cs
├── Prefabs/
│   └── Unit.prefab
└── Art/
    └── Sprites/
        └── (今はUnityデフォルトを使用)
```

---

## テストケース

| テスト | 手順 | 期待結果 |
|--------|------|----------|
| 単体生成 | 1回クリック | 1つのユニットが出現して上に移動 |
| 連続生成 | 長押し | 押している間ユニットの流れ |
| 離すと停止 | 長押し後に離す | 新しいユニットが出現しない |
| 画面外クリーンアップ | 多数のユニットを生成 | 画面外でユニットが消える |
| パフォーマンス | 10秒間長押し | ラグなし、60FPS維持 |

---

## 完了の定義

- [ ] 画面下部に大砲が表示される
- [ ] タップで単体ユニット生成
- [ ] 長押しで連続的な流れを生成
- [ ] ユニットが一定速度で上に移動
- [ ] 画面外のユニットがプールにリサイクルされる
- [ ] 100体以上のユニットでもフレーム落ちなし
- [ ] デスクトップ（マウス）とモバイル（タッチ）で動作

---

## 次のステップ

ユニットが生成・移動できたら → `03_multiplier_gates.md`（ユニットを増殖させるゲート）
