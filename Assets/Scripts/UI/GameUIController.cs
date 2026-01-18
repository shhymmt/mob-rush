using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI spawnCountText;

    [Header("Win Screen")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private Button winRestartButton;

    [Header("Lose Screen")]
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private Button loseRestartButton;

    void Start()
    {
        // Hide end screens
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);

        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSpawnCountChanged.AddListener(UpdateSpawnCount);
            GameManager.Instance.OnGameWin.AddListener(ShowWinScreen);
            GameManager.Instance.OnGameLose.AddListener(ShowLoseScreen);

            // Initialize spawn count display
            UpdateSpawnCount(GameManager.Instance.RemainingSpawns);
        }

        // Setup restart buttons
        if (winRestartButton != null)
            winRestartButton.onClick.AddListener(OnRestartClicked);
        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(OnRestartClicked);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSpawnCountChanged.RemoveListener(UpdateSpawnCount);
            GameManager.Instance.OnGameWin.RemoveListener(ShowWinScreen);
            GameManager.Instance.OnGameLose.RemoveListener(ShowLoseScreen);
        }
    }

    private void UpdateSpawnCount(int count)
    {
        if (spawnCountText != null)
        {
            spawnCountText.text = $"Units: {count}";
        }
    }

    private void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
    }

    private void ShowLoseScreen()
    {
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
        }
    }

    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartGame();
    }
}
