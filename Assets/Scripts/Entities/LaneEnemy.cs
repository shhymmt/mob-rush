using UnityEngine;

/// <summary>
/// Enemy that scrolls down in a lane. Has HP and can be damaged by bullets.
/// レーン内を下にスクロールする敵。HPを持ち、弾でダメージを受ける。
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Scrollable))]
public class LaneEnemy : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int baseHP = 10;

    [Header("Visual")]
    [SerializeField] private Color enemyColor = Color.red;

    private Health health;
    private Scrollable scrollable;
    private LaneEnemySpawner spawner;
    private SpriteRenderer spriteRenderer;
    private int currentLane;

    public int CurrentLane => currentLane;
    public Health Health => health;

    void Awake()
    {
        health = GetComponent<Health>();
        scrollable = GetComponent<Scrollable>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Healthイベントを購読
        if (health != null)
        {
            health.OnDeath.AddListener(OnDeath);
            health.OnHealthChanged.AddListener(OnHealthChanged);
        }

        // Scrollableイベントを購読
        if (scrollable != null)
        {
            scrollable.OnDespawned.AddListener(OnScrolledOffScreen);
        }
    }

    /// <summary>
    /// 敵を初期化
    /// </summary>
    public void Initialize(LaneEnemySpawner enemySpawner, int lane, int hp)
    {
        spawner = enemySpawner;
        currentLane = lane;

        // 先にGameObjectを有効化（Awakeが走るように）
        gameObject.SetActive(true);

        // HPを設定（ResetWithHPで確実にリセット）
        if (health != null)
        {
            health.ResetWithHP(hp);
        }

        // Scrollableを有効化
        if (scrollable != null)
        {
            scrollable.Activate();
        }

        // 色をリセット
        if (spriteRenderer != null)
        {
            spriteRenderer.color = enemyColor;
        }

        Debug.Log($"Enemy initialized: lane={lane}, hp={hp}");
    }

    /// <summary>
    /// プールに返す
    /// </summary>
    public void ReturnToPool()
    {
        if (scrollable != null)
        {
            scrollable.Deactivate();
        }

        if (spawner != null)
        {
            spawner.ReturnEnemy(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDeath()
    {
        Debug.Log($"Enemy destroyed! Lane: {currentLane}");

        // 爆発エフェクトを再生
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayEnemyExplosion(transform.position);
        }

        // 爆発音を再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(SoundManager.SoundType.Explosion);
        }

        ReturnToPool();
    }

    private void OnHealthChanged(int current, int max)
    {
        // HPに応じて色を変える（ダメージフィードバック）
        if (spriteRenderer != null)
        {
            float healthPercent = (float)current / max;
            // HPが減るほど暗くなる
            spriteRenderer.color = Color.Lerp(Color.black, enemyColor, 0.3f + healthPercent * 0.7f);
        }
    }

    private void OnScrolledOffScreen()
    {
        // 画面外にスクロールした = プレイヤーを通過した
        // これはゲームオーバーではない（弾で倒せなかった場合）
        // ゲームオーバーはプレイヤーとの衝突で発生
        Debug.Log($"Enemy scrolled off screen. Lane: {currentLane}");
        ReturnToPool();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"LaneEnemy OnTriggerEnter2D: collided with {other.gameObject.name}");

        // プレイヤーユニットグループとの衝突
        var playerGroup = other.GetComponent<PlayerUnitGroup>();
        if (playerGroup != null)
        {
            Debug.Log("Enemy collided with PlayerUnitGroup! Triggering Game Over...");

            // GameManagerに通知
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                Debug.Log("GameManager found, calling TriggerLose()");
                gameManager.TriggerLose();
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is NULL! Cannot trigger game over.");
            }

            ReturnToPool();
            return;
        }

        // 弾との衝突もここでログ
        if (other.GetComponent<Bullet>() != null)
        {
            Debug.Log("Enemy hit by bullet");
        }
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnDeath);
            health.OnHealthChanged.RemoveListener(OnHealthChanged);
        }

        if (scrollable != null)
        {
            scrollable.OnDespawned.RemoveListener(OnScrolledOffScreen);
        }
    }
}
