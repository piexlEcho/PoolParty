using DG.Tweening;
using UnityEngine;
using System.Collections;

public class Shooter : MonoBehaviour
{
    public GameObject bulletPrefab, typeTextPrefab;
    public Transform shootPoint;

    public KeyCode startChargeKey = KeyCode.P;
    public KeyCode addPowerKey = KeyCode.B;

    public float baseSpeed = 20f;
    public float maxChargeTime = 1f;
    public float maxSpeed = 200f;
    public float growthRate = 0.35f;

    [Header("Camera Feedback")]
    public CameraFeedbackController cameraFeedback; // Assign in inspector

    private float currentChargeTime = 0f;
    private int bPressCount = 0;
    private bool isCharging = false;

    private Vector3 currentWhoopeeScale;
    public float whoopeeScale = 1.25f;

    public bool useCustomDirection = false;// 是否使用自定义方向
    public Vector3 customDirection = Vector3.forward;

    private void Awake()
    {
        currentWhoopeeScale = transform.localScale;
    }

    private void Update()
    {
        // 开始蓄力
        if (Input.GetKeyDown(startChargeKey) && !isCharging)
        {
            StartCharge();
        }

        if (isCharging)
        {
            currentChargeTime += Time.deltaTime;

            float chargePercent = currentChargeTime / maxChargeTime;

            // Update camera feedback continuously while charging
            cameraFeedback?.StartCharge(chargePercent);

            // 按 B 增加蓄力
            if (Input.GetKeyDown(addPowerKey))
            {
                bPressCount++;
                Text("B", 1f + bPressCount / 10f);
                StartCoroutine(WhoopeeCushion(
                    currentWhoopeeScale * (1 + whoopeeScale * chargePercent), 0.1f));
            }

            // 到时间自动发射
            if (currentChargeTime >= maxChargeTime)
            {
                Text("T", 1f + bPressCount / 10f * 3f);
                StartCoroutine(WhoopeeCushion(currentWhoopeeScale, 0.2f));
                cameraFeedback?.FireKick();
                Fire();
                ResetCharge();
            }
        }
    }

    IEnumerator WhoopeeCushion(Vector3 endValue, float duration)
    {
        float time = 0;
        Vector3 startValue = transform.localScale;

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = endValue;
    }

    void StartCharge()
    {
        isCharging = true;
        currentChargeTime = 0f;
        bPressCount = 0;
        Text("P", 0);
        cameraFeedback?.StartCharge(0f);
    }

    void Text(string show, float bScale)
    {
        GameObject textInstance = Instantiate(typeTextPrefab, new Vector3(0, 0, -100f), Quaternion.identity);
        textInstance.GetComponent<TypeText>().TextToShow = show;
        if (bScale != 0) textInstance.GetComponent<TypeText>().bScale = bScale;
    }

    void ResetCharge()
    {
        isCharging = false;
        currentChargeTime = 0f;
        bPressCount = 0;
        cameraFeedback?.ReleaseCharge();
    }

    void Fire()
    {
        if (bulletPrefab == null || shootPoint == null) return;

        Vector3 dir = GetShootDirection();

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float speed = CalculateSpeed();
            rb.velocity = dir.normalized * speed;
        }

        bullet.transform.forward = dir.normalized;
    }

    float CalculateSpeed()
    {
        if (bPressCount <= 0)
        {
            return baseSpeed;
        }

        float t = 1f - Mathf.Exp(-growthRate * bPressCount);

        float speed = Mathf.Lerp(baseSpeed, maxSpeed, t);

        return Mathf.Min(speed, maxSpeed);
    }

    private Vector3 GetShootDirection()
    {
        if (useCustomDirection)
            return customDirection.normalized;
        else
            return shootPoint.forward;
    }

    // ---------- 外部接口 ----------
    public void SetCustomDirection(Vector3 newDirection)
    {
        useCustomDirection = true;
        customDirection = newDirection.normalized;
    }

    public void UseShootPointForward()
    {
        useCustomDirection = false;
    }

    public void RotateShootPoint(Quaternion newRotation)
    {
        if (shootPoint != null)
            shootPoint.rotation = newRotation;
    }
}