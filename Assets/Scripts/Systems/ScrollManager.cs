using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the scroll speed for all scrollable objects.
/// すべてのスクロール可能オブジェクトのスクロール速度を管理する。
/// </summary>
public class ScrollManager : MonoBehaviour
{
    public static ScrollManager Instance { get; private set; }

    [Header("Scroll Settings")]
    [SerializeField] private float baseScrollSpeed = 2f;
    [SerializeField] private float currentSpeedMultiplier = 1f;

    [Header("Boundaries")]
    [SerializeField] private float spawnY = 7f;      // スポーン位置（画面上）
    [SerializeField] private float despawnY = -6f;   // デスポーン位置（画面下）

    [Header("Events")]
    public UnityEvent<float> OnScrollSpeedChanged;

    private bool isScrolling = true;

    public float ScrollSpeed => baseScrollSpeed * currentSpeedMultiplier;
    public float SpawnY => spawnY;
    public float DespawnY => despawnY;
    public bool IsScrolling => isScrolling;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// スクロール速度の倍率を設定（ステージレベルで調整用）
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Max(0.1f, multiplier);
        OnScrollSpeedChanged?.Invoke(ScrollSpeed);
        Debug.Log($"Scroll speed changed: {ScrollSpeed}");
    }

    /// <summary>
    /// 基本スクロール速度を設定
    /// </summary>
    public void SetBaseSpeed(float speed)
    {
        baseScrollSpeed = Mathf.Max(0.1f, speed);
        OnScrollSpeedChanged?.Invoke(ScrollSpeed);
    }

    /// <summary>
    /// スクロール速度を設定（SetBaseSpeedのエイリアス）
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        SetBaseSpeed(speed);
    }

    /// <summary>
    /// スクロールを一時停止/再開
    /// </summary>
    public void SetScrolling(bool enabled)
    {
        isScrolling = enabled;
    }

    /// <summary>
    /// オブジェクトが画面外（下）かどうかをチェック
    /// </summary>
    public bool IsOffScreenBottom(float yPosition)
    {
        return yPosition < despawnY;
    }

    /// <summary>
    /// オブジェクトが画面外（上）かどうかをチェック
    /// </summary>
    public bool IsOffScreenTop(float yPosition)
    {
        return yPosition > spawnY;
    }
}
