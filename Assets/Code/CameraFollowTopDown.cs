using UnityEngine;

public class CameraFollowTopDown : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    [Tooltip("镜头平滑速度")]
    public float smoothSpeed = 8f;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 10f, -6f);

    [Header("Rotation")]
    public Vector3 cameraAngle = new Vector3(72f, 0f, 0f);

    [Header("Limit")]
    public float maxZ = 45f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;

        desiredPos.z = Mathf.Min(desiredPos.z, maxZ);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref velocity,
            1f / smoothSpeed
        );

        transform.rotation = Quaternion.Euler(cameraAngle);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}