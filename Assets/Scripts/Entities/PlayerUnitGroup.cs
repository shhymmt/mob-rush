using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Represents the player's unit group that moves between lanes.
/// プレイヤーのユニットグループを表す。レーン間を移動する。
/// </summary>
public class PlayerUnitGroup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int initialUnitCount = 3;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private int startingLane = 0;

    [Header("Visual Settings")]
    [SerializeField] private Color unitColor = new Color(0.2f, 0.6f, 1f); // 青
    [SerializeField] private float unitSize = 0.4f;
    [SerializeField] private float unitSpacing = 0.3f;

    [Header("References")]
    [SerializeField] private LaneManager laneManager;
    [SerializeField] private LaneInputHandler inputHandler;

    [Header("Events")]
    public UnityEvent<int> OnUnitCountChanged;
    public UnityEvent OnStartedMoving;
    public UnityEvent OnStoppedMoving;

    private int currentLane;
    private int unitCount;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private GameObject[] unitVisuals;

    public int CurrentLane => currentLane;
    public int UnitCount => unitCount;
    public bool IsMoving => isMoving;

    void Start()
    {
        // 参照を取得
        if (laneManager == null)
        {
            laneManager = LaneManager.Instance;
        }

        if (inputHandler == null)
        {
            inputHandler = FindFirstObjectByType<LaneInputHandler>();
        }

        // 入力ハンドラーのイベントを購読
        if (inputHandler != null)
        {
            inputHandler.OnLaneTapped.AddListener(OnLaneTapped);
        }

        // 初期化
        unitCount = initialUnitCount;
        currentLane = startingLane;

        // 初期位置を設定
        if (laneManager != null)
        {
            float xPos = laneManager.GetLaneXPosition(currentLane);
            transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
            targetPosition = transform.position;
        }

        // Colliderを追加（敵との衝突検出用）
        SetupCollider();

        // ビジュアルを作成
        CreateUnitVisuals();

        // イベント発火
        OnUnitCountChanged?.Invoke(unitCount);
    }

    private void SetupCollider()
    {
        // BoxCollider2Dがなければ追加
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);

        // Rigidbody2Dがなければ追加（トリガー検出に必要）
        // Dynamic + Gravity 0 で衝突検出を確実にする
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic; // Kinematicから変更
        rb.constraints = RigidbodyConstraints2D.FreezeAll; // 物理で動かないように固定
    }

    void Update()
    {
        // 移動処理
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // 移動完了チェック
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                OnStoppedMoving?.Invoke();
                Debug.Log($"Stopped moving. Now at lane {currentLane}");
            }
        }
    }

    private void OnLaneTapped(int laneIndex)
    {
        if (laneManager == null) return;
        if (!laneManager.IsValidLane(laneIndex)) return;

        // 同じレーンをタップした場合は何もしない
        if (laneIndex == currentLane && !isMoving) return;

        // 移動開始
        MoveToLane(laneIndex);
    }

    public void MoveToLane(int laneIndex)
    {
        if (laneManager == null) return;

        currentLane = laneIndex;
        float xPos = laneManager.GetLaneXPosition(laneIndex);
        targetPosition = new Vector3(xPos, transform.position.y, transform.position.z);

        if (!isMoving)
        {
            isMoving = true;
            OnStartedMoving?.Invoke();
            Debug.Log($"Started moving to lane {laneIndex}");
        }
    }

    /// <summary>
    /// ユニット数を追加（ゲートから）
    /// </summary>
    public void AddUnits(int count)
    {
        int previousCount = unitCount;
        unitCount += count;
        if (unitCount < 0) unitCount = 0;

        UpdateUnitVisuals();
        OnUnitCountChanged?.Invoke(unitCount);

        Debug.Log($"Units changed: now {unitCount}");

        // ビジュアルフィードバック
        if (count > 0)
        {
            // 増加: 緑フラッシュ + スケールアップ
            StartCoroutine(PulseScale(1.3f, 0.2f));
        }
        else if (count < 0)
        {
            // 減少: 赤フラッシュ
            StartCoroutine(FlashColor(Color.red, 0.2f));
        }

        // ユニット0でゲームオーバー
        if (unitCount <= 0)
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.TriggerLose();
            }
        }
    }

    /// <summary>
    /// スケールパルスアニメーション
    /// </summary>
    private IEnumerator PulseScale(float targetScale, float duration)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 peakScale = originalScale * targetScale;
        float halfDuration = duration / 2f;

        // Scale up
        float timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale, peakScale, t);
            yield return null;
        }

        // Scale down
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;
            transform.localScale = Vector3.Lerp(peakScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    /// <summary>
    /// カラーフラッシュアニメーション
    /// </summary>
    private IEnumerator FlashColor(Color flashColor, float duration)
    {
        if (unitVisuals == null) yield break;

        // Get original colors and set flash color
        Color[] originalColors = new Color[unitVisuals.Length];
        for (int i = 0; i < unitVisuals.Length; i++)
        {
            if (unitVisuals[i] != null)
            {
                var sr = unitVisuals[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    originalColors[i] = sr.color;
                    sr.color = flashColor;
                }
            }
        }

        yield return new WaitForSeconds(duration);

        // Restore original colors
        for (int i = 0; i < unitVisuals.Length; i++)
        {
            if (unitVisuals[i] != null)
            {
                var sr = unitVisuals[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = originalColors[i];
                }
            }
        }
    }

    /// <summary>
    /// ユニット数を設定
    /// </summary>
    public void SetUnitCount(int count)
    {
        unitCount = Mathf.Max(0, count);
        UpdateUnitVisuals();
        OnUnitCountChanged?.Invoke(unitCount);
    }

    private void CreateUnitVisuals()
    {
        // 最大表示数を制限（パフォーマンスのため）
        int maxVisuals = 10;
        unitVisuals = new GameObject[maxVisuals];

        for (int i = 0; i < maxVisuals; i++)
        {
            GameObject visual = new GameObject($"UnitVisual_{i}");
            visual.transform.SetParent(transform);

            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = unitColor;
            sr.sortingOrder = 5;

            visual.transform.localScale = Vector3.one * unitSize;
            visual.SetActive(false);

            unitVisuals[i] = visual;
        }

        UpdateUnitVisuals();
    }

    private void UpdateUnitVisuals()
    {
        if (unitVisuals == null) return;

        int displayCount = Mathf.Min(unitCount, unitVisuals.Length);

        // 配置パターン（三角形状）
        for (int i = 0; i < unitVisuals.Length; i++)
        {
            if (i < displayCount)
            {
                unitVisuals[i].SetActive(true);

                // 簡単な配置：下から上に並べる
                int row = 0;
                int col = 0;
                int index = i;

                // 各行のユニット数: 1, 2, 3, 4...
                int rowStart = 0;
                int rowSize = 1;
                while (index >= rowStart + rowSize)
                {
                    rowStart += rowSize;
                    row++;
                    rowSize = row + 1;
                }
                col = index - rowStart;

                float xOffset = (col - (rowSize - 1) / 2f) * unitSpacing;
                float yOffset = row * unitSpacing;

                unitVisuals[i].transform.localPosition = new Vector3(xOffset, yOffset, 0);
            }
            else
            {
                unitVisuals[i].SetActive(false);
            }
        }
    }

    private Sprite CreateCircleSprite()
    {
        // シンプルな円形テクスチャを作成
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance < radius - 1)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else if (distance < radius)
                {
                    float alpha = radius - distance;
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void OnDestroy()
    {
        // イベント購読解除
        if (inputHandler != null)
        {
            inputHandler.OnLaneTapped.RemoveListener(OnLaneTapped);
        }
    }
}
