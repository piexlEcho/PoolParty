using UnityEngine;
using System.Collections.Generic;

public class CameraTopDownTracker : MonoBehaviour
{
    [Header("追踪参数")]
    public string grainTag = "RiceGrain";

    [Tooltip("摄像机在世界空间中的固定高度 (Y 轴)")]
    public float fixedHeight = 10f;

    [Tooltip("俯视角度 (绕 X 轴，90° 为完全向下)")]
    [Range(0f, 90f)]
    public float lookDownAngle = 90f;

    [Tooltip("平滑跟随时间")]
    public float smoothTime = 0.3f;

    [Header("Z 轴限制")]
    [Tooltip("即使米粒飞得更远，摄像机也不会超过这个 Z 坐标")]
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

        // 锁定当前的 X 和 Y 坐标 (只跟随 Z)
        fixedX = transform.position.x;
        fixedY = fixedHeight;
    }

    public void StopTracking()
    {
        isTracking = false;
    }

    private void LateUpdate()
    {
        if (!isTracking) return;

        // 找到当前所有米粒中 Z 坐标最大的值，并限制不超过最大追踪距离
        float currentMaxZ = FindMaxGrainZ();
        if (currentMaxZ > recordedMaxZ)
        {
            recordedMaxZ = Mathf.Min(currentMaxZ, maxTrackingZ);
        }

        // 使用记录的 Z 值（可能被钳制在 maxTrackingZ）
        float desiredZ = recordedMaxZ;

        // 平滑 Z 轴移动
        targetZ = Mathf.SmoothDamp(targetZ, desiredZ, ref smoothVelocity, smoothTime);

        // 更新摄像机位置：X、Y 固定，Z 平滑跟随
        transform.position = new Vector3(fixedX, fixedY, targetZ);

        // 保持俯视角度
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