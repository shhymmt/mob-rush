using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns bullets from the player unit group. Uses object pooling.
/// プレイヤーユニットグループから弾を発射。オブジェクトプーリング使用。
/// </summary>
public class BulletSpawner : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float fireRate = 0.5f; // 発射間隔（秒）
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private Color bulletColor = Color.yellow;
    [SerializeField] private float bulletSize = 0.2f;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 20;

    [Header("References")]
    [SerializeField] private PlayerUnitGroup playerUnitGroup;

    private List<Bullet> bulletPool = new List<Bullet>();
    private float fireTimer = 0f;
    private bool canFire = true;
    private GameObject bulletPrefab;

    void Start()
    {
        if (playerUnitGroup == null)
        {
            playerUnitGroup = FindFirstObjectByType<PlayerUnitGroup>();
        }

        // 弾プレファブを作成
        CreateBulletPrefab();

        // プールを初期化
        InitializePool();

        // PlayerUnitGroupのイベントを購読
        if (playerUnitGroup != null)
        {
            playerUnitGroup.OnStartedMoving.AddListener(OnPlayerStartedMoving);
            playerUnitGroup.OnStoppedMoving.AddListener(OnPlayerStoppedMoving);
        }
    }

    void Update()
    {
        if (!canFire) return;
        if (playerUnitGroup == null) return;
        if (playerUnitGroup.IsMoving) return; // 移動中は発射しない
        if (playerUnitGroup.UnitCount <= 0) return; // ユニットがいない

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            FireBullets();
            fireTimer = fireRate;
        }
    }

    private void CreateBulletPrefab()
    {
        bulletPrefab = new GameObject("BulletPrefab");
        bulletPrefab.SetActive(false);

        // SpriteRenderer
        SpriteRenderer sr = bulletPrefab.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = bulletColor;
        sr.sortingOrder = 3;

        // Bullet component
        Bullet bullet = bulletPrefab.AddComponent<Bullet>();

        // Collider
        CircleCollider2D collider = bulletPrefab.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        // Rigidbody2D（トリガー検出用）
        // Dynamic + Gravity 0 で確実に衝突検出
        Rigidbody2D rb = bulletPrefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 回転だけ固定

        bulletPrefab.transform.localScale = Vector3.one * bulletSize;

        // プレファブを非表示の親に移動
        bulletPrefab.transform.SetParent(transform);
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            Bullet bullet = CreateBullet();
            bullet.gameObject.SetActive(false);
            bulletPool.Add(bullet);
        }
    }

    private Bullet CreateBullet()
    {
        GameObject obj = Instantiate(bulletPrefab, transform);
        obj.name = "Bullet";
        return obj.GetComponent<Bullet>();
    }

    private Bullet GetBulletFromPool()
    {
        // 非アクティブな弾を探す
        foreach (var bullet in bulletPool)
        {
            if (!bullet.gameObject.activeInHierarchy)
            {
                return bullet;
            }
        }

        // プールにない場合は新規作成
        Bullet newBullet = CreateBullet();
        bulletPool.Add(newBullet);
        return newBullet;
    }

    /// <summary>
    /// 弾をプールに返す
    /// </summary>
    public void ReturnBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void FireBullets()
    {
        if (playerUnitGroup == null) return;

        int unitCount = playerUnitGroup.UnitCount;
        Vector3 spawnPosition = playerUnitGroup.transform.position + Vector3.up * 0.5f;

        // 発射音を再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(SoundManager.SoundType.Shoot);
        }

        // ユニット数分の弾を発射（横に少しずらす）
        float spreadWidth = Mathf.Min(unitCount - 1, 5) * 0.15f;

        for (int i = 0; i < unitCount; i++)
        {
            Bullet bullet = GetBulletFromPool();

            // 横方向にばらけさせる
            float xOffset = 0;
            if (unitCount > 1)
            {
                xOffset = Mathf.Lerp(-spreadWidth, spreadWidth, (float)i / (unitCount - 1));
            }

            Vector3 bulletPos = spawnPosition + new Vector3(xOffset, 0, 0);
            bullet.Initialize(this, bulletPos, 1); // 1ダメージ/弾
        }

        // Debug.Log($"Fired {unitCount} bullets");
    }

    private void OnPlayerStartedMoving()
    {
        // 移動開始時は発射を停止（Updateで制御しているので特に何もしない）
        fireTimer = 0; // 移動後すぐに発射できるようにリセット
    }

    private void OnPlayerStoppedMoving()
    {
        // 移動停止後、少し待ってから発射再開
        fireTimer = 0.2f;
    }

    private Sprite CreateCircleSprite()
    {
        int size = 16;
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
        if (playerUnitGroup != null)
        {
            playerUnitGroup.OnStartedMoving.RemoveListener(OnPlayerStartedMoving);
            playerUnitGroup.OnStoppedMoving.RemoveListener(OnPlayerStoppedMoving);
        }
    }
}
