using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DishCam : MonoBehaviour
{
    [Header("Dish Overview")]
    public Transform dishViewPoint;
    public float transitionDuration = 1f;
    public Ease transitionEase = Ease.InOutQuad;

    [Header("References")]
    public CameraFollowTopDown cameraFollow;

    private bool _triggered = false;
    private Transform _cam;

    void Start() { }

    void Awake()
    {
        _cam = Camera.main.transform;
    }
    public void ResetForNewRound()
    {
        Debug.Log($"[DishCam] ResetForNewRound called — _triggered was {_triggered}, enabled was {enabled}");
        _triggered = false;
        enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[DishCam] OnTriggerEnter hit — object: {other.name}, tag: {other.tag}, _triggered: {_triggered}, enabled: {enabled}");

        if (_triggered) return; 
        if (!other.CompareTag("RiceGrain")) return;

        Debug.Log($"[DishCam] Transitioning — cameraFollow null: {cameraFollow == null}, _cam null: {_cam == null}, dishViewPoint null: {dishViewPoint == null}");

        _triggered = true;

        if (cameraFollow != null)
            cameraFollow.StopFollowing();

        _cam.DOMove(dishViewPoint.position, transitionDuration).SetEase(transitionEase);
        _cam.DORotateQuaternion(dishViewPoint.rotation, transitionDuration).SetEase(transitionEase);
    }
}
