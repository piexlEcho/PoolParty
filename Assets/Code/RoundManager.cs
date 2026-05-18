using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Fail Detection")]
    public float noScoreTimeout = 8f;

    [Header("Respawn")]
    public GameObject riceBallPrefab;
    public Transform riceBallSpawnPoint;

    [Header("References")]
    public Shooter shooter;
    public CameraFollowTopDown cameraFollowTopDown;

    private bool _roundActive = false;
    private bool _scoreRegistered = false;
    private Coroutine _failTimerCoroutine;
    private GameObject _currentRiceBall;
    public ArcadeCameraSwitch arcadeCamera;

    void Awake() => Instance = this;

    public void StartRound()
    {
        _roundActive = true;
        _scoreRegistered = false;

        ScoreManager.Instance?.UnfixScore();
        ScoreManager.Instance?.ResetScore();

        SpawnRiceBall();
    }

    public void NotifyScoreRegistered()
    {
        if (!_roundActive) return;

        _scoreRegistered = true;

        // Rice made it in — cancel fail timer
        if (_failTimerCoroutine != null)
        {
            StopCoroutine(_failTimerCoroutine);
            _failTimerCoroutine = null;
        }
    }

    public void StartFailTimer()
    {
        _roundActive = true; 
        _scoreRegistered = false;

        if (_failTimerCoroutine != null)
            StopCoroutine(_failTimerCoroutine);

        _failTimerCoroutine = StartCoroutine(FailTimerRoutine());
    }

    IEnumerator FailTimerRoutine()
    {
        yield return new WaitForSeconds(noScoreTimeout);

        if (_roundActive && !_scoreRegistered)
        {
            Debug.Log("Fail state — no rice reached zones, resetting");
            TriggerFailReset();
        }
    }

    void TriggerFailReset()
    {
        _roundActive = false;

        if (cameraFollowTopDown != null)
            cameraFollowTopDown.StopFollowing();

        GameObject[] grains = GameObject.FindGameObjectsWithTag("RiceGrain");
        foreach (var grain in grains) Destroy(grain);

        if (_currentRiceBall != null)
            Destroy(_currentRiceBall);

        // Mark as shooting so ArcadeCameraSwitch knows to call StartRound on arrival
        GameState.Phase = GamePhase.Shooting;
        arcadeCamera?.ToggleCamera();
    }

    void SpawnRiceBall()
    {
        if (_currentRiceBall != null)
            Destroy(_currentRiceBall);

        if (riceBallPrefab != null && riceBallSpawnPoint != null)
        {
            _currentRiceBall = Instantiate(
                riceBallPrefab,
                riceBallSpawnPoint.position,
                riceBallSpawnPoint.rotation
            );
        }
    }

    public void EndRound()
    {
        _roundActive = false;

        if (_failTimerCoroutine != null)
        {
            StopCoroutine(_failTimerCoroutine);
            _failTimerCoroutine = null;
        }
    }
}
