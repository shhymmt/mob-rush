using UnityEngine;

/// <summary>
/// Bullet fired by player units. Moves upward and deals damage to enemies.
/// プレイヤーユニットが発射する弾。上に移動して敵にダメージを与える。
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float despawnY = 8f;

    private BulletSpawner spawner;
    private bool isActive = false;

    public int Damage => damage;

    void Update()
    {
        if (!isActive) return;

        // 上に移動
        transform.position += Vector3.up * speed * Time.deltaTime;

        // 画面外チェック
        if (transform.position.y > despawnY)
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// 弾を初期化して発射
    /// </summary>
    public void Initialize(BulletSpawner bulletSpawner, Vector3 position, int bulletDamage = 1)
    {
        spawner = bulletSpawner;
        transform.position = position;
        damage = bulletDamage;
        isActive = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// プールに返す
    /// </summary>
    public void ReturnToPool()
    {
        isActive = false;

        if (spawner != null)
        {
            spawner.ReturnBullet(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        // 敵との衝突（Enemyタグ）
        if (other.CompareTag("Enemy"))
        {
            // Healthコンポーネントを持つ敵にダメージ
            var health = other.GetComponent<Health>();
            if (health != null)
            {
                // ヒットスパークエフェクトを再生
                if (EffectManager.Instance != null)
                {
                    EffectManager.Instance.PlayHitSpark(transform.position);
                }

                // ヒット音を再生
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySE(SoundManager.SoundType.Hit);
                }

                health.TakeDamage(damage);
                ReturnToPool();
            }
        }
    }
}
