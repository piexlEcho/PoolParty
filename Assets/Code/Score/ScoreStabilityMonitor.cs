using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreStabilityMonitor : MonoBehaviour
{
    [Header("稳定检测设置")]
    public float stableDuration = 2f;
    public bool resetOnScoreChange = true;

    [Header("稳定后执行")]
    public GameObject barrierToDestroy;
    public Transform attractCenterPoint;

    [Header("吸引设置")]
    public float attractForce = 10f;
    public float damping = 2f;

    [Header("中心点移动设置")]
    public Transform targetPosition;  // 要移动到的目标坐标
    public float waitBeforeMove = 1f;  // 移动前的等待时间
    public float moveDuration = 1f;    // 移动持续时间

    private Coroutine stabilityCoroutine;
    private bool isStableCompleted = false;
    private bool isAttracting = false;
    private bool isScoreFixed = false;  // 分数是否已固定

    private List<GameObject> attractedObjects = new List<GameObject>();
    private Vector3 originalCenterPointPosition;

    void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.AddListener(OnScoreChanged);
        }
        else
        {
            Debug.LogError("ScoreManager未找到！");
        }

        if (attractCenterPoint != null)
        {
            originalCenterPointPosition = attractCenterPoint.position;
        }
    }

    void Update()
    {
        // 如果正在吸引，每帧收集所有Score标签的物体
        if (isAttracting)
        {
            CollectScoreObjects();
            ApplyAttractForce();
        }
    }

    void OnScoreChanged(int newScore)
    {
        // 如果分数已经固定，不再响应分数变化
        if (isScoreFixed) return;
        if (isStableCompleted) return;

        if (resetOnScoreChange)
        {
            ResetStabilityTimer();
        }
    }

    public void StartStabilityMonitoring()
    {
        ResetStabilityTimer();
    }

    void ResetStabilityTimer()
    {
        if (stabilityCoroutine != null)
        {
            StopCoroutine(stabilityCoroutine);
        }

        stabilityCoroutine = StartCoroutine(StabilityCoroutine());
        Debug.Log($"分数变化，开始新的稳定计时（需要{stableDuration}秒稳定）");
    }

    IEnumerator StabilityCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < stableDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"分数已稳定{stableDuration}秒，开始执行后续操作");
        OnStabilityReached();
    }

    void OnStabilityReached()
    {
        Debug.Log("OnStabilityReached fired");
        if (isStableCompleted) return;
        isStableCompleted = true;

        RoundManager.Instance?.EndRound();

        // 1. 固定分数（不再受进出区域影响）
        FixCurrentScore();

        // 2. 删除阻挡板
        if (barrierToDestroy != null)
        {
            Destroy(barrierToDestroy);
            Debug.Log($"已删除阻挡板: {barrierToDestroy.name}");
        }

        // 3. 开始吸引
        StartAttract();

        // 4. 等待后移动中心点
        StartCoroutine(MoveCenterPointAfterDelay());
        CameraSequenceController.Instance?.BeginEndSequence();
    }

    void FixCurrentScore()
    {
        isScoreFixed = true;

        // 告诉ScoreManager固定当前分数，不再受进出区域影响
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.FixCurrentScore();
        }

        Debug.Log($"分数已固定，当前总分：{ScoreManager.Instance?.GetTotalScore()}");
    }

    void StartAttract()
    {
        isAttracting = true;
        Debug.Log("开始吸引所有Score标签的物体");
    }

    void CollectScoreObjects()
    {
        // 清理已销毁的物体
        attractedObjects.RemoveAll(obj => obj == null);

        // 查找所有Score标签的物体
        GameObject[] scoreObjects = GameObject.FindGameObjectsWithTag("Score");

        foreach (GameObject obj in scoreObjects)
        {
            if (!attractedObjects.Contains(obj))
            {
                attractedObjects.Add(obj);

                // 新物体浮空（禁用重力，但保留碰撞）
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;
                }

                Debug.Log($"新Score物体加入吸引: {obj.name}");
            }
        }
    }

    void ApplyAttractForce()
    {
        if (attractCenterPoint == null) return;

        foreach (GameObject obj in attractedObjects)
        {
            if (obj == null) continue;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null) continue;

            Vector3 direction = attractCenterPoint.position - obj.transform.position;
            float distance = direction.magnitude;

            // 吸引力（距离越远力越大，距离越近力越小）
            float forceMultiplier = Mathf.Clamp01(distance / 3f);
            Vector3 attractForceVec = direction.normalized * attractForce * forceMultiplier;

            // 阻尼力（防止飞过和震荡）
            Vector3 dampingForce = -rb.velocity * damping;

            rb.AddForce(attractForceVec + dampingForce, ForceMode.Acceleration);
        }
    }

    IEnumerator MoveCenterPointAfterDelay()
    {
        // 等待指定时间
        yield return new WaitForSeconds(waitBeforeMove);

        Debug.Log($"开始移动中心点从 {attractCenterPoint.position} 到 {targetPosition.position}");

        // 平滑移动中心点
        Vector3 startPos = attractCenterPoint.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            attractCenterPoint.position = Vector3.Lerp(startPos, targetPosition.position, t);
            yield return null;
        }

        attractCenterPoint.position = targetPosition.position;
        Debug.Log($"中心点移动完成，当前位置：{attractCenterPoint.position}");
    }

    public void StopAttract()
    {
        isAttracting = false;

        foreach (GameObject obj in attractedObjects)
        {
            if (obj != null)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = true;
                }
            }
        }

        attractedObjects.Clear();
        Debug.Log("已停止吸引");
    }

    public void ForceCompleteStability()
    {
        if (stabilityCoroutine != null)
        {
            StopCoroutine(stabilityCoroutine);
        }
        OnStabilityReached();
    }

    public void ResetForNewRound()
    {
        isStableCompleted = false;
        isScoreFixed = false;
        isAttracting = false;

        foreach (GameObject obj in attractedObjects)
        {
            if (obj != null)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null) rb.useGravity = true;
            }
        }
        attractedObjects.Clear();

        if (attractCenterPoint != null)
            attractCenterPoint.position = originalCenterPointPosition;

        if (stabilityCoroutine != null)
        {
            StopCoroutine(stabilityCoroutine);
            stabilityCoroutine = null;
        }
    }

    void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.RemoveListener(OnScoreChanged);
        }
        StopAttract();
    }
}