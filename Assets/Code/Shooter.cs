using UnityEngine;

/// <summary>
/// 3D 子弹发射器：按下指定按键从射击口生成子弹，子弹沿射击口前方飞行。
/// 速度与方向可通过其他脚本或 Inspector 动态修改。
/// </summary>
public class Shooter : MonoBehaviour
{
    [Header("发射配置")]
    [Tooltip("子弹预制体（需带有 Rigidbody 组件）")]
    public GameObject bulletPrefab;

    [Tooltip("发射口 Transform（子弹生成位置与方向参照）")]
    public Transform shootPoint;

    [Tooltip("发射按键")]
    public KeyCode shootKey = KeyCode.Mouse0;

    [Header("子弹属性")]
    [Tooltip("子弹飞行速度（米/秒）")]
    public float bulletSpeed = 20f;

    [Tooltip("是否启用自定义发射方向（若不勾选，则使用射击口的前向）")]
    public bool useCustomDirection = false;

    [Tooltip("自定义发射方向（世界坐标系）")]
    public Vector3 customDirection = Vector3.forward;

    /// <summary>
    /// 每帧检测输入并发射子弹
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(shootKey))
        {
            Shoot();
        }
    }

    /// <summary>
    /// 发射子弹（可由外部脚本直接调用）
    /// </summary>
    public void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("子弹预制体未赋值！");
            return;
        }

        if (shootPoint == null)
        {
            Debug.LogWarning("射击口 Transform 未赋值！");
            return;
        }

        // 计算最终发射方向
        Vector3 shootDirection = GetShootDirection();

        // 实例化子弹
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

        // 赋予子弹速度（假设子弹预制体带有 Rigidbody）
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = shootDirection.normalized * bulletSpeed;
        }
        else
        {
            Debug.LogError("子弹预制体缺少 Rigidbody 组件，无法使用物理速度移动。");
            // 若没有 Rigidbody，可选择自行编写移动逻辑，此处略。
        }

        // 可选：让子弹朝向与飞行方向一致
        bullet.transform.forward = shootDirection.normalized;
    }

    /// <summary>
    /// 获取当前的发射方向（世界坐标系单位向量）
    /// </summary>
    private Vector3 GetShootDirection()
    {
        if (useCustomDirection)
        {
            return customDirection.normalized;
        }
        else
        {
            return shootPoint.forward;
        }
    }

    // ---------- 以下为方便外部系统调节速度与方向的公共方法 ----------

    /// <summary>
    /// 设置子弹速度（米/秒）
    /// </summary>
    public void SetBulletSpeed(float newSpeed)
    {
        bulletSpeed = Mathf.Max(0f, newSpeed);
    }

    /// <summary>
    /// 设置发射方向为射击口的当前前向（关闭自定义方向）
    /// </summary>
    public void UseShootPointForward()
    {
        useCustomDirection = false;
    }

    /// <summary>
    /// 设置自定义发射方向（自动开启自定义方向）
    /// </summary>
    public void SetCustomDirection(Vector3 newDirection)
    {
        useCustomDirection = true;
        customDirection = newDirection.normalized;
    }

    /// <summary>
    /// 直接修改射击口 Transform 的旋转（会影响射击口前向）
    /// </summary>
    public void RotateShootPoint(Quaternion newRotation)
    {
        if (shootPoint != null)
        {
            shootPoint.rotation = newRotation;
        }
    }
}