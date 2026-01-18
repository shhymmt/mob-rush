using UnityEngine;
using TMPro;

/// <summary>
/// Gate that scrolls down and affects player unit count.
/// 下にスクロールしてプレイヤーのユニット数に影響を与えるゲート。
/// </summary>
[RequireComponent(typeof(Scrollable))]
public class LaneGate : MonoBehaviour
{
    public enum GateType
    {
        Add,      // +N
        Subtract, // -N
        Multiply  // xN
    }

    [Header("Settings")]
    [SerializeField] private GateType gateType = GateType.Add;
    [SerializeField] private int value = 5;

    [Header("Visual")]
    [SerializeField] private Color addColor = new Color(0.2f, 0.8f, 0.2f);      // 緑
    [SerializeField] private Color subtractColor = new Color(0.8f, 0.2f, 0.2f); // 赤
    [SerializeField] private Color multiplyColor = new Color(0.8f, 0.6f, 0.2f); // オレンジ

    private Scrollable scrollable;
    private LaneGateSpawner spawner;
    private SpriteRenderer spriteRenderer;
    private TextMeshPro labelText;
    private int currentLane;
    private bool hasBeenTriggered = false;

    public int CurrentLane => currentLane;
    public GateType Type => gateType;
    public int Value => value;

    void Awake()
    {
        scrollable = GetComponent<Scrollable>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Scrollableイベントを購読
        if (scrollable != null)
        {
            scrollable.OnDespawned.AddListener(OnScrolledOffScreen);
        }
    }

    /// <summary>
    /// ゲートを初期化
    /// </summary>
    public void Initialize(LaneGateSpawner gateSpawner, int lane, GateType type, int gateValue)
    {
        spawner = gateSpawner;
        currentLane = lane;
        gateType = type;
        value = gateValue;
        hasBeenTriggered = false;

        // 先にGameObjectを有効化
        gameObject.SetActive(true);

        // Scrollableを有効化
        if (scrollable != null)
        {
            scrollable.Activate();
        }

        // ビジュアルを更新
        UpdateVisual();

        Debug.Log($"Gate initialized: lane={lane}, type={type}, value={gateValue}");
    }

    private void UpdateVisual()
    {
        // 色を設定
        if (spriteRenderer != null)
        {
            switch (gateType)
            {
                case GateType.Add:
                    spriteRenderer.color = addColor;
                    break;
                case GateType.Subtract:
                    spriteRenderer.color = subtractColor;
                    break;
                case GateType.Multiply:
                    spriteRenderer.color = multiplyColor;
                    break;
            }
        }

        // ラベルを更新
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        // 子オブジェクトからTextMeshProを探す
        if (labelText == null)
        {
            labelText = GetComponentInChildren<TextMeshPro>();
        }

        if (labelText != null)
        {
            string text = "";
            switch (gateType)
            {
                case GateType.Add:
                    text = $"+{value}";
                    break;
                case GateType.Subtract:
                    text = $"-{value}";
                    break;
                case GateType.Multiply:
                    text = $"x{value}";
                    break;
            }
            labelText.text = text;
        }
    }

    /// <summary>
    /// プールに返す
    /// </summary>
    public void ReturnToPool()
    {
        if (scrollable != null)
        {
            scrollable.Deactivate();
        }

        if (spawner != null)
        {
            spawner.ReturnGate(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnScrolledOffScreen()
    {
        Debug.Log($"Gate scrolled off screen. Lane: {currentLane}");
        ReturnToPool();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 既にトリガーされていたら無視
        if (hasBeenTriggered) return;

        // プレイヤーユニットグループとの衝突
        var playerGroup = other.GetComponent<PlayerUnitGroup>();
        if (playerGroup != null)
        {
            hasBeenTriggered = true;
            ApplyEffect(playerGroup);
            ReturnToPool();
        }
    }

    private void ApplyEffect(PlayerUnitGroup playerGroup)
    {
        int currentUnits = playerGroup.UnitCount;
        int newUnits = currentUnits;
        string popupText = "";
        Color popupColor = Color.white;

        switch (gateType)
        {
            case GateType.Add:
                newUnits = currentUnits + value;
                popupText = $"+{value}";
                popupColor = new Color(0.2f, 1f, 0.2f); // Green
                Debug.Log($"Gate +{value}: {currentUnits} -> {newUnits}");
                break;

            case GateType.Subtract:
                newUnits = currentUnits - value;
                popupText = $"-{value}";
                popupColor = new Color(1f, 0.2f, 0.2f); // Red
                Debug.Log($"Gate -{value}: {currentUnits} -> {newUnits}");
                break;

            case GateType.Multiply:
                newUnits = currentUnits * value;
                popupText = $"x{value}";
                popupColor = new Color(1f, 0.8f, 0.2f); // Orange/Gold
                Debug.Log($"Gate x{value}: {currentUnits} -> {newUnits}");
                break;
        }

        // エフェクトを再生
        if (EffectManager.Instance != null)
        {
            // フラッシュエフェクト
            EffectManager.Instance.PlayGateFlash(transform.position, gateType);

            // 数字ポップアップ
            Vector3 popupPosition = playerGroup.transform.position + Vector3.up * 0.5f;
            EffectManager.Instance.ShowNumberPopup(popupPosition, popupText, popupColor);
        }

        // ゲートサウンドを再生
        if (SoundManager.Instance != null)
        {
            switch (gateType)
            {
                case GateType.Add:
                    SoundManager.Instance.PlaySE(SoundManager.SoundType.GateAdd);
                    break;
                case GateType.Subtract:
                    SoundManager.Instance.PlaySE(SoundManager.SoundType.GateSubtract);
                    break;
                case GateType.Multiply:
                    SoundManager.Instance.PlaySE(SoundManager.SoundType.GateMultiply);
                    break;
            }
        }

        // ユニット数を更新（AddUnitsは差分を渡す）
        int diff = newUnits - currentUnits;
        playerGroup.AddUnits(diff);
    }

    void OnDestroy()
    {
        if (scrollable != null)
        {
            scrollable.OnDespawned.RemoveListener(OnScrolledOffScreen);
        }
    }
}
