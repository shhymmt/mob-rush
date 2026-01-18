using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays boss HP bar and related UI elements.
/// ボスのHPバーと関連UI要素を表示。
/// </summary>
public class BossUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bossUIPanel;
    [SerializeField] private Image hpBarFill;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI bossNameText;

    [Header("Settings")]
    [SerializeField] private string bossName = "BOSS";
    [SerializeField] private Color hpBarColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color hpBarLowColor = new Color(1f, 0.5f, 0f); // HP低下時の色

    [Header("Auto Create UI")]
    [SerializeField] private bool autoCreateUI = true;

    private LaneBoss currentBoss;
    private Health bossHealth;
    private Canvas canvas;

    void Start()
    {
        // BossSpawnerのイベントを購読
        if (BossSpawner.Instance != null)
        {
            BossSpawner.Instance.OnBossSpawned.AddListener(OnBossSpawned);
            BossSpawner.Instance.OnBossDefeated.AddListener(OnBossDefeated);
        }

        // UIを自動作成
        if (autoCreateUI)
        {
            CreateUI();
        }

        // 初期状態は非表示
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(false);
        }
    }

    private void CreateUI()
    {
        // 既存のCanvasを探す
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            // Canvasがない場合は作成
            GameObject canvasObj = new GameObject("BossUICanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 最前面に表示
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Boss UI Panel
        bossUIPanel = new GameObject("BossUIPanel");
        bossUIPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRt = bossUIPanel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 1f);
        panelRt.anchorMax = new Vector2(0.5f, 1f);
        panelRt.pivot = new Vector2(0.5f, 1f);
        panelRt.anchoredPosition = new Vector2(0, -80);
        panelRt.sizeDelta = new Vector2(300, 60);

        // 背景
        Image panelBg = bossUIPanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.7f);

        // Boss Name Text
        GameObject nameObj = new GameObject("BossNameText");
        nameObj.transform.SetParent(bossUIPanel.transform, false);

        bossNameText = nameObj.AddComponent<TextMeshProUGUI>();
        bossNameText.text = bossName;
        bossNameText.fontSize = 24;
        bossNameText.color = Color.red;
        bossNameText.alignment = TextAlignmentOptions.Center;
        bossNameText.fontStyle = FontStyles.Bold;

        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0, 0.6f);
        nameRt.anchorMax = new Vector2(1, 1f);
        nameRt.offsetMin = new Vector2(10, 0);
        nameRt.offsetMax = new Vector2(-10, -5);

        // HP Bar Background
        GameObject hpBarBg = new GameObject("HPBarBG");
        hpBarBg.transform.SetParent(bossUIPanel.transform, false);

        Image hpBarBgImage = hpBarBg.AddComponent<Image>();
        hpBarBgImage.color = new Color(0.2f, 0.2f, 0.2f);

        RectTransform hpBarBgRt = hpBarBg.GetComponent<RectTransform>();
        hpBarBgRt.anchorMin = new Vector2(0, 0);
        hpBarBgRt.anchorMax = new Vector2(1, 0.5f);
        hpBarBgRt.offsetMin = new Vector2(10, 5);
        hpBarBgRt.offsetMax = new Vector2(-10, -5);

        // HP Bar Fill
        GameObject hpBarFillObj = new GameObject("HPBarFill");
        hpBarFillObj.transform.SetParent(hpBarBg.transform, false);

        hpBarFill = hpBarFillObj.AddComponent<Image>();
        hpBarFill.color = hpBarColor;

        RectTransform hpBarFillRt = hpBarFillObj.GetComponent<RectTransform>();
        hpBarFillRt.anchorMin = Vector2.zero;
        hpBarFillRt.anchorMax = Vector2.one;
        hpBarFillRt.offsetMin = new Vector2(2, 2);
        hpBarFillRt.offsetMax = new Vector2(-2, -2);
        hpBarFillRt.pivot = new Vector2(0, 0.5f);

        // HP Text
        GameObject hpTextObj = new GameObject("HPText");
        hpTextObj.transform.SetParent(hpBarBg.transform, false);

        hpText = hpTextObj.AddComponent<TextMeshProUGUI>();
        hpText.text = "100 / 100";
        hpText.fontSize = 16;
        hpText.color = Color.white;
        hpText.alignment = TextAlignmentOptions.Center;

        RectTransform hpTextRt = hpTextObj.GetComponent<RectTransform>();
        hpTextRt.anchorMin = Vector2.zero;
        hpTextRt.anchorMax = Vector2.one;
        hpTextRt.offsetMin = Vector2.zero;
        hpTextRt.offsetMax = Vector2.zero;

        bossUIPanel.SetActive(false);
    }

    private void OnBossSpawned()
    {
        if (BossSpawner.Instance == null) return;

        currentBoss = BossSpawner.Instance.CurrentBoss;
        if (currentBoss != null)
        {
            bossHealth = currentBoss.Health;
            if (bossHealth != null)
            {
                bossHealth.OnHealthChanged.AddListener(OnBossHealthChanged);
            }

            // UIを表示
            if (bossUIPanel != null)
            {
                bossUIPanel.SetActive(true);
            }

            // 初期HP表示
            if (bossHealth != null)
            {
                UpdateHPDisplay(bossHealth.CurrentHealth, bossHealth.MaxHealth);
            }

            Debug.Log("Boss UI shown");
        }
    }

    private void OnBossDefeated()
    {
        // Healthイベントの購読解除
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged.RemoveListener(OnBossHealthChanged);
        }

        currentBoss = null;
        bossHealth = null;

        // UIを非表示
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(false);
        }

        Debug.Log("Boss UI hidden");
    }

    private void OnBossHealthChanged(int current, int max)
    {
        UpdateHPDisplay(current, max);
    }

    private void UpdateHPDisplay(int current, int max)
    {
        if (hpBarFill != null)
        {
            float percent = max > 0 ? (float)current / max : 0;
            hpBarFill.rectTransform.anchorMax = new Vector2(percent, 1);

            // HP低下時に色を変える
            if (percent <= 0.3f)
            {
                hpBarFill.color = hpBarLowColor;
            }
            else
            {
                hpBarFill.color = hpBarColor;
            }
        }

        if (hpText != null)
        {
            hpText.text = $"{current} / {max}";
        }
    }

    void OnDestroy()
    {
        if (BossSpawner.Instance != null)
        {
            BossSpawner.Instance.OnBossSpawned.RemoveListener(OnBossSpawned);
            BossSpawner.Instance.OnBossDefeated.RemoveListener(OnBossDefeated);
        }

        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged.RemoveListener(OnBossHealthChanged);
        }
    }
}
