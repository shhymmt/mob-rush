using UnityEngine;

/// <summary>
/// Projectile fired by the boss. Moves downward and damages player.
/// ボスが発射する弾。下に移動してプレイヤーにダメージを与える。
/// </summary>
public class BossProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float despawnY = -6f;
    [SerializeField] private Color projectileColor = new Color(1f, 0.3f, 0.3f); // 赤系
    [SerializeField] private float projectileSize = 0.3f;

    private BossSpawner spawner;
    private bool isActive = false;

    void Update()
    {
        if (!isActive) return;

        // 下に移動
        transform.position += Vector3.down * speed * Time.deltaTime;

        // 画面外チェック
        if (transform.position.y < despawnY)
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// プロジェクタイルを初期化して発射
    /// </summary>
    public void Initialize(BossSpawner bossSpawner, Vector3 position, float projectileSpeed)
    {
        spawner = bossSpawner;
        transform.position = position;
        speed = projectileSpeed;
        isActive = true;
        gameObject.SetActive(true);

        // ビジュアルを設定
        SetupVisual();
        SetupCollider();
    }

    private void SetupVisual()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        sr.sprite = CreateProjectileSprite();
        sr.color = projectileColor;
        sr.sortingOrder = 8;

        transform.localScale = Vector3.one * projectileSize;
    }

    private Sprite CreateProjectileSprite()
    {
        // ダイヤモンド型（ひし形）のスプライト
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // マンハッタン距離でひし形を描画
                float dist = Mathf.Abs(x - center.x) + Mathf.Abs(y - center.y);
                float maxDist = size / 2f;

                if (dist < maxDist - 2)
                {
                    // 内側：グラデーション
                    float intensity = 1f - (dist / maxDist) * 0.3f;
                    texture.SetPixel(x, y, new Color(intensity, intensity, intensity, 1f));
                }
                else if (dist < maxDist)
                {
                    // エッジ：アンチエイリアス
                    float alpha = maxDist - dist;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
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

    private void SetupCollider()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }
        collider.isTrigger = true;
        collider.radius = 0.4f;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    /// <summary>
    /// プールに返す
    /// </summary>
    public void ReturnToPool()
    {
        isActive = false;

        if (spawner != null)
        {
            spawner.ReturnProjectile(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        // プレイヤーユニットグループとの衝突
        var playerGroup = other.GetComponent<PlayerUnitGroup>();
        if (playerGroup != null)
        {
            Debug.Log("Boss projectile hit player!");

            // ヒットエフェクト
            if (EffectManager.Instance != null)
            {
                EffectManager.Instance.PlayHitSpark(transform.position);
            }

            // ヒット音
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySE(SoundManager.SoundType.Hit);
            }

            // プレイヤーにダメージ（ユニットを減らす）
            playerGroup.AddUnits(-3); // 3体減少

            ReturnToPool();
        }
    }
}
