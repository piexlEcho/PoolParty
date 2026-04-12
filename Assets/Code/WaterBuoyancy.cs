using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WaterBuoyancy : MonoBehaviour
{
    [Header("浮力参数")]
    [Tooltip("液体密度，水的密度约为 1000 kg/m³")]
    public float density = 1000f;
    [Tooltip("浮力强度系数，调大可增加上浮力")]
    public float buoyancyMultiplier = 1.2f;
    [Tooltip("垂直方向的速度阻尼系数 (0-1)，值越大停止越快")]
    [Range(0f, 1f)]
    public float verticalDamping = 0.5f;
    [Tooltip("水平方向的速度阻尼系数")]
    [Range(0f, 1f)]
    public float horizontalDamping = 0.1f;

    [Header("水面区域")]
    [Tooltip("水面最高点 Y 坐标（世界空间）")]
    public float waterSurfaceLevel;

    private BoxCollider waterCollider;
    private float waterTopY;
    private float waterBottomY;

    void Awake()
    {
        waterCollider = GetComponent<BoxCollider>();
        if (!waterCollider.isTrigger)
            Debug.LogWarning("Water Collider 建议设为 Is Trigger");

        // 计算水面实际范围
        Bounds bounds = waterCollider.bounds;
        waterTopY = bounds.max.y;
        waterBottomY = bounds.min.y;
        // 若未手动指定水面高度，默认使用 Collider 顶部
        if (waterSurfaceLevel == 0f)
            waterSurfaceLevel = waterTopY;
    }

    void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        // 获取物体的包围盒
        Bounds objBounds = GetObjectBounds(other);
        float objBottomY = objBounds.min.y;
        float objTopY = objBounds.max.y;
        float objHeight = objBounds.size.y;

        // 计算浸没深度（限制在 0 到物体高度之间）
        float submergedDepth = Mathf.Clamp(waterSurfaceLevel - objBottomY, 0f, objHeight);
        if (submergedDepth <= 0f) return;

        // 浸没比例 (0~1)
        float submergedRatio = submergedDepth / objHeight;

        // 计算浸没体积（近似为包围盒体积 × 浸没比例）
        float volume = objBounds.size.x * objBounds.size.y * objBounds.size.z;
        float submergedVolume = volume * submergedRatio;

        // 浮力大小： F = ρ * g * V
        float buoyancyForce = density * Physics.gravity.magnitude * submergedVolume * buoyancyMultiplier;

        // 施力点：物体浸没部分的几何中心 (向上偏移一点以提供稳定力矩)
        Vector3 forcePoint = new Vector3(
            objBounds.center.x,
            objBottomY + submergedDepth * 0.5f,
            objBounds.center.z
        );

        // 施加浮力
        rb.AddForceAtPosition(Vector3.up * buoyancyForce, forcePoint, ForceMode.Force);

        // 施加速度阻尼（抵消振荡）
        ApplyDamping(rb, submergedRatio);
    }

    void ApplyDamping(Rigidbody rb, float submergedRatio)
    {
        // 阻尼强度随浸没比例增加，完全浸没时阻尼最强
        float verticalDamp = Mathf.Lerp(0f, verticalDamping, submergedRatio);
        float horizontalDamp = Mathf.Lerp(0f, horizontalDamping, submergedRatio);

        Vector3 vel = rb.velocity;

        // 垂直方向阻尼（只影响 Y 轴速度）
        if (verticalDamp > 0f)
            vel.y *= (1f - verticalDamp * Time.fixedDeltaTime * 50f);  // 乘系数使调节手感舒适

        // 水平方向阻尼
        if (horizontalDamp > 0f)
        {
            vel.x *= (1f - horizontalDamp * Time.fixedDeltaTime * 30f);
            vel.z *= (1f - horizontalDamp * Time.fixedDeltaTime * 30f);
        }

        rb.velocity = vel;

    }

    // 获取物体精确包围盒（包括所有碰撞器/渲染器）
    Bounds GetObjectBounds(Collider col)
    {
        // 优先使用 Collider 自身 bounds
        Bounds bounds = col.bounds;

        Renderer[] renderers = col.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    void OnDrawGizmosSelected()
    {
        if (waterCollider == null) waterCollider = GetComponent<BoxCollider>();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(waterCollider.bounds.center, waterCollider.bounds.size);
        Gizmos.color = Color.blue;
        Vector3 lineStart = new Vector3(transform.position.x, waterSurfaceLevel, transform.position.z);
        Vector3 lineEnd = lineStart + Vector3.right * waterCollider.bounds.size.x * 0.5f;
        Gizmos.DrawLine(lineStart, lineEnd);
    }
}