using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Makes an object scroll downward. Attach to enemies, gates, etc.
/// オブジェクトを下方向にスクロールさせる。敵やゲートにアタッチ。
/// </summary>
public class Scrollable : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool useGlobalSpeed = true;
    [SerializeField] private float customSpeed = 2f;
    [SerializeField] private bool destroyOnDespawn = false; // falseならプールに返す

    [Header("Events")]
    public UnityEvent OnDespawned;

    private ScrollManager scrollManager;
    private bool isActive = true;

    public bool IsActive => isActive;

    void Start()
    {
        scrollManager = ScrollManager.Instance;
    }

    void Update()
    {
        if (!isActive) return;

        // スクロール処理
        float speed = GetScrollSpeed();
        if (speed > 0)
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
        }

        // 画面外チェック
        CheckDespawn();
    }

    private float GetScrollSpeed()
    {
        if (scrollManager == null)
        {
            scrollManager = ScrollManager.Instance;
        }

        if (scrollManager != null && !scrollManager.IsScrolling)
        {
            return 0f;
        }

        if (useGlobalSpeed && scrollManager != null)
        {
            return scrollManager.ScrollSpeed;
        }

        return customSpeed;
    }

    private void CheckDespawn()
    {
        if (scrollManager == null)
        {
            scrollManager = ScrollManager.Instance;
        }

        float despawnY = scrollManager != null ? scrollManager.DespawnY : -6f;

        if (transform.position.y < despawnY)
        {
            Despawn();
        }
    }

    /// <summary>
    /// デスポーン処理
    /// </summary>
    public void Despawn()
    {
        OnDespawned?.Invoke();

        if (destroyOnDespawn)
        {
            Destroy(gameObject);
        }
        else
        {
            // プールに返す（または無効化）
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// オブジェクトを有効化（プールから再利用時）
    /// </summary>
    public void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// オブジェクトを無効化
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
    }

    /// <summary>
    /// カスタム速度を設定
    /// </summary>
    public void SetCustomSpeed(float speed)
    {
        customSpeed = speed;
        useGlobalSpeed = false;
    }

    /// <summary>
    /// グローバル速度を使用するように設定
    /// </summary>
    public void UseGlobalSpeed()
    {
        useGlobalSpeed = true;
    }
}
