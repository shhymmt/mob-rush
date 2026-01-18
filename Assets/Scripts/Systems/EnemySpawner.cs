using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Settings")]
    [SerializeField] private float spawnInterval = 1.2f; // 敵スポーン間隔（秒）
    [SerializeField] private int poolSize = 100;
    [SerializeField] private float initialDelay = 1.5f; // ゲーム開始後の最初のスポーンまでの遅延

    private ObjectPool<GameObject> enemyPool;
    private float spawnTimer;
    private bool isSpawning = true;

    void Awake()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        enemyPool = new ObjectPool<GameObject>(
            createFunc: CreateEnemy,
            actionOnGet: OnGetEnemy,
            actionOnRelease: OnReleaseEnemy,
            actionOnDestroy: OnDestroyEnemy,
            defaultCapacity: poolSize,
            maxSize: poolSize
        );
    }

    void Start()
    {
        // 最初のスポーンは少し遅延させる
        spawnTimer = spawnInterval - initialDelay;
    }

    void Update()
    {
        if (!isSpawning) return;

        // ゲームがプレイ中でない場合はスポーンしない
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        Vector3 position = spawnPoint.position;
        // わずかなランダム散布
        position.x += Random.Range(-0.5f, 0.5f);

        var enemyObj = enemyPool.Get();
        enemyObj.transform.position = position;
    }

    private GameObject CreateEnemy()
    {
        var obj = Instantiate(enemyPrefab);
        obj.SetActive(false);
        return obj;
    }

    private void OnGetEnemy(GameObject obj)
    {
        obj.SetActive(true);
        var enemy = obj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Initialize(this);
        }
    }

    private void OnReleaseEnemy(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyEnemy(GameObject obj)
    {
        Destroy(obj);
    }

    public void ReturnToPool(GameObject enemyObj)
    {
        enemyPool.Release(enemyObj);
    }

    // スポーンの開始・停止
    public void SetSpawning(bool enabled)
    {
        isSpawning = enabled;
    }

    // スポーン間隔の調整（難易度調整用）
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }
}
