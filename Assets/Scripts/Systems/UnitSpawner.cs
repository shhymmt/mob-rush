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

    // 生成時に渡すゲート（ゲートから生成された場合のみ使用）
    private Gate currentSpawnGate = null;
    // 生成時に渡す方向
    private Vector2 currentSpawnDirection = Vector2.up;

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

    void Start()
    {
        // InputHandlerにキャノンの位置を設定
        if (inputHandler != null && spawnPoint != null)
        {
            inputHandler.SetCannonTransform(spawnPoint);
        }
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
        // リソースチェック
        if (GameManager.Instance != null && !GameManager.Instance.TryConsumeSpawn())
        {
            return; // リソース不足またはゲーム終了
        }

        Vector3 position = spawnPoint.position;
        // わずかなランダム散布を追加
        position.x += Random.Range(-0.2f, 0.2f);

        currentSpawnGate = null; // 通常生成はゲートなし
        currentSpawnDirection = inputHandler.AimDirection; // 現在の照準方向
        var unitObj = unitPool.Get();
        unitObj.transform.position = position;
    }

    // ゲートから呼ばれる：特定位置でユニットを生成（ゲートを通過済みとしてマーク）
    public GameObject SpawnUnitAtPosition(Vector3 position, Gate fromGate, Vector2 direction)
    {
        currentSpawnGate = fromGate; // このゲートを通過済みとして設定
        currentSpawnDirection = direction; // 元のユニットと同じ方向
        var unitObj = unitPool.Get();
        if (unitObj != null)
        {
            unitObj.transform.position = position;
        }
        currentSpawnGate = null;
        return unitObj;
    }

    // 後方互換性のためのオーバーロード（方向なし = 上向き）
    public GameObject SpawnUnitAtPosition(Vector3 position, Gate fromGate)
    {
        return SpawnUnitAtPosition(position, fromGate, Vector2.up);
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
        unit.Initialize(this, currentSpawnDirection, currentSpawnGate);
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
