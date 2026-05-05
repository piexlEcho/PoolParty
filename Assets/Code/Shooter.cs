using UnityEngine;

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

    public void UseShootPointForward()
    {
        useCustomDirection = false;
    }

    public void SetCustomDirection(Vector3 newDirection)
    {
        useCustomDirection = true;
        customDirection = newDirection.normalized;
    }

    public void RotateShootPoint(Quaternion newRotation)
    {
        if (shootPoint != null)
        {
            shootPoint.rotation = newRotation;
        }
    }
}