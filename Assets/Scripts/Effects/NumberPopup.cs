using UnityEngine;
using TMPro;

/// <summary>
/// Floating number popup that rises and fades.
/// 上に浮いてフェードする数字ポップアップ。
/// </summary>
public class NumberPopup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float riseDuration = 0.8f;
    [SerializeField] private float riseDistance = 1.5f;
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float peakScale = 1.2f;

    private TextMeshPro textMesh;
    private float timer;
    private Vector3 startPosition;
    private Color startColor;
    private bool isPlaying;

    void Awake()
    {
        CreateTextMesh();
    }

    private void CreateTextMesh()
    {
        if (textMesh != null) return;

        textMesh = gameObject.AddComponent<TextMeshPro>();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 5;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.sortingOrder = 15;

        // Set rect transform size
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(3f, 1f);
        }
    }

    /// <summary>
    /// ポップアップを表示
    /// </summary>
    public void Show(Vector3 position, string text, Color color)
    {
        if (textMesh == null)
        {
            CreateTextMesh();
        }

        gameObject.SetActive(true);
        transform.position = position;
        startPosition = position;
        startColor = color;

        textMesh.text = text;
        textMesh.color = color;

        timer = 0f;
        isPlaying = true;

        // Start at small scale
        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;
        float progress = timer / riseDuration;

        if (progress >= 1f)
        {
            // Animation complete
            isPlaying = false;
            gameObject.SetActive(false);
            return;
        }

        // Rise up
        float yOffset = Mathf.Lerp(0f, riseDistance, EaseOutQuad(progress));
        transform.position = startPosition + Vector3.up * yOffset;

        // Scale: small -> big -> medium
        float scale;
        if (progress < 0.3f)
        {
            // Scale up quickly
            scale = Mathf.Lerp(startScale, peakScale, progress / 0.3f);
        }
        else
        {
            // Scale down to normal
            scale = Mathf.Lerp(peakScale, 1f, (progress - 0.3f) / 0.7f);
        }
        transform.localScale = Vector3.one * scale;

        // Fade out in second half
        if (progress > 0.5f)
        {
            float alpha = Mathf.Lerp(1f, 0f, (progress - 0.5f) / 0.5f);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
    }

    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}
