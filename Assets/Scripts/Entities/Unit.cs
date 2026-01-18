using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float despawnY = 6f; // 画面上端より上
    [SerializeField] private float despawnX = 10f; // 画面左右端より外

    private Rigidbody2D rb;
    private UnitSpawner spawner;
    private HashSet<Gate> passedGates = new HashSet<Gate>();
    private Vector2 moveDirection = Vector2.up;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(UnitSpawner unitSpawner, Vector2 direction, Gate initialGate = null)
    {
        spawner = unitSpawner;
        moveDirection = direction.normalized;
        rb.linearVelocity = moveDirection * speed;
        passedGates.Clear(); // プールから再利用時にリセット

        // ゲートから生成された場合、そのゲートを通過済みとしてマーク
        if (initialGate != null)
        {
            passedGates.Add(initialGate);
        }
    }

    // 後方互換性のためのオーバーロード
    public void Initialize(UnitSpawner unitSpawner, Gate initialGate = null)
    {
        Initialize(unitSpawner, Vector2.up, initialGate);
    }

    public Vector2 GetMoveDirection()
    {
        return moveDirection;
    }

    void Update()
    {
        // 画面外かチェック（上下左右）
        Vector3 pos = transform.position;
        if (pos.y > despawnY || pos.y < -despawnY || Mathf.Abs(pos.x) > despawnX)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        rb.linearVelocity = Vector2.zero;
        passedGates.Clear();

        if (spawner != null)
        {
            spawner.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public bool HasPassedGate(Gate gate)
    {
        return passedGates.Contains(gate);
    }

    public void MarkGatePassed(Gate gate)
    {
        passedGates.Add(gate);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 敵との衝突をチェック
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 1対1相殺：両方を消滅させる
                enemy.ReturnToPool();
                ReturnToPool();
            }
        }
    }
}
