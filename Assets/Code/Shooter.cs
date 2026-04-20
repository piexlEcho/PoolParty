using UnityEngine;

/// <summary>
/// 3D 子弹发射器：按下指定按键从射击口生成子弹，子弹沿射击口前方飞行。
/// 速度与方向可通过其他脚本或 Inspector 动态修改。
/// </summary>
public class Shooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public KeyCode shootKey = KeyCode.Mouse0;
    public float bulletSpeed = 20f;

    public bool useCustomDirection = false;

    public Vector3 customDirection = Vector3.forward;

    private void Update()
    {
        if (Input.GetKeyDown(shootKey))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        if (shootPoint == null)
        {
            return;
        }

        Vector3 shootDirection = GetShootDirection();

        // 实例化子弹
        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = shootDirection.normalized * bulletSpeed;
        }

        // 子弹朝向与方向一致
        bullet.transform.forward = shootDirection.normalized;
    }

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

    // ---------- 以下为方便外部调节速度与方向的公共方法 ----------
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