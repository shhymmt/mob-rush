using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Boss enemy that appears at the end of stages.
/// ステージ終盤に登場するボス敵。
/// </summary>
[RequireComponent(typeof(Health))]
public class LaneBoss : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Color bossColor = new Color(0.8f, 0.1f, 0.1f); // 濃い赤
    [SerializeField] private float bossSize = 1.5f;

    [Header("Attack Settings")]
    [SerializeField] private float phase1AttackInterval = 3f;
    [SerializeField] private float phase2AttackInterval = 2f;
    [SerializeField] private float projectileSpeed = 5f;

    [Header("Events")]
    public UnityEvent OnBossDefeated;
    public UnityEvent<int> OnPhaseChanged; // 1 or 2

    private Health health;
    private SpriteRenderer spriteRenderer;
    private BossSpawner spawner;
    private LaneManager laneManager;
    private int currentPhase = 1;
    private bool isAttacking = false;
    private Coroutine attackCoroutine;

    public Health Health => health;
    public int CurrentPhase => currentPhase;

    void Awake()
    {
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Healthイベントを購読
        if (health != null)
        {
            health.OnDeath.AddListener(OnDeath);
            health.OnHealthChanged.AddListener(OnHealthChanged);
        }
    }

    /// <summary>
    /// ボスを初期化
    /// </summary>
    public void Initialize(BossSpawner bossSpawner, int bossHP)
    {
        spawner = bossSpawner;
        laneManager = LaneManager.Instance;
        currentPhase = 1;
        isAttacking = false;

        gameObject.SetActive(true);

        // HPを設定
        if (health != null)
        {
            health.ResetWithHP(bossHP);
        }

        // ビジュアルを設定
        SetupVisual();

        // Colliderを設定
        SetupCollider();

        Debug.Log($"Boss initialized with HP: {bossHP}");

        // ボス出現音を再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(SoundManager.SoundType.BossAppear);
        }

        // 攻撃開始
        StartAttacking();
    }

    private void SetupVisual()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // 大きめの四角形スプライトを作成
        spriteRenderer.sprite = CreateBossSprite();
        spriteRenderer.color = bossColor;
        spriteRenderer.sortingOrder = 10;

        transform.localScale = Vector3.one * bossSize;
    }

    private Sprite CreateBossSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);

        // 角丸の四角形
        float cornerRadius = 8f;
        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = true;
                float border = 2f;

                // 角の判定
                if (x < cornerRadius && y < cornerRadius)
                {
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, cornerRadius)) < cornerRadius;
                }
                else if (x > size - cornerRadius && y < cornerRadius)
                {
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(size - cornerRadius, cornerRadius)) < cornerRadius;
                }
                else if (x < cornerRadius && y > size - cornerRadius)
                {
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, size - cornerRadius)) < cornerRadius;
                }
                else if (x > size - cornerRadius && y > size - cornerRadius)
                {
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(size - cornerRadius, size - cornerRadius)) < cornerRadius;
                }

                if (inside)
                {
                    // 外枠を少し明るく
                    if (x < border || x > size - border || y < border || y > size - border)
                    {
                        texture.SetPixel(x, y, new Color(1f, 0.3f, 0.3f));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
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
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);

        // Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void StartAttacking()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        isAttacking = true;
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    private void StopAttacking()
    {
        isAttacking = false;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private IEnumerator AttackRoutine()
    {
        // 最初の攻撃まで少し待つ
        yield return new WaitForSeconds(1f);

        while (isAttacking)
        {
            float interval = currentPhase == 1 ? phase1AttackInterval : phase2AttackInterval;

            // 攻撃実行
            if (currentPhase == 1)
            {
                FireSingleShot();
            }
            else
            {
                FireDoubleShot();
            }

            yield return new WaitForSeconds(interval);
        }
    }

    private void FireSingleShot()
    {
        if (laneManager == null) return;

        // 攻撃音を再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(SoundManager.SoundType.BossAttack);
        }

        // ランダムなレーンを選択
        int targetLane = Random.Range(0, laneManager.LaneCount);
        SpawnProjectile(targetLane);

        Debug.Log($"Boss fired single shot at lane {targetLane}");
    }

    private void FireDoubleShot()
    {
        if (laneManager == null) return;

        // 攻撃音を再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(SoundManager.SoundType.BossAttack);
        }

        // 全レーンに発射（2レーンの場合、片方だけ空ける）
        int safeLane = Random.Range(0, laneManager.LaneCount);

        for (int i = 0; i < laneManager.LaneCount; i++)
        {
            if (i != safeLane)
            {
                SpawnProjectile(i);
            }
        }

        Debug.Log($"Boss fired double shot, safe lane: {safeLane}");
    }

    private void SpawnProjectile(int targetLane)
    {
        if (spawner == null) return;

        float xPos = laneManager.GetLaneXPosition(targetLane);
        Vector3 spawnPos = new Vector3(xPos, transform.position.y - 0.5f, 0);

        spawner.SpawnProjectile(spawnPos, projectileSpeed);
    }

    private void OnHealthChanged(int current, int max)
    {
        // HPに応じて色を変える
        if (spriteRenderer != null)
        {
            float healthPercent = (float)current / max;
            spriteRenderer.color = Color.Lerp(Color.black, bossColor, 0.3f + healthPercent * 0.7f);
        }

        // Phase 2への移行チェック（HP 50%以下）
        if (currentPhase == 1 && current <= max / 2)
        {
            currentPhase = 2;
            OnPhaseChanged?.Invoke(2);
            Debug.Log("Boss entered Phase 2!");

            // 攻撃パターンを更新（コルーチンは自動的に新しい間隔を使用）
        }
    }

    private void OnDeath()
    {
        Debug.Log("Boss defeated!");
        StopAttacking();

        // 大爆発エフェクト
        if (EffectManager.Instance != null)
        {
            // 複数の爆発を発生
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0
                );
                EffectManager.Instance.PlayEnemyExplosion(transform.position + offset);
            }
        }

        // 爆発音
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(SoundManager.SoundType.Explosion);
        }

        OnBossDefeated?.Invoke();

        // 非アクティブ化
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnDeath);
            health.OnHealthChanged.RemoveListener(OnHealthChanged);
        }
    }
}
