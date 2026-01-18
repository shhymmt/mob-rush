using UnityEngine;

/// <summary>
/// Test spawner for scrolling system. Creates simple objects that scroll down.
/// スクロールシステムのテスト用スポナー。下にスクロールするシンプルなオブジェクトを生成。
/// </summary>
public class ScrollTestSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool spawnEnabled = true;

    [Header("References")]
    [SerializeField] private LaneManager laneManager;
    [SerializeField] private ScrollManager scrollManager;

    private float spawnTimer;

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

        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (!spawnEnabled) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnTestObject();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnTestObject()
    {
        if (laneManager == null || scrollManager == null) return;

        // ランダムなレーンを選択
        int lane = Random.Range(0, laneManager.LaneCount);
        float xPos = laneManager.GetLaneXPosition(lane);
        float yPos = scrollManager.SpawnY;

        // テストオブジェクトを作成
        GameObject testObj = new GameObject("ScrollTestObject");
        testObj.transform.position = new Vector3(xPos, yPos, 0);

        // SpriteRendererを追加（赤い四角）
        SpriteRenderer sr = testObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = Color.red;
        sr.sortingOrder = 1;
        testObj.transform.localScale = Vector3.one * 0.8f;

        // Scrollableコンポーネントを追加
        Scrollable scrollable = testObj.AddComponent<Scrollable>();

        // BoxCollider2Dを追加（後で衝突検出用）
        BoxCollider2D collider = testObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Debug.Log($"Spawned test object at lane {lane}");
    }

    private Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    /// <summary>
    /// スポーンを有効/無効にする
    /// </summary>
    public void SetSpawnEnabled(bool enabled)
    {
        spawnEnabled = enabled;
    }
}
