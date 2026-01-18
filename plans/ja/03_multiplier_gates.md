# 乗算ゲート計画

## 目標

ユニットが通過すると増殖するゲートを実装する。これがゲームを満足感のあるものにするコア「数字が増える」メカニクス。

---

## 私が作るもの

```
┌─────────────────────────────────────────┐
│                                         │
│    ┌─────┐         ┌─────┐              │
│    │ ×3  │         │ ×2  │   ← ゲート   │
│    └──┬──┘         └──┬──┘              │
│       │               │                 │
│       ↓               ↓                 │
│      ●●●             ●●    ← 出力       │
│       ↑               ↑                 │
│       ●               ●     ← 入力      │
│                                         │
│              ___                        │
│             /   \                       │
│            | (◯) |  ← 大砲             │
│            └─────┘                      │
│                                         │
└─────────────────────────────────────────┘

1ユニットが×3ゲートに入る → 3ユニットが出てくる
```

---

## ゲートの種類

最初のバージョンでは：

| ゲート | 効果 | 見た目 |
|--------|------|--------|
| ×2 | ユニットを2倍 | 緑のゲート |
| ×3 | ユニットを3倍 | 青のゲート |
| ×5 | ユニットを5倍 | 紫のゲート |
| +10 | 10ユニット追加 | 黄色のゲート |

---

## Unityセットアップ

### Gate Prefab構造

```
Gate (Prefab)
├── Visual (SpriteRenderer - 色付きバー)
├── Label (TextMeshPro - "×2", "+10"など)
└── TriggerZone (BoxCollider2D - Is Trigger: true)
```

### 必要なコンポーネント

| GameObject | コンポーネント |
|------------|---------------|
| Gate | Gate.cs, BoxCollider2D (trigger) |
| Visual | SpriteRenderer |
| Label | TextMeshProUGUI or TextMeshPro |

### タグ＆レイヤー

```
Edit → Tags and Layers:
- タグ作成: "Gate"
- タグ作成: "Unit"
```

---

## 実装ステップ

### ステップ1: Gateスクリプト作成

```csharp
// Assets/Scripts/Entities/Gate.cs
using UnityEngine;
using TMPro;

public enum GateType { Multiply, Add }

public class Gate : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GateType gateType = GateType.Multiply;
    [SerializeField] private int value = 2;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private TextMeshPro label;

    [Header("Colors")]
    [SerializeField] private Color multiplyColor = Color.green;
    [SerializeField] private Color addColor = Color.yellow;

    private UnitSpawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<UnitSpawner>();
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // タイプと値に基づいて色を設定
        if (gateType == GateType.Multiply)
        {
            visual.color = value switch
            {
                2 => new Color(0, 1, 0),      // 緑
                3 => new Color(0, 0.5f, 1),   // 青
                5 => new Color(0.6f, 0, 1),   // 紫
                _ => multiplyColor
            };
            label.text = $"×{value}";
        }
        else
        {
            visual.color = addColor;
            label.text = $"+{value}";
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Unit")) return;

        var unit = other.GetComponent<Unit>();
        if (unit == null) return;

        // ユニットがすでにこのゲートを通過したかチェック
        if (unit.HasPassedGate(this)) return;

        // 通過済みとしてマーク
        unit.MarkGatePassed(this);

        // ゲート効果を処理
        ProcessGateEffect(unit);
    }

    private void ProcessGateEffect(Unit unit)
    {
        int unitsToSpawn = gateType == GateType.Multiply
            ? value - 1  // 元のユニットは継続、追加分を生成
            : value;     // Addは指定数だけ生成

        Vector3 basePos = unit.transform.position;

        for (int i = 0; i < unitsToSpawn; i++)
        {
            SpawnExtraUnit(basePos);
        }

        // 視覚的フィードバック
        ShowPopup(basePos);
        PulseGate();
    }

    private void SpawnExtraUnit(Vector3 basePosition)
    {
        if (spawner == null) return;

        Vector3 offset = new Vector3(
            Random.Range(-0.3f, 0.3f),
            0.2f,
            0
        );

        var newUnit = spawner.SpawnUnitAtPosition(basePosition + offset);

        // 新しいユニットが同じゲートを再トリガーしないようpassedGatesをコピー
        if (newUnit != null)
        {
            var unitComponent = newUnit.GetComponent<Unit>();
            unitComponent?.MarkGatePassed(this);
        }
    }

    private void ShowPopup(Vector3 position)
    {
        // TODO: ポップアップテキスト効果を実装
        // 今はログだけ
        Debug.Log($"ゲートトリガー: {(gateType == GateType.Multiply ? "×" : "+")}{value}");
    }

    private void PulseGate()
    {
        // シンプルなスケールパルスアニメーション
        StartCoroutine(PulseAnimation());
    }

    private System.Collections.IEnumerator PulseAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        float duration = 0.1f;
        float elapsed = 0f;

        // スケールアップ
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;

        // スケールダウン
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
```

---

### ステップ2: Unitスクリプト更新

Unit.csにゲート追跡を追加：

