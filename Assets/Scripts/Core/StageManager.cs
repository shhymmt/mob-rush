using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Manages stage progression and applies stage settings.
/// ステージ進行を管理し、ステージ設定を適用。
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Stage Settings")]
    [SerializeField] private int currentStageIndex = 0;
    [SerializeField] private bool autoProgressStages = true;

    [Header("Events")]
    public UnityEvent<int> OnStageChanged;      // ステージ番号
    public UnityEvent<string> OnStageNameChanged; // ステージ名

    [Header("References")]
    [SerializeField] private ScrollManager scrollManager;
    [SerializeField] private LaneEnemySpawner enemySpawner;
    [SerializeField] private LaneGateSpawner gateSpawner;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SpawnCoordinator spawnCoordinator;

    private List<StageData> stages = new List<StageData>();
    private StageData currentStage;

    public int CurrentStageIndex => currentStageIndex;
    public StageData CurrentStage => currentStage;
    public int TotalStages => stages.Count;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // デフォルトステージを作成
        CreateDefaultStages();
    }

    void Start()
    {
        // 参照を取得
        if (scrollManager == null)
            scrollManager = ScrollManager.Instance;
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<LaneEnemySpawner>();
        if (gateSpawner == null)
            gateSpawner = FindFirstObjectByType<LaneGateSpawner>();
        if (gameManager == null)
            gameManager = GameManager.Instance;
        if (spawnCoordinator == null)
            spawnCoordinator = SpawnCoordinator.Instance;

        // GameManagerのイベントを購読
        if (gameManager != null)
        {
            gameManager.OnGameWin.AddListener(OnGameWin);
        }

        // 最初のステージを適用
        ApplyStage(currentStageIndex);
    }

    private void CreateDefaultStages()
    {
        stages.Clear();
        stages.Add(StageData.CreateTutorial());
        stages.Add(StageData.CreateStage2());
        stages.Add(StageData.CreateStage3());
    }

    /// <summary>
    /// ステージを適用
    /// </summary>
    public void ApplyStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= stages.Count)
        {
            Debug.LogWarning($"Invalid stage index: {stageIndex}");
            return;
        }

        currentStageIndex = stageIndex;
        currentStage = stages[stageIndex];

        Debug.Log($"Applying stage: {currentStage.stageName}");

        // ScrollManagerに速度を適用
        if (scrollManager != null)
        {
            scrollManager.SetScrollSpeed(currentStage.scrollSpeed);
        }

        // GameManagerにステージ時間を適用
        if (gameManager != null)
        {
            gameManager.SetStageDuration(currentStage.duration);
        }

        // EnemySpawnerに設定を適用
        ApplyEnemySettings();

        // GateSpawnerに設定を適用
        ApplyGateSettings();

        // SpawnCoordinatorをクリア
        if (spawnCoordinator != null)
        {
            spawnCoordinator.ClearAllRecords();
        }

        // イベントを発火
        OnStageChanged?.Invoke(currentStageIndex + 1); // 1-indexed for display
        OnStageNameChanged?.Invoke(currentStage.stageName);
    }

    private void ApplyEnemySettings()
    {
        if (enemySpawner == null || currentStage == null) return;

        enemySpawner.SetSpawnInterval(currentStage.enemySpawnInterval);
        enemySpawner.SetHPRange(currentStage.enemyMinHP, currentStage.enemyMaxHP);
    }

    private void ApplyGateSettings()
    {
        if (gateSpawner == null || currentStage == null) return;

        gateSpawner.SetSpawnInterval(currentStage.gateSpawnInterval);
        gateSpawner.SetTypeWeights(
            currentStage.addWeight,
            currentStage.subtractWeight,
            currentStage.multiplyWeight
        );
        gateSpawner.SetValueRanges(
            currentStage.minAddValue, currentStage.maxAddValue,
            currentStage.minSubtractValue, currentStage.maxSubtractValue,
            currentStage.minMultiplyValue, currentStage.maxMultiplyValue
        );
    }

    private void OnGameWin()
    {
        if (!autoProgressStages) return;

        // 次のステージがあれば進む
        if (currentStageIndex + 1 < stages.Count)
        {
            Debug.Log($"Stage {currentStageIndex + 1} complete! Next stage available.");
        }
        else
        {
            Debug.Log("All stages complete!");
        }
    }

    /// <summary>
    /// 次のステージに進む
    /// </summary>
    public void NextStage()
    {
        if (currentStageIndex + 1 < stages.Count)
        {
            ApplyStage(currentStageIndex + 1);
        }
    }

    /// <summary>
    /// ステージをリセット（最初のステージに戻る）
    /// </summary>
    public void ResetToFirstStage()
    {
        ApplyStage(0);
    }

    /// <summary>
    /// カスタムステージを追加
    /// </summary>
    public void AddStage(StageData stage)
    {
        stages.Add(stage);
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameWin.RemoveListener(OnGameWin);
        }
    }
}
