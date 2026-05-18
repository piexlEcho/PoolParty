using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("当前分数")]
    public int totalScore = 0;

    [Header("事件")]
    public UnityEvent<int> OnScoreChanged;

    private bool isScoreFixed = false;  // 分数是否已固定
    private float _fishMultiplier = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 由老虎机在确认鱼类时调用
    public void SetFishMultiplier(float multiplier)
    {
        _fishMultiplier = multiplier;
        Debug.Log($"鱼类倍率已设置：{_fishMultiplier}x");
    }

    public void AddScore(int score, string zoneName, GameObject hitObject)
    {
        // 如果分数已固定，不再增减
        if (isScoreFixed) return;

        int finalScore = Mathf.RoundToInt(score * _fishMultiplier);
        totalScore += score;
        Debug.Log($"[+{score}] {hitObject.name} 进入 {zoneName}，总分：{totalScore}");
        OnScoreChanged?.Invoke(totalScore);
    }

    public void SubtractScore(int score, string zoneName, GameObject hitObject)
    {
        // 如果分数已固定，不再增减
        if (isScoreFixed) return;

        int finalScore = Mathf.RoundToInt(score * _fishMultiplier);
        totalScore -= score;
        Debug.Log($"[-{score}] {hitObject.name} 离开 {zoneName}，总分：{totalScore}");
        OnScoreChanged?.Invoke(totalScore);
    }

    public void FixCurrentScore()
    {
        isScoreFixed = true;
        Debug.Log($"分数已固定，最终分数：{totalScore}");
    }

    public void ResetScore()
    {
        if (isScoreFixed)
        {
            Debug.LogWarning("分数已固定，无法重置。如需重置请先调用 UnfixScore()");
            return;
        }

        totalScore = 0;
        _fishMultiplier = 1f; // 重置鱼类倍率
        OnScoreChanged?.Invoke(totalScore);
        Debug.Log("分数已重置");
    }

    public void UnfixScore()
    {
        isScoreFixed = false;
        Debug.Log("分数已解锁，可以继续增减");
    }

    public int GetTotalScore()
    {
        return totalScore;
    }

    public bool IsScoreFixed()
    {
        return isScoreFixed;
    }
}