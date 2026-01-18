using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for the lane runner game mode.
/// レーンランナーモード用のUI。
/// </summary>
public class LaneGameUI : MonoBehaviour
{
    [Header("Unit Count Display")]
    [SerializeField] private TextMeshProUGUI unitCountText;

    [Header("Stage Display")]
    [SerializeField] private TextMeshProUGUI stageText;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverTitleText;
    [SerializeField] private TextMeshProUGUI gameOverMessageText;
    [SerializeField] private Button restartButton;

    [Header("References")]
    [SerializeField] private PlayerUnitGroup playerUnitGroup;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private StageManager stageManager;

    [Header("Auto Create UI")]
    [SerializeField] private bool autoCreateUI = true;

    private Canvas canvas;

    void Start()
    {
        // 参照を取得
        if (playerUnitGroup == null)
        {
            playerUnitGroup = FindFirstObjectByType<PlayerUnitGroup>();
        }

        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        if (stageManager == null)
        {
            stageManager = StageManager.Instance;
        }

        // UIを自動作成
        if (autoCreateUI)
        {
            CreateUI();
        }

        // イベントを購読
        if (playerUnitGroup != null)
        {
            playerUnitGroup.OnUnitCountChanged.AddListener(OnUnitCountChanged);
            UpdateUnitCountDisplay(playerUnitGroup.UnitCount);
        }

        if (gameManager != null)
        {
            gameManager.OnGameWin.AddListener(OnGameWin);
            gameManager.OnGameLose.AddListener(OnGameLose);
        }

        if (stageManager != null)
        {
            stageManager.OnStageNameChanged.AddListener(OnStageNameChanged);
            // 初期ステージ名を表示
            if (stageManager.CurrentStage != null)
            {
                UpdateStageDisplay(stageManager.CurrentStage.stageName);
            }
        }

        // ゲームオーバーパネルを非表示
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void CreateUI()
    {
        // Canvasを作成
        GameObject canvasObj = new GameObject("GameCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystemを作成（ボタンクリックに必要）
        CreateEventSystem();

        // Unit Count Display
        CreateUnitCountDisplay(canvasObj);

        // Stage Display
        CreateStageDisplay(canvasObj);

        // Game Over Panel
        CreateGameOverPanel(canvasObj);
    }

    private void CreateEventSystem()
    {
        // EventSystemが存在しない場合のみ作成
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    private void CreateUnitCountDisplay(GameObject canvasObj)
    {
        GameObject textObj = new GameObject("UnitCountText");
        textObj.transform.SetParent(canvasObj.transform, false);

        unitCountText = textObj.AddComponent<TextMeshProUGUI>();
        unitCountText.text = "Units: 3";
        unitCountText.fontSize = 36;
        unitCountText.color = Color.white;
        unitCountText.alignment = TextAlignmentOptions.TopLeft;

        // 位置を左上に
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -20);
        rt.sizeDelta = new Vector2(200, 50);

        // 背景を追加
        GameObject bgObj = new GameObject("UnitCountBG");
        bgObj.transform.SetParent(textObj.transform, false);
        bgObj.transform.SetAsFirstSibling();

        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);

        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = new Vector2(-10, -5);
        bgRt.offsetMax = new Vector2(10, 5);
    }

    private void CreateStageDisplay(GameObject canvasObj)
    {
        GameObject textObj = new GameObject("StageText");
        textObj.transform.SetParent(canvasObj.transform, false);

        stageText = textObj.AddComponent<TextMeshProUGUI>();
        stageText.text = "Stage 1";
        stageText.fontSize = 36;
        stageText.color = Color.white;
        stageText.alignment = TextAlignmentOptions.TopRight;

        // 位置を右上に
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -20);
        rt.sizeDelta = new Vector2(200, 50);

        // 背景を追加
        GameObject bgObj = new GameObject("StageBG");
        bgObj.transform.SetParent(textObj.transform, false);
        bgObj.transform.SetAsFirstSibling();

        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);

        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = new Vector2(-10, -5);
        bgRt.offsetMax = new Vector2(10, 5);
    }

    private void CreateGameOverPanel(GameObject canvasObj)
    {
        // パネル
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);

        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);

        RectTransform panelRt = gameOverPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        // タイトルテキスト
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(gameOverPanel.transform, false);

        gameOverTitleText = titleObj.AddComponent<TextMeshProUGUI>();
        gameOverTitleText.text = "GAME OVER";
        gameOverTitleText.fontSize = 72;
        gameOverTitleText.color = Color.red;
        gameOverTitleText.alignment = TextAlignmentOptions.Center;

        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.65f);
        titleRt.anchorMax = new Vector2(0.5f, 0.65f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = Vector2.zero;
        titleRt.sizeDelta = new Vector2(600, 100);

        // メッセージテキスト
        GameObject msgObj = new GameObject("MessageText");
        msgObj.transform.SetParent(gameOverPanel.transform, false);

        gameOverMessageText = msgObj.AddComponent<TextMeshProUGUI>();
        gameOverMessageText.text = "";
        gameOverMessageText.fontSize = 36;
        gameOverMessageText.color = Color.white;
        gameOverMessageText.alignment = TextAlignmentOptions.Center;

        RectTransform msgRt = msgObj.GetComponent<RectTransform>();
        msgRt.anchorMin = new Vector2(0.5f, 0.5f);
        msgRt.anchorMax = new Vector2(0.5f, 0.5f);
        msgRt.pivot = new Vector2(0.5f, 0.5f);
        msgRt.anchoredPosition = Vector2.zero;
        msgRt.sizeDelta = new Vector2(400, 50);

        // リスタートボタン
        GameObject buttonObj = new GameObject("RestartButton");
        buttonObj.transform.SetParent(gameOverPanel.transform, false);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f);

        restartButton = buttonObj.AddComponent<Button>();
        restartButton.onClick.AddListener(OnRestartClicked);

        RectTransform buttonRt = buttonObj.GetComponent<RectTransform>();
        buttonRt.anchorMin = new Vector2(0.5f, 0.3f);
        buttonRt.anchorMax = new Vector2(0.5f, 0.3f);
        buttonRt.pivot = new Vector2(0.5f, 0.5f);
        buttonRt.anchoredPosition = Vector2.zero;
        buttonRt.sizeDelta = new Vector2(200, 60);

        // ボタンテキスト
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);

        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "RESTART";
        buttonText.fontSize = 32;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;

        RectTransform buttonTextRt = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRt.anchorMin = Vector2.zero;
        buttonTextRt.anchorMax = Vector2.one;
        buttonTextRt.offsetMin = Vector2.zero;
        buttonTextRt.offsetMax = Vector2.zero;

        gameOverPanel.SetActive(false);
    }

    private void OnUnitCountChanged(int count)
    {
        UpdateUnitCountDisplay(count);
    }

    private void UpdateUnitCountDisplay(int count)
    {
        if (unitCountText != null)
        {
            unitCountText.text = $"Units: {count}";
        }
    }

    private void OnStageNameChanged(string stageName)
    {
        UpdateStageDisplay(stageName);
    }

    private void UpdateStageDisplay(string stageName)
    {
        if (stageText != null)
        {
            stageText.text = stageName;
        }
    }

    private void OnGameWin()
    {
        ShowGameOverPanel("VICTORY!", "Stage Complete!", Color.green);
    }

    private void OnGameLose()
    {
        ShowGameOverPanel("GAME OVER", "Try Again!", Color.red);
    }

    private void ShowGameOverPanel(string title, string message, Color titleColor)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverTitleText != null)
            {
                gameOverTitleText.text = title;
                gameOverTitleText.color = titleColor;
            }

            if (gameOverMessageText != null)
            {
                gameOverMessageText.text = message;
            }
        }
    }

    private void OnRestartClicked()
    {
        // ボタンクリック音を再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(SoundManager.SoundType.ButtonClick);
        }

        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    void OnDestroy()
    {
        if (playerUnitGroup != null)
        {
            playerUnitGroup.OnUnitCountChanged.RemoveListener(OnUnitCountChanged);
        }

        if (gameManager != null)
        {
            gameManager.OnGameWin.RemoveListener(OnGameWin);
            gameManager.OnGameLose.RemoveListener(OnGameLose);
        }

        if (stageManager != null)
        {
            stageManager.OnStageNameChanged.RemoveListener(OnStageNameChanged);
        }
    }
}
