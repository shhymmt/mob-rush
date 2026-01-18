using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Coordinates spawning to prevent overlaps between enemies and gates.
/// 敵とゲートの重なりを防ぐためにスポーンを調整。
/// </summary>
public class SpawnCoordinator : MonoBehaviour
{
    public static SpawnCoordinator Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float minVerticalDistance = 2f; // 最小垂直距離
    [SerializeField] private float trackingDuration = 3f;    // スポーン位置を追跡する時間

    [Header("References")]
    [SerializeField] private LaneManager laneManager;

    // レーンごとの最近のスポーン位置を追跡
    private Dictionary<int, List<SpawnRecord>> recentSpawns = new Dictionary<int, List<SpawnRecord>>();

    private struct SpawnRecord
    {
        public float yPosition;
        public float timestamp;
    }

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
        if (laneManager == null)
        {
            laneManager = LaneManager.Instance;
        }

        // レーンごとのリストを初期化
        if (laneManager != null)
        {
            for (int i = 0; i < laneManager.LaneCount; i++)
            {
                recentSpawns[i] = new List<SpawnRecord>();
            }
        }
    }

    void Update()
    {
        // 古いスポーン記録をクリーンアップ
        CleanupOldRecords();
    }

    /// <summary>
    /// 指定したレーンと位置にスポーン可能かチェック
    /// </summary>
    public bool CanSpawnAt(int lane, float yPosition)
    {
        if (!recentSpawns.ContainsKey(lane))
        {
            return true;
        }

        foreach (var record in recentSpawns[lane])
        {
            float distance = Mathf.Abs(yPosition - record.yPosition);
            if (distance < minVerticalDistance)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// スポーン可能なレーンを取得（ランダム選択、重なり回避）
    /// </summary>
    public int GetAvailableLane(float yPosition)
    {
        if (laneManager == null) return 0;

        List<int> availableLanes = new List<int>();

        for (int i = 0; i < laneManager.LaneCount; i++)
        {
            if (CanSpawnAt(i, yPosition))
            {
                availableLanes.Add(i);
            }
        }

        if (availableLanes.Count == 0)
        {
            // 全レーンが埋まっている場合はスポーンをスキップ
            return -1;
        }

        return availableLanes[Random.Range(0, availableLanes.Count)];
    }

    /// <summary>
    /// スポーンを記録
    /// </summary>
    public void RecordSpawn(int lane, float yPosition)
    {
        if (!recentSpawns.ContainsKey(lane))
        {
            recentSpawns[lane] = new List<SpawnRecord>();
        }

        recentSpawns[lane].Add(new SpawnRecord
        {
            yPosition = yPosition,
            timestamp = Time.time
        });
    }

    /// <summary>
    /// 古いスポーン記録を削除
    /// </summary>
    private void CleanupOldRecords()
    {
        float cutoffTime = Time.time - trackingDuration;

        foreach (var lane in recentSpawns.Keys)
        {
            recentSpawns[lane].RemoveAll(record => record.timestamp < cutoffTime);
        }
    }

    /// <summary>
    /// 全てのスポーン記録をクリア（ステージ開始時など）
    /// </summary>
    public void ClearAllRecords()
    {
        foreach (var lane in recentSpawns.Keys)
        {
            recentSpawns[lane].Clear();
        }
    }
}
