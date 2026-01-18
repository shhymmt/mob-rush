using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 4f;
    [SerializeField] private float despawnY = -6f; // 画面下端より下

    private Rigidbody2D rb;
    private EnemySpawner spawner;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
        // 敵は下に向かって移動
        rb.linearVelocity = Vector2.down * speed;
    }

    void Update()
    {
        // 画面下端に到達したかチェック
        if (transform.position.y < despawnY)
        {
            // 敗北ラインに到達 - GameManagerに通知
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnEnemyReachedBottom();
            }
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        rb.linearVelocity = Vector2.zero;

        if (spawner != null)
        {
            spawner.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
