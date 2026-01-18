using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private ParticleSystem destructionEffect;

    [Header("Settings")]
    [SerializeField] private int damagePerUnit = 1;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    private Color originalColor;
    private bool isDestroyed = false;
    private Coroutine flashCoroutine;

    void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (visual != null)
            originalColor = visual.color;
    }

    void Start()
    {
        // Subscribe to health events
        if (health != null)
        {
            health.OnHealthChanged.AddListener(OnHealthChanged);
            health.OnDeath.AddListener(OnBaseDestroyed);
        }
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged.RemoveListener(OnHealthChanged);
            health.OnDeath.RemoveListener(OnBaseDestroyed);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;
        if (!other.CompareTag("Unit")) return;

        // Deal damage to base
        health.TakeDamage(damagePerUnit);

        // Visual feedback
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            if (visual != null) visual.color = originalColor;
        }
        flashCoroutine = StartCoroutine(FlashHit());

        // Return unit to pool
        var unit = other.GetComponent<Unit>();
        if (unit != null)
        {
            unit.ReturnToPool();
        }
    }

    private System.Collections.IEnumerator FlashHit()
    {
        if (visual != null)
        {
            visual.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            visual.color = originalColor;
        }
        flashCoroutine = null;
    }

    private void OnHealthChanged(int current, int max)
    {
        // HPBar updates via its own listener
    }

    private void OnBaseDestroyed()
    {
        isDestroyed = true;

        // Play destruction effect
        if (destructionEffect != null)
        {
            destructionEffect.transform.SetParent(null); // Unparent so it survives
            destructionEffect.Play();
            Destroy(destructionEffect.gameObject, destructionEffect.main.duration);
        }

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyBaseDestroyed(this);
        }

        // Hide the base
        if (visual != null) visual.enabled = false;
        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
    }

    // For stage setup
    public void Initialize(int hp)
    {
        if (health != null)
        {
            health.SetMaxHealth(hp);
        }
        isDestroyed = false;
        if (visual != null) visual.enabled = true;
        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = true;
    }
}
