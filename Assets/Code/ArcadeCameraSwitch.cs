using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ArcadeCameraSwitch : MonoBehaviour
{
    [Header("Transform")]
    [SerializeField] public Transform targetPoint;
    [SerializeField] public float cameraMoveDuration = 1.0f;
    [SerializeField] public Ease easeType = Ease.InOutQuad;

    [Header("Toggle")]
    [SerializeField] public MonoBehaviour scriptToToggle;


    [Header("Rotation")]
    [SerializeField] public Vector3 originalPosition;
    [SerializeField] public Quaternion originalRotation;
    [SerializeField] public bool isAtTarget = false;
    [SerializeField] private Tween moveTween;
    [SerializeField] private Tween rotateTween;


    private void Start()
    {
        originalPosition = transform.position; 
        originalRotation = transform.rotation;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCamera();
            Debug.Log("fuck off");
        }
    }
    public void ToggleCamera()
    {
        moveTween?.Kill();
        rotateTween?.Kill();

        if (!isAtTarget)
        {
            moveTween = transform.DOMove(targetPoint.position, cameraMoveDuration).SetEase(easeType);
            rotateTween = transform.DORotateQuaternion(targetPoint.rotation, cameraMoveDuration).SetEase(easeType);
            scriptToToggle.enabled = false;
        }
        else
        {
            moveTween = transform.DOMove(targetPoint.position, cameraMoveDuration).SetEase(easeType);
            rotateTween = transform.DORotateQuaternion(targetPoint.rotation, cameraMoveDuration).SetEase(easeType);
            scriptToToggle.enabled = true;
        }
        isAtTarget = !isAtTarget;
    }
}