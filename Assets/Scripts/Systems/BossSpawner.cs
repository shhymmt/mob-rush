using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Spawns and manages the boss and its projectiles.
/// ボスとその弾を生成・管理する。
/// </summary>
public class BossSpawner : MonoBehaviour
{
    public static BossSpawner Instance { get; private set; }

    [Header("Boss Settings")]
    [SerializeField] private int baseHP = 50;
    [SerializeField] private int hpPerStage = 50;
    [SerializeField] private float bossSpawnY = 4f;

    [Header("Projectile Pool")]
    [SerializeField] private int initialProjectilePool = 10;

    [Header("Events")]
    public UnityEvent OnBossSpawned;
    public UnityEvent OnBossDefeated;

    private LaneBoss currentBoss;
    private GameObject bossPrefab;
    private GameObject projectilePrefab;
    private List<BossProjectile> projectilePool = new List<BossProjectile>();

    public LaneBoss CurrentBoss => currentBoss;
    public bool IsBossActive => currentBoss != null && currentBoss.gameObject.activeInHierarchy;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CreatePrefabs();
        InitializeProjectilePool();
    }

    private void CreatePrefabs()
    {
        // ボスプレファブを作成
        bossPrefab = new GameObject("BossPrefab");
        bossPrefab.SetActive(false);
        bossPrefab.AddComponent<LaneBoss>();
        bossPrefab.AddComponent<Health>();
        bossPrefab.tag = "Enemy"; // 弾との衝突判定用
        bossPrefab.transform.SetParent(transform);

        // プロジェクタイルプレファブを作成
        projectilePrefab = new GameObject("BossProjectilePrefab");
        projectilePrefab.SetActive(false);
        projectilePrefab.AddComponent<BossProjectile>();
        projectilePrefab.transform.SetParent(transform);
    }

    private void InitializeProjectilePool()
    {
        for (int i = 0; i < initialProjectilePool; i++)
        {
            BossProjectile projectile = CreateProjectile();
            projectile.gameObject.SetActive(false);
            projectilePool.Add(projectile);
        }
    }

    private BossProjectile CreateProjectile()
    {
        GameObject obj = Instantiate(projectilePrefab, transform);
        obj.name = "BossProjectile";
        return obj.GetComponent<BossProjectile>();
    }

    /// <summary>
    /// ステージに応じたボスをスポーン
    /// </summary>
    public LaneBoss SpawnBoss(int stageNumber)
    {
        if (currentBoss != null && currentBoss.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Boss already exists!");
            return currentBoss;
        }

        // ボスHPを計算
        int bossHP = CalculateBossHP(stageNumber);

        // ボスを生成
        if (currentBoss == null)
        {
            GameObject bossObj = Instantiate(bossPrefab, transform);
            bossObj.name = "LaneBoss";
            currentBoss = bossObj.GetComponent<LaneBoss>();

            // イベントを購読
            currentBoss.OnBossDefeated.AddListener(OnBossDefeatedHandler);
        }

        // 位置を設定（画面上部中央）
        currentBoss.transform.position = new Vector3(0, bossSpawnY, 0);

        // 初期化
        currentBoss.Initialize(this, bossHP);

        Debug.Log($"Boss spawned for stage {stageNumber} with HP: {bossHP}");

        OnBossSpawned?.Invoke();

        return currentBoss;
    }

    /// <summary>
    /// ステージに応じたボスHPを計算
    /// </summary>
    private int CalculateBossHP(int stageNumber)
    {
        // Stage 1: 50, Stage 2: 100, Stage 3: 150, Stage 4+: 150 + (stage-3)*50
        if (stageNumber <= 3)
        {
            return baseHP * stageNumber;
        }
        else
        {
            return 150 + (stageNumber - 3) * hpPerStage;
        }
    }

    /// <summary>
    /// ボス弾をスポーン
    /// </summary>
    public void SpawnProjectile(Vector3 position, float speed)
    {
        BossProjectile projectile = GetProjectileFromPool();
        projectile.Initialize(this, position, speed);
    }

    private BossProjectile GetProjectileFromPool()
    {
        foreach (var projectile in projectilePool)
        {
            if (!projectile.gameObject.activeInHierarchy)
            {
                return projectile;
            }
        }

        // プールにない場合は新規作成
        BossProjectile newProjectile = CreateProjectile();
        projectilePool.Add(newProjectile);
        return newProjectile;
    }

    /// <summary>
    /// プロジェクタイルをプールに返す
    /// </summary>
    public void ReturnProjectile(BossProjectile projectile)
    {
        projectile.gameObject.SetActive(false);
    }

    private void OnBossDefeatedHandler()
    {
        Debug.Log("Boss defeated! Triggering win...");
        OnBossDefeated?.Invoke();

        // GameManagerに勝利を通知
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerWinExternal();
        }
    }

    /// <summary>
    /// 全てのボス弾を消去
    /// </summary>
    public void ClearAllProjectiles()
    {
        foreach (var projectile in projectilePool)
        {
            if (projectile != null && projectile.gameObject.activeInHierarchy)
            {
                projectile.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ボスを非アクティブ化
    /// </summary>
    public void DeactivateBoss()
    {
        if (currentBoss != null)
        {
            currentBoss.gameObject.SetActive(false);
        }
        ClearAllProjectiles();
    }
}
