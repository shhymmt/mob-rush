using UnityEngine;

public class HPBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health targetHealth;
    [SerializeField] private Transform fillTransform;
    [SerializeField] private SpriteRenderer fillRenderer;

    [Header("Settings")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;

    private Vector3 originalScale;

    void Start()
    {
        if (fillTransform != null)
        {
            originalScale = fillTransform.localScale;
        }

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.AddListener(UpdateBar);
            UpdateBar(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }

    void OnDestroy()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.RemoveListener(UpdateBar);
        }
    }

    private void UpdateBar(int current, int max)
    {
        if (fillTransform == null) return;

        float percent = (float)current / max;

        // Scale the fill bar horizontally
        Vector3 newScale = originalScale;
        newScale.x = originalScale.x * percent;
        fillTransform.localScale = newScale;

        // Adjust position to keep left-aligned
        Vector3 pos = fillTransform.localPosition;
        pos.x = -originalScale.x * (1 - percent) / 2;
        fillTransform.localPosition = pos;

        // Change color based on health percentage
        if (fillRenderer != null)
        {
            fillRenderer.color = percent <= lowHealthThreshold ? lowHealthColor : fullHealthColor;
        }
    }

    // For external assignment
    public void SetTarget(Health health)
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.RemoveListener(UpdateBar);
        }

        targetHealth = health;

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.AddListener(UpdateBar);
            UpdateBar(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }
}
