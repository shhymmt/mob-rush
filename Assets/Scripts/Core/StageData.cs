using UnityEngine;

/// <summary>
/// Stage configuration data.
/// ステージ設定データ。
/// </summary>
[System.Serializable]
public class StageData
{
    [Header("Basic Info")]
    public string stageName = "Stage 1";
    public float duration = 30f;

    [Header("Scroll Settings")]
    public float scrollSpeed = 3f;

    [Header("Enemy Settings")]
    public float enemySpawnInterval = 3f;
    public int enemyMinHP = 5;
    public int enemyMaxHP = 10;

    [Header("Gate Settings")]
    public float gateSpawnInterval = 5f;
    [Range(0, 100)] public int addWeight = 50;
    [Range(0, 100)] public int subtractWeight = 30;
    [Range(0, 100)] public int multiplyWeight = 20;

    [Header("Gate Values")]
    public int minAddValue = 3;
    public int maxAddValue = 10;
    public int minSubtractValue = 2;
    public int maxSubtractValue = 5;
    public int minMultiplyValue = 2;
    public int maxMultiplyValue = 3;

    /// <summary>
    /// デフォルトのチュートリアルステージを作成
    /// </summary>
    public static StageData CreateTutorial()
    {
        return new StageData
        {
            stageName = "Stage 1",
            duration = 30f,
            scrollSpeed = 2f,
            enemySpawnInterval = 4f,
            enemyMinHP = 3,
            enemyMaxHP = 5,
            gateSpawnInterval = 6f,
            addWeight = 70,
            subtractWeight = 20,
            multiplyWeight = 10,
            minAddValue = 3,
            maxAddValue = 5,
            minSubtractValue = 1,
            maxSubtractValue = 2,
            minMultiplyValue = 2,
            maxMultiplyValue = 2
        };
    }

    /// <summary>
    /// Stage 2を作成
    /// </summary>
    public static StageData CreateStage2()
    {
        return new StageData
        {
            stageName = "Stage 2",
            duration = 45f,
            scrollSpeed = 3f,
            enemySpawnInterval = 3f,
            enemyMinHP = 5,
            enemyMaxHP = 10,
            gateSpawnInterval = 5f,
            addWeight = 50,
            subtractWeight = 30,
            multiplyWeight = 20,
            minAddValue = 3,
            maxAddValue = 8,
            minSubtractValue = 2,
            maxSubtractValue = 4,
            minMultiplyValue = 2,
            maxMultiplyValue = 3
        };
    }

    /// <summary>
    /// Stage 3を作成
    /// </summary>
    public static StageData CreateStage3()
    {
        return new StageData
        {
            stageName = "Stage 3",
            duration = 60f,
            scrollSpeed = 4f,
            enemySpawnInterval = 2.5f,
            enemyMinHP = 8,
            enemyMaxHP = 15,
            gateSpawnInterval = 4f,
            addWeight = 40,
            subtractWeight = 40,
            multiplyWeight = 20,
            minAddValue = 5,
            maxAddValue = 10,
            minSubtractValue = 3,
            maxSubtractValue = 6,
            minMultiplyValue = 2,
            maxMultiplyValue = 3
        };
    }
}
