# 敵基地計画

## 目標

敵基地を実装する - プレイヤーが蓄積したユニットで破壊するターゲット。これによりゲームに目標と勝利条件が生まれる。

---

## 私が作るもの

```
┌─────────────────────────────────────────┐
│                                         │
│         ┌─────────────────┐             │
│         │   ENEMY BASE    │             │
│         │   HP: ████░░    │  ← ターゲット│
│         └─────────────────┘             │
│                  ↑                      │
│              ●●●●●●●                    │
│              (ユニットが基地にヒット)    │
│                  ↑                      │
│              ●●●●●●●●●●                 │
│                  ↑                      │
│              ┌─────┐                    │
│              │ ×3  │                    │
│              └─────┘                    │
│                  ↑                      │
│                 ●●●                     │
│                                         │
│              ___                        │
│             /   \\                       │
│            | (◯) |  ← 大砲              │
│            └─────┘                      │
│                                         │
└─────────────────────────────────────────┘

ユニットが敵基地にヒット → 基地HPが減少 → HPが0に → 勝利！
```

---

## 参考: Mob Controlの敵基地

[MAF Analysis](https://maf.ad/en/blog/mob-control-analysis-hybrid-casual/)による:

| 機能 | Mob Controlの実装 |
|------|-------------------|
| ビジュアル | HPバー付きの建物/要塞 |
| ダメージ | 各ユニットは接触時に1ダメージ |
| フィードバック | ヒットエフェクト、HPバー更新 |
| 破壊 | HP = 0でパーティクルと共に爆発 |
| 複数基地 | 後半ステージでは複数の基地 |

MVPでは、シンプルなHP仕組みの単一基地を実装。

---

## Unityセットアップ

### EnemyBase Prefab構造

```
EnemyBase (Prefab)
├── Visual (SpriteRenderer - 基地の建物)
├── HPBar (Canvas + SliderまたはImage fill)
│   ├── Background (Image)
│   └── Fill (Image - HPが減ると減少)
├── HitZone (BoxCollider2D - Is Trigger: true)
└── DestructionEffect (ParticleSystem - 無効、死亡時に再生)
```

### 必要なコンポーネント

| GameObject | コンポーネント |
|------------|---------------|
| EnemyBase | EnemyBase.cs, BoxCollider2D (trigger) |
| Visual | SpriteRenderer |
| HPBar | Canvas (World Space), SliderまたはImage |
| DestructionEffect | ParticleSystem |

### タグ＆レイヤー

```
Edit → Tags and Layers:
- タグ: "EnemyBase"（勝利条件チェック用）
- "Unit"タグが存在することを確認（前の計画から）
```

---

## 実装ステップ

### ステップ1: ヘルスシステム作成

再利用可能なモジュール式ヘルスシステム（[参考](https://medium.com/nerd-for-tech/tip-of-the-day-modular-health-system-unity-8f5d2f187027)）:

```csharp
// Assets/Scripts/Core/Health.cs
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxHealth = 100;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged; // current, max
    public UnityEvent OnDeath;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercent => (float)currentHealth / maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // すでに死亡

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(int newMax)
    {
        maxHealth = newMax;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
```

---

### ステップ2: EnemyBaseスクリプト作成

```csharp
// Assets/Scripts/Entities/EnemyBase.cs
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private ParticleSystem destructionEffect;

    [Header("Settings")]
    [SerializeField] private int damagePerUnit = 1;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    private Color originalColor;
    private bool isDestroyed = false;

    void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (visual != null)
            originalColor = visual.color;
    }

    void Start()
    {
        // ヘルスイベントを購読
        health.OnHealthChanged.AddListener(OnHealthChanged);
        health.OnDeath.AddListener(OnBaseDestroyed);
    }

    void OnDestroy()
    {
        health.OnHealthChanged.RemoveListener(OnHealthChanged);
        health.OnDeath.RemoveListener(OnBaseDestroyed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;
        if (!other.CompareTag("Unit")) return;

        // 基地にダメージ
        health.TakeDamage(damagePerUnit);

        // 視覚的フィードバック
        StartCoroutine(FlashHit());

        // ユニットをプールに戻す
        var unit = other.GetComponent<Unit>();
        if (unit != null)
        {
            unit.ReturnToPool();
        }
    }

    private System.Collections.IEnumerator FlashHit()
    {
        if (visual != null)
        {
            visual.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            visual.color = originalColor;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        // HPBarは独自のリスナーまたは直接参照で更新
        // 画面揺れなどのエフェクトをここに追加可能
    }

    private void OnBaseDestroyed()
    {
        isDestroyed = true;

        // 破壊エフェクト再生
        if (destructionEffect != null)
        {
            destructionEffect.transform.SetParent(null); // 生き残るように親を外す
            destructionEffect.Play();
            Destroy(destructionEffect.gameObject, destructionEffect.main.duration);
        }

        // GameManagerに通知
        GameManager.Instance?.OnEnemyBaseDestroyed(this);

        // 基地を非表示または破壊
        visual.enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    // ステージセットアップ用
    public void Initialize(int hp)
    {
        health.SetMaxHealth(hp);
        isDestroyed = false;
        visual.enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }
}
```

---

### ステップ3: HPバーUI作成

```csharp
// Assets/Scripts/UI/HPBar.cs
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health targetHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private Slider slider; // Imageの代替

    [Header("Settings")]
    [SerializeField] private Gradient colorGradient; // 緑から赤
    [SerializeField] private bool useGradient = true;

    void Start()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.AddListener(UpdateBar);
            UpdateBar(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }

    void OnDestroy()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.RemoveListener(UpdateBar);
        }
    }

    private void UpdateBar(int current, int max)
    {
        float percent = (float)current / max;

        if (slider != null)
        {
            slider.value = percent;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = percent;

            if (useGradient && colorGradient != null)
            {
                fillImage.color = colorGradient.Evaluate(percent);
            }
        }
    }

    // 外部からの割り当て用
    public void SetTarget(Health health)
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.RemoveListener(UpdateBar);
        }

        targetHealth = health;

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.AddListener(UpdateBar);
            UpdateBar(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }
}
```

---

### ステップ4: GameManager作成（基本版）

```csharp
// Assets/Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Events")]
    public UnityEvent OnGameWin;
    public UnityEvent OnGameLose;

    private EnemyBase[] enemyBases;
    private int destroyedBases = 0;

    void Awake()
    {
        // シングルトンパターン
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // シーン内のすべての敵基地を検索
        enemyBases = FindObjectsOfType<EnemyBase>();
        destroyedBases = 0;
    }

    public void OnEnemyBaseDestroyed(EnemyBase destroyedBase)
    {
        destroyedBases++;

        // すべての基地が破壊されたかチェック
        if (destroyedBases >= enemyBases.Length)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        Debug.Log("勝利！すべての敵基地を破壊！");
        OnGameWin?.Invoke();
        // 05_basic_game_loop.mdで拡張
    }

    public void TriggerLose()
    {
        Debug.Log("敗北！リソース切れ！");
        OnGameLose?.Invoke();
        // 05_basic_game_loop.mdで拡張
    }
}
```

---

### ステップ5: EnemyBase Prefab作成

**Unity Editorで：**

1. 空のGameObject作成、"EnemyBase"と命名
2. BoxCollider2D追加（Is Trigger: true, size: 2 x 1）
3. Health.csコンポーネント追加
4. EnemyBase.csコンポーネント追加
5. タグを"EnemyBase"に設定

6. 子"Visual"作成：
   - SpriteRenderer追加（四角スプライト使用）
   - Scale: (2, 1, 1)
   - Color: 赤または敵の色

7. 子"HPBar"作成：
   - Canvasコンポーネント追加（Render Mode: World Space）
   - Canvasサイズを基地の上に収まるように設定
   - 子"Background"をImage（グレー）で追加
   - 子"Fill"をImage（緑、Image Type: Filled）で追加
   - HPBar.csをHPBarオブジェクトに追加

8. 子"DestructionEffect"作成：
   - ParticleSystem追加
   - 爆発エフェクト用に設定
   - Play On Awake: falseに設定

9. Inspectorで参照を割り当て
10. Prefabとして保存

---

### ステップ6: プール返却のためUnit更新

Unit.csに適切なプール返却があることを確認（02_basic_unit_movement.mdから）：

```csharp
// すでにUnit.csにあるが、このメソッドの存在を確認：
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
```

---

## シーンセットアップチェックリスト

1. **GameManager（空のGameObject）**
   - GameManager.cs追加
   - 位置は重要でない

2. **EnemyBase Prefabインスタンス**
   - 画面上部に配置: (0, 4, 0)
   - Healthコンポーネント経由でHP設定（テスト用に50など）

3. **タグ確認**
   - Unit prefabに"Unit"タグ
   - EnemyBase prefabに"EnemyBase"タグ

4. **物理設定**
   - ユニットとEnemyBaseが相互作用できることを確認（レイヤー衝突マトリックス）

---

## 作成/修正するファイル

```
Assets/Scripts/
├── Core/
│   ├── Health.cs           # 新規: モジュール式ヘルスシステム
│   └── GameManager.cs      # 新規: ゲーム状態管理
├── Entities/
│   ├── Unit.cs             # 確認: ReturnToPoolが存在
│   └── EnemyBase.cs        # 新規
└── UI/
    └── HPBar.cs            # 新規

Assets/Prefabs/
├── Unit.prefab             # 確認: "Unit"タグ
└── EnemyBase.prefab        # 新規
```

---

## テストケース

| テスト | 手順 | 期待結果 |
|--------|------|----------|
| ユニットが基地にヒット | ユニットを生成、基地にヒットさせる | 基地HPが減少、ユニットが消える |
| HPバー更新 | 基地にダメージを与える | HPバーのfillが減少 |
| 視覚的フィードバック | ユニットが基地にヒット | 基地が一瞬赤く点滅 |
| 基地破壊 | HPを0に減らす | 破壊エフェクト再生、基地非表示 |
| 勝利条件 | すべての基地を破壊 | GameManager.OnGameWinが呼び出される |
| 複数ユニット | 多数のユニットが同時に基地にヒット | 各々がダメージを与え、エラーなし |

---

## エッジケース

| シナリオ | 処理 |
|----------|------|
| すでに破壊された基地にユニットがヒット | isDestroyedフラグで処理を防止 |
| レベルに複数の基地 | 破壊数を追跡、すべて破壊で勝利 |
| 非常に高いダメージ | HPを0にクランプ、マイナスにならない |
| 基地HPが0またはマイナスに設定 | SetMaxHealthで最小1を保証 |

---

## 完了の定義

- [ ] 敵基地がHPバー付きで描画される
- [ ] 基地にヒットしたユニットがダメージを与える（HPが減少）
- [ ] HPバーがダメージ時に視覚的に更新される
- [ ] 基地が点滅またはヒットフィードバックを表示
- [ ] 基地破壊時にパーティクルエフェクトが再生
- [ ] 基地HPが0になると勝利条件がトリガー
- [ ] GameManagerが勝利通知を受け取る
- [ ] 基地にヒットしたユニットがプールに戻る

---

## パフォーマンス考慮事項

| 技法 | 目的 |
|------|------|
| イベント駆動のHP更新 | フレームごとのHPチェックなし |
| トリガー衝突 | 効率的な衝突検出 |
| ユニットのオブジェクトプーリング | ユニットは破壊されずプールに戻る |

---

## 次のステップ

敵基地が動作したら → `05_basic_game_loop.md`（リソース制限、敗北条件、リスタート、UI）

---

*情報はWeb検索で検証済み - 2025年1月*

出典:
- [Modular Health System](https://medium.com/nerd-for-tech/tip-of-the-day-modular-health-system-unity-8f5d2f187027)
- [Health Bar Implementation](https://www.codemahal.com/adding-a-health-bar-to-a-2d-game-in-unity)
- [Unity Discussions - Health System](https://discussions.unity.com/t/health-system/805432)
