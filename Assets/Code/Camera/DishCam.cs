using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DishCam : MonoBehaviour
{
    [Header("Dish Overview")]
    public Transform dishViewPoint;        // Empty GameObject positioned above the dish
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

    // Called by RoundManager.StartRound() to reset each round
    public void ResetForNewRound()
    {
        _triggered = false;
        enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("RiceGrain")) return;

        _triggered = true;

        // Stop camera following the grain
        if (cameraFollow != null)
            cameraFollow.StopFollowing();

        // Smoothly pan to dish overview position
        _cam.DOMove(dishViewPoint.position, transitionDuration)
            .SetEase(transitionEase);
        _cam.DORotateQuaternion(dishViewPoint.rotation, transitionDuration)
            .SetEase(transitionEase);
    }
}
