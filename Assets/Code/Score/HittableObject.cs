using UnityEngine;

public class HittableObject : MonoBehaviour
{
    [Header("分数")]
    public int scoreValue = 10;

    private int originalScore;

    private string originalTag;

    void Awake()
    {
        originalScore = scoreValue;
    }


    public void SetScoreValue(int newScore)    // 区域倍率
    {
        scoreValue = newScore;
    }

    public void ResetScore()
    {
        scoreValue = originalScore;
    }

    public void SetAsScoring()
    {
        if (gameObject.tag != "Score")
        {
            originalTag = gameObject.tag;
            gameObject.tag = "Score";
        }
    }

    public void SetAsNormal()
    {
        if (gameObject.tag == "Score")
        {
            gameObject.tag = originalTag;
        }
    }
}