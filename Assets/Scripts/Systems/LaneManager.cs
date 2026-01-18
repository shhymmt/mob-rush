using UnityEngine;

/// <summary>
/// Manages lane positions and provides lane-related utilities.
/// 2レーンを管理し、レーン関連のユーティリティを提供する。
/// </summary>
public class LaneManager : MonoBehaviour
{
    public static LaneManager Instance { get; private set; }

    [Header("Lane Settings")]
    [SerializeField] private int laneCount = 2;
    [SerializeField] private float laneWidth = 2f;
    [SerializeField] private float laneSpacing = 0.5f; // レーン間の間隔

    [Header("Visual Settings")]
    [SerializeField] private Color lane1Color = new Color(0.2f, 0.4f, 0.8f, 0.3f);
    [SerializeField] private Color lane2Color = new Color(0.8f, 0.4f, 0.2f, 0.3f);
    [SerializeField] private bool showLaneIndicators = true;

    private float[] laneXPositions;
    private GameObject[] laneIndicators;

    public int LaneCount => laneCount;
    public float LaneWidth => laneWidth;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CalculateLanePositions();
    }

    void Start()
    {
        if (showLaneIndicators)
        {
            CreateLaneIndicators();
        }
    }

    private void CalculateLanePositions()
    {
        laneXPositions = new float[laneCount];

        // 2レーンの場合: 左レーン(-1.25), 右レーン(1.25)
        // 中央を0として、左右に配置
        float totalWidth = (laneCount - 1) * (laneWidth + laneSpacing);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < laneCount; i++)
        {
            laneXPositions[i] = startX + i * (laneWidth + laneSpacing);
        }

        Debug.Log($"Lane positions: Lane 0 = {laneXPositions[0]}, Lane 1 = {laneXPositions[1]}");
    }

    private void CreateLaneIndicators()
    {
        laneIndicators = new GameObject[laneCount];
        Color[] colors = { lane1Color, lane2Color };

        for (int i = 0; i < laneCount; i++)
        {
            // レーンインジケータ用のGameObjectを作成
            GameObject indicator = new GameObject($"LaneIndicator_{i}");
            indicator.transform.SetParent(transform);
            indicator.transform.position = new Vector3(laneXPositions[i], 0, 0);

            // SpriteRendererを追加（シンプルな長方形）
            SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRectSprite();
            sr.color = colors[i % colors.Length];
            sr.sortingOrder = -10; // 背景に配置

            // スケールを調整（幅=laneWidth, 高さ=画面全体）
            indicator.transform.localScale = new Vector3(laneWidth, 15f, 1f);

            laneIndicators[i] = indicator;
        }
    }

    private Sprite CreateRectSprite()
    {
        // 1x1の白いテクスチャを作成
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    /// <summary>
    /// 指定したレーン番号のX座標を取得
    /// </summary>
    public float GetLaneXPosition(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= laneCount)
        {
            Debug.LogWarning($"Invalid lane index: {laneIndex}. Returning lane 0.");
            return laneXPositions[0];
        }
        return laneXPositions[laneIndex];
    }

    /// <summary>
    /// ワールドX座標から最も近いレーン番号を取得
    /// </summary>
    public int GetLaneFromWorldX(float worldX)
    {
        int closestLane = 0;
        float closestDistance = Mathf.Abs(worldX - laneXPositions[0]);

        for (int i = 1; i < laneCount; i++)
        {
            float distance = Mathf.Abs(worldX - laneXPositions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestLane = i;
            }
        }

        return closestLane;
    }

    /// <summary>
    /// スクリーン座標からレーン番号を取得
    /// </summary>
    public int GetLaneFromScreenPosition(Vector2 screenPosition, Camera camera)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        return GetLaneFromWorldX(worldPos.x);
    }

    /// <summary>
    /// 指定したレーンが有効かどうか
    /// </summary>
    public bool IsValidLane(int laneIndex)
    {
        return laneIndex >= 0 && laneIndex < laneCount;
    }
}
