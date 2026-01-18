using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int startingSpawnCount = 30;

    [Header("Lane Runner Settings")]
    [SerializeField] private bool useLaneRunnerMode = true;
    [SerializeField] private float stageDuration = 30f; // ステージの長さ（秒）

    [Header("Boss Battle Settings")]
    [SerializeField] private bool enableBossBattle = true;
    [SerializeField] private float bossSpawnTimeBeforeEnd = 10f; // ステージ終了N秒前にボス出現

    [Header("Events")]
    public UnityEvent OnGameWin;
    public UnityEvent OnGameLose;
    public UnityEvent<int> OnSpawnCountChanged;
    public UnityEvent OnBossPhaseStarted;

    private GameState currentState = GameState.Playing;
    private int remainingSpawns;
    private EnemyBase[] enemyBases;
    private int destroyedBases = 0;
    private float stageTimer = 0f;
    private bool bossSpawned = false;
    private bool inBossPhase = false;

    public GameState CurrentState => currentState;
    public int RemainingSpawns => remainingSpawns;
    public float StageTimer => stageTimer;
    public float StageDuration => stageDuration;
    public bool InBossPhase => inBossPhase;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        // レーンランナーモードのステージタイマー
        if (useLaneRunnerMode && currentState == GameState.Playing)
        {
            stageTimer += Time.deltaTime;

            // ボス出現チェック
            if (enableBossBattle && !bossSpawned)
            {
                float bossSpawnTime = stageDuration - bossSpawnTimeBeforeEnd;
                if (stageTimer >= bossSpawnTime)
                {
                    SpawnBoss();
                }
            }

            // ボス戦中は時間切れで勝利しない（ボスを倒して勝利）
            if (!inBossPhase && stageTimer >= stageDuration)
            {
                TriggerWin();
            }
        }
    }

    private void SpawnBoss()
    {
        bossSpawned = true;
        inBossPhase = true;

        Debug.Log("Boss phase started!");

        // ボスをスポーン
        if (BossSpawner.Instance != null)
        {
            int stageNumber = StageManager.Instance != null
                ? StageManager.Instance.CurrentStageIndex + 1
                : 1;
            BossSpawner.Instance.SpawnBoss(stageNumber);
        }

        OnBossPhaseStarted?.Invoke();
    }

    public void InitializeGame()
    {
        currentState = GameState.Playing;
        remainingSpawns = startingSpawnCount;
        destroyedBases = 0;
        stageTimer = 0f;
        bossSpawned = false;
        inBossPhase = false;

        // Find all enemy bases (Unity 6 API) - 従来モード用
        if (!useLaneRunnerMode)
        {
            enemyBases = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        }

        // ボスをクリア
        if (BossSpawner.Instance != null)
        {
            BossSpawner.Instance.DeactivateBoss();
        }

        // Notify UI
        OnSpawnCountChanged?.Invoke(remainingSpawns);

        // Resume time
        Time.timeScale = 1f;
    }

    // Called by UnitSpawner when a unit is spawned
    public bool TryConsumeSpawn()
    {
        if (currentState != GameState.Playing) return false;
        if (remainingSpawns <= 0) return false;

        remainingSpawns--;
        OnSpawnCountChanged?.Invoke(remainingSpawns);

        // Check if out of spawns
        if (remainingSpawns <= 0)
        {
            StartCoroutine(CheckForLoseCondition());
        }

        return true;
    }

    private System.Collections.IEnumerator CheckForLoseCondition()
    {
        yield return new WaitForSeconds(0.5f);

        while (currentState == GameState.Playing)
        {
            int activeUnits = CountActiveUnits();

            if (activeUnits == 0)
            {
                if (destroyedBases < enemyBases.Length)
                {
                    TriggerLose();
                }
                yield break;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private int CountActiveUnits()
    {
        var units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var unit in units)
        {
            if (unit.gameObject.activeInHierarchy)
                count++;
        }
        return count;
    }

    public void OnEnemyBaseDestroyed(EnemyBase destroyedBase)
    {
        destroyedBases++;

        if (destroyedBases >= enemyBases.Length)
        {
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Won;
        Debug.Log("WIN! Stage Complete!");

        // 勝利音を再生（BGMを一時停止）
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PauseBGM();
            SoundManager.Instance.PlaySE(SoundManager.SoundType.Victory);
        }

        OnGameWin?.Invoke();

        // ゲームを一時停止
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 外部から勝利をトリガー（ステージクリアなど）
    /// </summary>
    public void TriggerWinExternal()
    {
        TriggerWin();
    }

    public void TriggerLose()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Lost;
        Debug.Log("LOSE! Game Over!");

        // ゲームオーバー音を再生（BGMを一時停止）
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PauseBGM();
            SoundManager.Instance.PlaySE(SoundManager.SoundType.GameOver);
        }

        OnGameLose?.Invoke();

        // ゲームを一時停止
        Time.timeScale = 0f;
    }

    // 敵が画面下端に到達した時に呼ばれる
    public void OnEnemyReachedBottom()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Lost;
        Debug.Log("LOSE! Enemy reached the bottom!");

        OnGameLose?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// ステージ時間を設定
    /// </summary>
    public void SetStageDuration(float duration)
    {
        stageDuration = Mathf.Max(10f, duration);
    }
}
