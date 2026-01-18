using UnityEngine;
using TMPro;

public enum GateType { Multiply, Add }

public class Gate : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GateType gateType = GateType.Multiply;
    [SerializeField] private int value = 2;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private TextMeshPro label;

    [Header("Colors")]
    [SerializeField] private Color multiplyColor = Color.green;
    [SerializeField] private Color addColor = Color.yellow;

    private UnitSpawner spawner;
    private Vector3 originalScale;
    private Coroutine pulseCoroutine;

    void Start()
    {
        originalScale = transform.localScale; // 起動時に元のスケールを保存
        spawner = FindFirstObjectByType<UnitSpawner>();
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (visual == null) return;

        // タイプと値に基づいて色を設定
        if (gateType == GateType.Multiply)
        {
            visual.color = value switch
            {
                2 => new Color(0, 1, 0),      // 緑
                3 => new Color(0, 0.5f, 1),   // 青
                5 => new Color(0.6f, 0, 1),   // 紫
                _ => multiplyColor
            };
            if (label != null) label.text = $"×{value}";
        }
        else
        {
            visual.color = addColor;
            if (label != null) label.text = $"+{value}";
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Unit")) return;

        var unit = other.GetComponent<Unit>();
        if (unit == null) return;

        // ユニットがすでにこのゲートを通過したかチェック
        if (unit.HasPassedGate(this)) return;

        // 通過済みとしてマーク
        unit.MarkGatePassed(this);

        // ゲート効果を処理
        ProcessGateEffect(unit);
    }

    private void ProcessGateEffect(Unit unit)
    {
        int unitsToSpawn = gateType == GateType.Multiply
            ? value - 1  // 元のユニットは継続、追加分を生成
            : value;     // Addは指定数だけ生成

        Vector3 basePos = unit.transform.position;
        Vector2 direction = unit.GetMoveDirection(); // 元のユニットの方向を取得

        for (int i = 0; i < unitsToSpawn; i++)
        {
            SpawnExtraUnit(basePos, direction);
        }

        // 視覚的フィードバック
        PulseGate();
    }

    private void SpawnExtraUnit(Vector3 basePosition, Vector2 direction)
    {
        if (spawner == null) return;

        Vector3 offset = new Vector3(
            Random.Range(-0.3f, 0.3f),
            0.2f,
            0
        );

        // このゲートを通過済み + 同じ方向で生成
        spawner.SpawnUnitAtPosition(basePosition + offset, this, direction);
    }

    private void PulseGate()
    {
        // 既存のコルーチンを停止してから新しいものを開始
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            transform.localScale = originalScale; // スケールをリセット
        }
        pulseCoroutine = StartCoroutine(PulseAnimation());
    }

    private System.Collections.IEnumerator PulseAnimation()
    {
        Vector3 targetScale = originalScale * 1.2f;

        float duration = 0.1f;
        float elapsed = 0f;

        // スケールアップ
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;

        // スケールダウン
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = originalScale;
        pulseCoroutine = null;
    }
}
