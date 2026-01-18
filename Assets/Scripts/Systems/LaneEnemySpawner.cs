using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns enemies in lanes with object pooling.
/// レーンに敵をスポーン。オブジェクトプーリング使用。
/// </summary>
public class LaneEnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private bool spawnEnabled = true;

    [Header("Enemy Settings")]
    [SerializeField] private int enemyMinHP = 5;
    [SerializeField] private int enemyMaxHP = 10;
    [SerializeField] private float enemySize = 0.8f;
    [SerializeField] private Color enemyColor = Color.red;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;

    [Header("References")]
    [SerializeField] private LaneManager laneManager;
    [SerializeField] private ScrollManager scrollManager;
    [SerializeField] private SpawnCoordinator spawnCoordinator;

    private List<LaneEnemy> enemyPool = new List<LaneEnemy>();
    private float spawnTimer;
    private GameObject enemyPrefab;

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

        // 敵プレファブを作成
        CreateEnemyPrefab();

        // プールを初期化
        InitializePool();

        // 初期ディレイを設定
        spawnTimer = initialDelay;
    }

    void Update()
    {
        if (!spawnEnabled) return;

        // ボスフェーズ中は敵をスポーンしない
        if (GameManager.Instance != null && GameManager.Instance.InBossPhase)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    private void CreateEnemyPrefab()
    {
        enemyPrefab = new GameObject("EnemyPrefab");
        enemyPrefab.SetActive(false);
        enemyPrefab.tag = "Enemy"; // 重要：Enemyタグを設定

        // SpriteRenderer
        SpriteRenderer sr = enemyPrefab.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = enemyColor;
        sr.sortingOrder = 2;

        // Health component
        Health health = enemyPrefab.AddComponent<Health>();

        // Scrollable component
        Scrollable scrollable = enemyPrefab.AddComponent<Scrollable>();

        // LaneEnemy component
        LaneEnemy enemy = enemyPrefab.AddComponent<LaneEnemy>();

        // Collider
        BoxCollider2D collider = enemyPrefab.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        // Rigidbody2D（トリガー検出用）
        // Dynamic + Gravity 0 で確実に衝突検出
        Rigidbody2D rb = enemyPrefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeAll; // 物理で動かないように

        enemyPrefab.transform.localScale = Vector3.one * enemySize;

        // プレファブを非表示の親に移動
        enemyPrefab.transform.SetParent(transform);
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            LaneEnemy enemy = CreateEnemy();
            enemy.gameObject.SetActive(false);
            enemyPool.Add(enemy);
        }
    }

    private LaneEnemy CreateEnemy()
    {
        GameObject obj = Instantiate(enemyPrefab, transform);
        obj.name = "LaneEnemy";
        obj.tag = "Enemy"; // タグを確実に設定
        return obj.GetComponent<LaneEnemy>();
    }

    private LaneEnemy GetEnemyFromPool()
    {
        // 非アクティブな敵を探す
        foreach (var enemy in enemyPool)
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                return enemy;
            }
        }

        // プールにない場合は新規作成
        LaneEnemy newEnemy = CreateEnemy();
        enemyPool.Add(newEnemy);
        return newEnemy;
    }

    /// <summary>
    /// 敵をプールに返す
    /// </summary>
    public void ReturnEnemy(LaneEnemy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void SpawnEnemy()
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
                Debug.Log("Enemy spawn skipped: no available lane");
                return;
            }
        }
        else
        {
            lane = Random.Range(0, laneManager.LaneCount);
        }

        float xPos = laneManager.GetLaneXPosition(lane);

        // ランダムなHPを決定
        int hp = Random.Range(enemyMinHP, enemyMaxHP + 1);

        // プールから敵を取得
        LaneEnemy enemy = GetEnemyFromPool();
        enemy.transform.position = new Vector3(xPos, yPos, 0);
        enemy.Initialize(this, lane, hp);

        // スポーンを記録
        if (spawnCoordinator != null)
        {
            spawnCoordinator.RecordSpawn(lane, yPos);
        }

        Debug.Log($"Spawned enemy at lane {lane} with {hp} HP");
    }

    /// <summary>
    /// 指定したレーンに敵をスポーン
    /// </summary>
    public void SpawnEnemyAtLane(int lane, int hp = -1)
    {
        if (laneManager == null || scrollManager == null) return;
        if (!laneManager.IsValidLane(lane)) return;

        float xPos = laneManager.GetLaneXPosition(lane);
        float yPos = scrollManager.SpawnY;

        int actualHP = hp > 0 ? hp : Random.Range(enemyMinHP, enemyMaxHP + 1);

        LaneEnemy enemy = GetEnemyFromPool();
        enemy.transform.position = new Vector3(xPos, yPos, 0);
        enemy.Initialize(this, lane, actualHP);

        // スポーンを記録
        if (spawnCoordinator != null)
        {
            spawnCoordinator.RecordSpawn(lane, yPos);
        }
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
    /// 敵HP範囲を設定
    /// </summary>
    public void SetHPRange(int minHP, int maxHP)
    {
        enemyMinHP = Mathf.Max(1, minHP);
        enemyMaxHP = Mathf.Max(enemyMinHP, maxHP);
    }

    private Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }
}
