using UnityEngine;
using UnityEngine.Events;
using TMPro;  // 添加 TMPro 命名空间

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("当前分数")]
    public int totalScore = 0;

    [Header("UI 显示")]
    [Tooltip("用于显示分数的 TextMeshPro 文本组件")]
    public TextMeshProUGUI scoreText;

    public string scoreSuffix = "!!!";
    public string startPrompt = "!!Go!!";

    public Color normalColor = Color.white;
    public Color fixedColor = Color.yellow;

    [Header("事件")]
    public UnityEvent<int> OnScoreChanged;

    private bool isScoreFixed = false;  // 分数是否已固定
    private float _fishMultiplier = 1f;
    private bool hasEverScored = false;  // 是否曾经得到过分数（用于显示提示）

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

    void Start()
    {
        // 初始化分数显示
        UpdateScoreDisplay();
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
        totalScore += finalScore;

        // 标记曾经得到过分数
        if (!hasEverScored && finalScore > 0)
        {
            hasEverScored = true;
        }

        // 更新 UI 显示
        UpdateScoreDisplay();

        // 触发事件
        OnScoreChanged?.Invoke(totalScore);

        RoundManager.Instance?.NotifyScoreRegistered();
    }

    public void SubtractScore(int score, string zoneName, GameObject hitObject)
    {
        // 如果分数已固定，不再增减
        if (isScoreFixed) return;

        int finalScore = Mathf.RoundToInt(score * _fishMultiplier);
        totalScore -= finalScore;

        // 更新 UI 显示
        UpdateScoreDisplay();

        // 触发事件
        OnScoreChanged?.Invoke(totalScore);
    }

    public void FixCurrentScore()
    {
        isScoreFixed = true;
        Debug.Log($"分数已固定，最终分数：{totalScore}");

        // 固定分数时改变文本颜色
        UpdateScoreDisplay();
        if (scoreText != null)
        {
            scoreText.color = fixedColor;
        }
    }

    public void ResetScore()
    {
        if (isScoreFixed)
        {
            return;
        }

        totalScore = 0;
        _fishMultiplier = 1f;
        hasEverScored = false;  // 重置得分标记

        // 更新 UI 显示
        UpdateScoreDisplay();

        // 恢复文本颜色
        if (scoreText != null)
        {
            scoreText.color = normalColor;
        }

        OnScoreChanged?.Invoke(totalScore);
        Debug.Log("分数已重置");
    }

    public void UnfixScore()
    {
        isScoreFixed = false;

        // 恢复文本颜色
        if (scoreText != null)
        {
            scoreText.color = normalColor;
        }
    }

    public int GetTotalScore()
    {
        return totalScore;
    }

    public bool IsScoreFixed()
    {
        return isScoreFixed;
    }

    // 更新分数显示
    private void UpdateScoreDisplay()
    {
        if (scoreText == null) return;

        // 如果分数固定，正常显示分数
        if (isScoreFixed)
        {
            scoreText.text = $"{totalScore}{scoreSuffix}";
            return;
        }

        // 如果还没得到过分数，显示提示
        if (!hasEverScored && totalScore == 0)
        {
            scoreText.text = startPrompt;
        }
        else
        {
            scoreText.text = $"{totalScore}{scoreSuffix}";
        }
    }

    // 外部可调用的显示更新方法（用于手动刷新）
    public void RefreshScoreDisplay()
    {
        UpdateScoreDisplay();
    }

    // 强制重置得分标记（用于特殊情况）
    public void ResetScoreMark()
    {
        hasEverScored = false;
        UpdateScoreDisplay();
    }
}