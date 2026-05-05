using UnityEngine;
using System.Collections.Generic;

public class CameraTopDownTracker : MonoBehaviour
{
    [Header("追踪参数")]
    public string grainTag = "RiceGrain";

    [Tooltip("摄像固定高度")]
    public float fixedHeight = 10f;

    [Tooltip("俯视角度")]
    [Range(0f, 90f)]
    public float lookDownAngle = 90f;

    [Tooltip("平滑跟随时间")]
    public float smoothTime = 0.3f;

    [Header("Z 轴限制")]
    public float maxTrackingZ = 45f;

    [Header("状态")]
    public bool isTracking = false;

    private float targetZ;
    private float smoothVelocity = 0f;
    private float recordedMaxZ = float.MinValue;
    private float fixedX;
    private float fixedY;

    private void Start()
    {
        targetZ = transform.position.z;
    }

    public void StartTracking()
    {
        isTracking = true;
        recordedMaxZ = float.MinValue;

        // 锁定xy
        fixedX = transform.position.x;
        fixedY = fixedHeight;
    }

    public void StopTracking()
    {
        isTracking = false;
    }

    private void Update()
    {
        if (!isTracking) return;

        // 不超过最大追踪距离
        float currentMaxZ = FindMaxGrainZ();
        if (currentMaxZ > recordedMaxZ)
        {
            recordedMaxZ = Mathf.Min(currentMaxZ, maxTrackingZ);
        }

        float desiredZ = recordedMaxZ;

        // 平滑 Z移动
        targetZ = Mathf.SmoothDamp(targetZ, desiredZ, ref smoothVelocity, smoothTime);

        transform.position = new Vector3(fixedX, fixedY, targetZ);

        transform.rotation = Quaternion.Euler(lookDownAngle, 0f, 0f);
    }

    private float FindMaxGrainZ()
    {
        GameObject[] grains = GameObject.FindGameObjectsWithTag(grainTag);
        float maxZ = float.MinValue;
        foreach (GameObject grain in grains)
        {
            float z = grain.transform.position.z;
            if (z > maxZ)
                maxZ = z;
        }
        return maxZ;
    }
}