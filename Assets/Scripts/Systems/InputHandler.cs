using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cannonTransform;
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private float defaultAimAngle = 90f; // デフォルトは真上（90度）

    public bool IsSpawning { get; private set; }
    public Vector2 AimDirection { get; private set; } = Vector2.up;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        AimDirection = Vector2.up;
    }

    void Update()
    {
        IsSpawning = false;
        Vector2 inputScreenPosition = Vector2.zero;
        bool hasInput = false;

        // マウス入力
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            IsSpawning = true;
            inputScreenPosition = Mouse.current.position.ReadValue();
            hasInput = true;
        }

        // タッチ入力（マウスよりも優先）
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            IsSpawning = true;
            inputScreenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            hasInput = true;
        }

        // 入力位置から方向を計算
        if (hasInput && cannonTransform != null && mainCamera != null)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(inputScreenPosition.x, inputScreenPosition.y, 0));
            Vector2 cannonPos = cannonTransform.position;
            Vector2 targetPos = new Vector2(worldPos.x, worldPos.y);

            Vector2 direction = (targetPos - cannonPos).normalized;

            // 下向き（負のY）を防ぐ - 最低でも水平方向に
            if (direction.y < 0.1f)
            {
                direction.y = 0.1f;
                direction = direction.normalized;
            }

            AimDirection = direction;
        }
    }

    // キャノン参照を設定（UnitSpawnerから呼ばれる）
    public void SetCannonTransform(Transform cannon)
    {
        cannonTransform = cannon;
    }
}
