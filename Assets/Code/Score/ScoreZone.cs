using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScoreZone : MonoBehaviour
{
    [Header("区域权重")]
    public float scoreMultiplier = 1f;

    [Header("可选：只处理特定原始标签的物体（留空则处理所有）")]
    public string[] validOriginalTags = { };

    [Header("防抖动设置")]
    [Tooltip("同一物体进入/退出的冷却时间（秒）")]
    public float cooldownTime = 0.5f;

    private ScoreManager scoreManager;
    private Dictionary<GameObject, int> objectsInZone = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, float> objectCooldown = new Dictionary<GameObject, float>();

    void Start()
    {
        scoreManager = ScoreManager.Instance;

        if (scoreManager == null)
        {
            Debug.LogError("场景中没有ScoreManager！");
        }

        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        // 清理过期的冷却记录
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kvp in objectCooldown)
        {
            if (Time.time > kvp.Value)
            {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var obj in toRemove)
        {
            objectCooldown.Remove(obj);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 检查冷却
        if (objectCooldown.ContainsKey(other.gameObject))
        {
            return;
        }

        // 如果分数已固定，不再处理计分，但继续改变标签
        bool isScoreFixed = scoreManager != null && scoreManager.IsScoreFixed();

        HittableObject hittable = other.GetComponent<HittableObject>();
        if (hittable == null) return;

        string originalTag = other.tag;
        if (!IsValidOriginalTag(originalTag)) return;

        if (!objectsInZone.ContainsKey(other.gameObject))
        {
            objectsInZone.Add(other.gameObject, hittable.scoreValue);

            // 只有分数未固定时才计分
            if (!isScoreFixed)
            {
                int finalScore = Mathf.RoundToInt(hittable.scoreValue * scoreMultiplier);
                scoreManager?.AddScore(finalScore, gameObject.name, other.gameObject);
            }

            // 改变标签为Score（吸引用，不管分数是否固定都要改）
            hittable.SetAsScoring();

            Debug.Log($"{other.gameObject.name} 进入 {gameObject.name}，标签改为Score" +
                      (isScoreFixed ? "（分数已固定，不计分）" : $"，+{Mathf.RoundToInt(hittable.scoreValue * scoreMultiplier)}分"));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (objectsInZone.ContainsKey(other.gameObject))
        {
            // 只有分数未固定时才扣分
            if (scoreManager != null && !scoreManager.IsScoreFixed())
            {
                int originalScore = objectsInZone[other.gameObject];
                int lostScore = Mathf.RoundToInt(originalScore * scoreMultiplier);
                scoreManager?.SubtractScore(lostScore, gameObject.name, other.gameObject);
            }

            // 恢复原始标签
            HittableObject hittable = other.GetComponent<HittableObject>();
            if (hittable != null)
            {
                hittable.SetAsNormal();
            }

            objectsInZone.Remove(other.gameObject);

            // 添加冷却，防止反复进出
            objectCooldown[other.gameObject] = Time.time + cooldownTime;

            Debug.Log($"{other.gameObject.name} 离开 {gameObject.name}，标签恢复，进入冷却 {cooldownTime} 秒");
        }
    }

    private bool IsValidOriginalTag(string tag)
    {
        // 如果数组为空，处理所有标签
        if (validOriginalTags.Length == 0) return true;

        foreach (string validTag in validOriginalTags)
        {
            if (tag == validTag) return true;
        }
        return false;
    }

    void OnDestroy()
    {
        foreach (var item in objectsInZone)
        {
            if (item.Key != null)
            {
                if (scoreManager != null && !scoreManager.IsScoreFixed())
                {
                    int lostScore = Mathf.RoundToInt(item.Value * scoreMultiplier);
                    scoreManager?.SubtractScore(lostScore, gameObject.name, item.Key);
                }

                HittableObject hittable = item.Key.GetComponent<HittableObject>();
                if (hittable != null)
                {
                    hittable.SetAsNormal();
                }
            }
        }
        objectsInZone.Clear();
    }
}