```csharp
// Assets/Scripts/Entities/Unit.cs
using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float despawnY = 6f;

    private Rigidbody2D rb;
    private UnitSpawner spawner;
    private HashSet<Gate> passedGates = new HashSet<Gate>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        spawner = FindObjectOfType<UnitSpawner>();
    }

    public void Initialize()
    {
        rb.velocity = Vector2.up * speed;
        passedGates.Clear(); // プールから再利用時にリセット
    }

    void Update()
    {
        if (transform.position.y > despawnY)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        passedGates.Clear();

        if (spawner != null)
        {
            spawner.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public bool HasPassedGate(Gate gate)
    {
        return passedGates.Contains(gate);
    }

    public void MarkGatePassed(Gate gate)
    {
        passedGates.Add(gate);
    }
}
```

---

### ステップ3: UnitSpawner更新

特定位置で生成するメソッドを追加：

```csharp
// UnitSpawner.csに追加

public GameObject SpawnUnitAtPosition(Vector3 position)
{
    var unitObj = unitPool.Get();
    if (unitObj != null)
    {
        unitObj.transform.position = position;
    }
    return unitObj;
}
```

---

### ステップ4: Gate Prefab作成

**Unity Editorで：**

1. 空のGameObject作成、"Gate"と命名
2. BoxCollider2D追加（Is Trigger: true, size: 1.5 x 0.3）
3. タグを"Gate"に設定

4. 子"Visual"作成：
   - SpriteRenderer追加（四角スプライト使用）
   - Scale: (1.5, 0.2, 1)

5. 子"Label"作成：
   - TextMeshProコンポーネント追加（またはTextMesh使用）
   - Anchor: center
   - Font size: 0.5（ワールド空間）

6. ルートにGate.csスクリプト追加
7. Inspectorで参照を割り当て
8. Prefabとして保存

---

### ステップ5: シーンにゲート配置

シンプルなGateManagerを作成するか手動配置：

```csharp
// Assets/Scripts/Systems/GateManager.cs
using UnityEngine;

public class GateManager : MonoBehaviour
{
    [SerializeField] private GameObject gatePrefab;

    void Start()
    {
        // ステージレイアウト例
        CreateGate(GateType.Multiply, 2, new Vector3(-1.5f, 0f, 0f));
        CreateGate(GateType.Multiply, 3, new Vector3(1.5f, 0f, 0f));
        CreateGate(GateType.Multiply, 2, new Vector3(0f, 2f, 0f));
        CreateGate(GateType.Add, 5, new Vector3(-1f, 3.5f, 0f));
        CreateGate(GateType.Multiply, 5, new Vector3(1f, 3.5f, 0f));
    }

    private void CreateGate(GateType type, int value, Vector3 position)
    {
        var gateObj = Instantiate(gatePrefab, position, Quaternion.identity);
        var gate = gateObj.GetComponent<Gate>();
        // 注: SetGateTypeメソッドを公開するか、SerializeFieldを使用する必要あり
    }
}
```

またはシーン内で手動でゲートを配置しInspectorで設定。

---

## シーンセットアップチェックリスト

1. **Unit Prefab** - "Unit"タグ追加
2. **Gate Prefab** - "Gate"タグ追加、BoxCollider2D (trigger)
3. **GateManager** - GateManager.cs付きの空GameObjec
4. **物理設定** - ユニットとゲートが衝突できることを確認

**レイヤー衝突マトリックス：**
```
Edit → Project Settings → Physics 2D → Layer Collision Matrix
ユニットとゲートが相互作用できることを確認
```

---

## 作成/修正するファイル

```
Assets/Scripts/
├── Entities/
│   ├── Unit.cs         # 修正: ゲート追跡追加
│   └── Gate.cs         # 新規
├── Systems/
│   ├── UnitSpawner.cs  # 修正: SpawnUnitAtPosition追加
│   └── GateManager.cs  # 新規（オプション）
└── Effects/
    └── PopupText.cs    # 新規（オプション）

Assets/Prefabs/
├── Unit.prefab         # 修正: タグ追加
└── Gate.prefab         # 新規
```

---

## テストケース

| テスト | 手順 | 期待結果 |
|--------|------|----------|
| 基本乗算 | 1ユニットが×2ゲートにヒット | 2ユニットが上に継続 |
| 3倍乗算 | 1ユニットが×3ゲートにヒット | 3ユニットが上に継続 |
| 加算ゲート | 1ユニットが+5ゲートにヒット | 合計6ユニット（1 + 5） |
| 連鎖ゲート | 1ユニットが×2、次に×3をヒット | 両ゲート後に6ユニット |
| 二重トリガーなし | ユニットが同じゲートを通過 | 1回だけ乗算 |
| 視覚フィードバック | ユニットがゲートにヒット | ゲートがパルス、ポップアップ表示 |

---

## エッジケース

| シナリオ | 処理 |
|----------|------|
| 多数のユニットが同時に同じゲートにヒット | 各々が独立してトリガー |
| 乗算中にプール枯渇 | 可能な限り生成 |
| ゲートが生成したユニットが別のゲートにヒット | 通常通り動作（連鎖乗算） |
| 重なったゲート | ユニットは両方をトリガー（設計上の決定） |

---

## 完了の定義

- [ ] ゲートが正しい色とラベルで描画される
- [ ] ゲートを通過するユニットが正しく乗算される
- [ ] ×2、×3、×5、+Nゲートがすべて動作
- [ ] 視覚的フィードバック（パルス、オプションでポップアップ）
- [ ] ユニットが同じゲートを2回トリガーしない
- [ ] 連鎖乗算が動作（ゲート → ゲート）
- [ ] ステージレイアウトに複数のゲート

---

## 次のステップ

ゲートが動作したら → `04_enemy_base.md`（蓄積したユニットで破壊するターゲット）
