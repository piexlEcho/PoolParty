using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFeedbackController : MonoBehaviour
{
    [Header("Charge")]
    public float chargeFOV = 50f;

    public float chargeTilt = 8f;

    public float chargeSmooth = 0.1f;

    [Header("Fire")]
    public float fireKick = 4f;

    public float returnTime = 0.2f;

    private Camera cam;

    private float defaultFOV;

    private Quaternion defaultRot;

    private Coroutine fovRoutine;

    private Coroutine rotRoutine;

    void Awake()
    {
        cam = GetComponent<Camera>();

        defaultFOV = cam.fieldOfView;

        defaultRot = transform.localRotation;
    }

    public void StartCharge(float chargePercent)
    {
        float targetFOV =
            Mathf.Lerp(defaultFOV, chargeFOV, chargePercent);

        float targetTilt =
            Mathf.Lerp(0f, chargeTilt, chargePercent);

        AnimateFOV(targetFOV, chargeSmooth);

        AnimateRotation(
            Quaternion.Euler(targetTilt, 0f, 0f),
            chargeSmooth
        );
    }

    public void ReleaseCharge()
    {
        AnimateFOV(defaultFOV, returnTime);

        AnimateRotation(defaultRot, returnTime);
    }

    public void FireKick()
    {
        StopAllCoroutines();

        StartCoroutine(FireKickRoutine());
    }

    IEnumerator FireKickRoutine()
    {
        Quaternion kickRot =
            Quaternion.Euler(fireKick, 0f, 0f);

        transform.localRotation = kickRot;

        yield return new WaitForSeconds(0.05f);

        AnimateRotation(defaultRot, returnTime);
    }

    void AnimateFOV(float target, float duration)
    {
        if (fovRoutine != null)
        {
            StopCoroutine(fovRoutine);
        }

        fovRoutine =
            StartCoroutine(FOVRoutine(target, duration));
    }

    IEnumerator FOVRoutine(float target, float duration)
    {
        float start = cam.fieldOfView;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            cam.fieldOfView = Mathf.Lerp(
                start,
                target,
                elapsed / duration
            );

            yield return null;
        }

        cam.fieldOfView = target;
    }

    void AnimateRotation(
        Quaternion target,
        float duration
    )
    {
        if (rotRoutine != null)
        {
            StopCoroutine(rotRoutine);
        }

        rotRoutine =
            StartCoroutine(
                RotationRoutine(target, duration)
            );
    }

    IEnumerator RotationRoutine(
        Quaternion target,
        float duration
    )
    {
        Quaternion start = transform.localRotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            transform.localRotation =
                Quaternion.Lerp(
                    start,
                    target,
                    elapsed / duration
                );

            yield return null;
        }

        transform.localRotation = target;
    }
}