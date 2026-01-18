using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// Handles lane-based input (tap/click to switch lanes).
/// レーンベースの入力を処理（タップ/クリックでレーン切り替え）。
/// </summary>
public class LaneInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LaneManager laneManager;

    [Header("Events")]
    public UnityEvent<int> OnLaneTapped; // レーンがタップされた時に発火（レーン番号を渡す）

    private bool wasPressed = false;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (laneManager == null)
        {
            laneManager = LaneManager.Instance;
        }
    }

    void Update()
    {
        bool isPressed = false;
        Vector2 inputPosition = Vector2.zero;

        // タッチ入力を優先
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            isPressed = true;
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        // マウス入力
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            isPressed = true;
            inputPosition = Mouse.current.position.ReadValue();
        }

        // タップ検出（押された瞬間のみ）
        if (isPressed && !wasPressed)
        {
            OnInputDown(inputPosition);
        }

        wasPressed = isPressed;
    }

    private void OnInputDown(Vector2 screenPosition)
    {
        if (laneManager == null)
        {
            laneManager = LaneManager.Instance;
            if (laneManager == null)
            {
                Debug.LogWarning("LaneManager not found!");
                return;
            }
        }

        // スクリーン座標からレーン番号を取得
        int tappedLane = laneManager.GetLaneFromScreenPosition(screenPosition, mainCamera);

        Debug.Log($"Lane tapped: {tappedLane}");

        // イベントを発火
        OnLaneTapped?.Invoke(tappedLane);
    }

    /// <summary>
    /// 外部からレーンマネージャーを設定
    /// </summary>
    public void SetLaneManager(LaneManager manager)
    {
        laneManager = manager;
    }
}
