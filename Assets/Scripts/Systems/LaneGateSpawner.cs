using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Spawns gates in lanes with object pooling.
/// レーンにゲートをスポーン。オブジェクトプーリング使用。
/// </summary>
public class LaneGateSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private bool spawnEnabled = true;

    [Header("Gate Settings")]
    [SerializeField] private float gateWidth = 1.5f;
    [SerializeField] private float gateHeight = 0.5f;

    [Header("Gate Value Ranges")]
    [SerializeField] private int minAddValue = 3;
    [SerializeField] private int maxAddValue = 10;
    [SerializeField] private int minSubtractValue = 2;
    [SerializeField] private int maxSubtractValue = 5;
    [SerializeField] private int minMultiplyValue = 2;
    [SerializeField] private int maxMultiplyValue = 3;

    [Header("Gate Type Weights")]
    [SerializeField] [Range(0, 100)] private int addWeight = 50;
    [SerializeField] [Range(0, 100)] private int subtractWeight = 30;
    [SerializeField] [Range(0, 100)] private int multiplyWeight = 20;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;

    [Header("References")]
    [SerializeField] private LaneManager laneManager;
    [SerializeField] private ScrollManager scrollManager;
    [SerializeField] private SpawnCoordinator spawnCoordinator;

    private List<LaneGate> gatePool = new List<LaneGate>();
    private float spawnTimer;
    private GameObject gatePrefab;

    void Start()
    {
        if (laneManager == null)
        {
            laneManager = LaneManager.Instance;
        }
        if (scrollManager == null)
        {
            scrollManager = ScrollManager.Instance;
        }
        if (spawnCoordinator == null)
        {
            spawnCoordinator = SpawnCoordinator.Instance;
        }

        // ゲートプレファブを作成
        CreateGatePrefab();

        // プールを初期化
        InitializePool();

        // 初期ディレイを設定
        spawnTimer = initialDelay;
    }

    void Update()
    {
        if (!spawnEnabled) return;

        // ボスフェーズ中はゲートをスポーンしない
        if (GameManager.Instance != null && GameManager.Instance.InBossPhase)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnGate();
            spawnTimer = spawnInterval;
        }
    }

    private void CreateGatePrefab()
    {
        gatePrefab = new GameObject("GatePrefab");
        gatePrefab.SetActive(false);

        // SpriteRenderer
        SpriteRenderer sr = gatePrefab.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite();
        sr.color = Color.green;
        sr.sortingOrder = 1;

        // Scrollable component
        Scrollable scrollable = gatePrefab.AddComponent<Scrollable>();

        // LaneGate component
        LaneGate gate = gatePrefab.AddComponent<LaneGate>();

        // Collider
        BoxCollider2D collider = gatePrefab.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);

        // Rigidbody2D
        Rigidbody2D rb = gatePrefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        gatePrefab.transform.localScale = new Vector3(gateWidth, gateHeight, 1f);

        // ラベル用の子オブジェクトを作成
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(gatePrefab.transform);
        labelObj.transform.localPosition = Vector3.zero;
        labelObj.transform.localScale = new Vector3(1f / gateWidth, 1f / gateHeight, 1f); // 親のスケールを相殺

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = "+5";
        tmp.fontSize = 4;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.sortingOrder = 2;

        // プレファブを非表示の親に移動
        gatePrefab.transform.SetParent(transform);
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            LaneGate gate = CreateGate();
            gate.gameObject.SetActive(false);
            gatePool.Add(gate);
        }
    }

    private LaneGate CreateGate()
    {
        GameObject obj = Instantiate(gatePrefab, transform);
        obj.name = "LaneGate";
        return obj.GetComponent<LaneGate>();
    }

    private LaneGate GetGateFromPool()
    {
        // 非アクティブなゲートを探す
        foreach (var gate in gatePool)
        {
            if (!gate.gameObject.activeInHierarchy)
            {
                return gate;
            }
        }

        // プールにない場合は新規作成
        LaneGate newGate = CreateGate();
        gatePool.Add(newGate);
        return newGate;
    }

    /// <summary>
    /// ゲートをプールに返す
    /// </summary>
    public void ReturnGate(LaneGate gate)
    {
        gate.gameObject.SetActive(false);
    }

    private void SpawnGate()
    {
        if (laneManager == null || scrollManager == null) return;

        float yPos = scrollManager.SpawnY;

        // SpawnCoordinatorを使用してレーンを選択
        int lane;
        if (spawnCoordinator != null)
        {
            lane = spawnCoordinator.GetAvailableLane(yPos);
            if (lane < 0)
            {
                // 全レーンが埋まっている場合はスキップ
                Debug.Log("Gate spawn skipped: no available lane");
                return;
            }
        }
        else
        {
            lane = Random.Range(0, laneManager.LaneCount);
        }

        float xPos = laneManager.GetLaneXPosition(lane);

        // ランダムなゲートタイプを選択
        LaneGate.GateType type = GetRandomGateType();
        int value = GetRandomValue(type);

        // プールからゲートを取得
        LaneGate gate = GetGateFromPool();
        gate.transform.position = new Vector3(xPos, yPos, 0);
        gate.Initialize(this, lane, type, value);

        // スポーンを記録
        if (spawnCoordinator != null)
        {
            spawnCoordinator.RecordSpawn(lane, yPos);
        }
    }

    private LaneGate.GateType GetRandomGateType()
    {
        int totalWeight = addWeight + subtractWeight + multiplyWeight;
        int random = Random.Range(0, totalWeight);

        if (random < addWeight)
        {
            return LaneGate.GateType.Add;
        }
        else if (random < addWeight + subtractWeight)
        {
            return LaneGate.GateType.Subtract;
        }
        else
        {
            return LaneGate.GateType.Multiply;
        }
    }

    private int GetRandomValue(LaneGate.GateType type)
    {
        switch (type)
        {
            case LaneGate.GateType.Add:
                return Random.Range(minAddValue, maxAddValue + 1);
            case LaneGate.GateType.Subtract:
                return Random.Range(minSubtractValue, maxSubtractValue + 1);
            case LaneGate.GateType.Multiply:
                return Random.Range(minMultiplyValue, maxMultiplyValue + 1);
            default:
                return 5;
        }
    }

    /// <summary>
    /// 指定したレーンにゲートをスポーン
    /// </summary>
    public void SpawnGateAtLane(int lane, LaneGate.GateType type, int value)
    {
        if (laneManager == null || scrollManager == null) return;
        if (!laneManager.IsValidLane(lane)) return;

        float xPos = laneManager.GetLaneXPosition(lane);
        float yPos = scrollManager.SpawnY;

        LaneGate gate = GetGateFromPool();
        gate.transform.position = new Vector3(xPos, yPos, 0);
        gate.Initialize(this, lane, type, value);
    }

    /// <summary>
    /// スポーンを有効/無効にする
    /// </summary>
    public void SetSpawnEnabled(bool enabled)
    {
        spawnEnabled = enabled;
    }

    /// <summary>
    /// スポーン間隔を設定
    /// </summary>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = Mathf.Max(0.5f, interval);
    }

    /// <summary>
    /// ゲートタイプの重みを設定
    /// </summary>
    public void SetTypeWeights(int add, int subtract, int multiply)
    {
        addWeight = Mathf.Max(0, add);
        subtractWeight = Mathf.Max(0, subtract);
        multiplyWeight = Mathf.Max(0, multiply);
    }

    /// <summary>
    /// ゲート値の範囲を設定
    /// </summary>
    public void SetValueRanges(int minAdd, int maxAdd, int minSub, int maxSub, int minMul, int maxMul)
    {
        minAddValue = Mathf.Max(1, minAdd);
        maxAddValue = Mathf.Max(minAddValue, maxAdd);
        minSubtractValue = Mathf.Max(1, minSub);
        maxSubtractValue = Mathf.Max(minSubtractValue, maxSub);
        minMultiplyValue = Mathf.Max(2, minMul);
        maxMultiplyValue = Mathf.Max(minMultiplyValue, maxMul);
    }

    private Sprite CreateRectSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }
}
