using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    [Header("Fish Types")]
    public FishData[] fishTypes;

    [Header("Slot UI")]
    public Image[] slotImages;
    public TextMeshProUGUI[] slotLabels;
    public TextMeshProUGUI multiplierText;

    [Header("Buttons")]
    public Button spinButton;
    public Button confirmButton;

    [Header("Spin Settings")]
    public float spinDuration = 2f;
    public float spinTickInterval = 0.08f;

    [Header("References")]
    public ArcadeCameraSwitch arcadeCamera;
    public FishSpawner fishSpawner;

    private FishData _result;
    private bool _isSpinning = false;

    void Start()
    {
        confirmButton.interactable = false;
        spinButton.onClick.AddListener(OnSpin);
        confirmButton.onClick.AddListener(OnConfirm);
    }

    public void OnSpin()
    {
        if (_isSpinning) return;
        spinButton.interactable = false;
        confirmButton.interactable = false;
        RoundManager.Instance?.SpawnRiceBall();

        if (multiplierText != null)
            multiplierText.text = "?";

        StartCoroutine(SpinRoutine());
    }

    IEnumerator SpinRoutine()
    {
        _isSpinning = true;
        _result = PickWeightedFish();

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            foreach (var img in slotImages)
            {
                if (img != null)
                    img.sprite = fishTypes[Random.Range(0, fishTypes.Length)].icon;
            }
            foreach (var label in slotLabels)
            {
                if (label != null)
                    label.text = fishTypes[Random.Range(0, fishTypes.Length)].fishName;
            }

            yield return new WaitForSeconds(spinTickInterval);
            elapsed += spinTickInterval;
        }

        foreach (var img in slotImages)
            if (img != null) img.sprite = _result.icon;

        foreach (var label in slotLabels)
            if (label != null) label.text = _result.fishName;

        if (multiplierText != null)
            multiplierText.text = $"{_result.multiplier}x";

        _isSpinning = false;
        spinButton.interactable = true;
        confirmButton.interactable = true;
    }

    FishData PickWeightedFish()
    {
        float total = 0f;
        foreach (var f in fishTypes) total += f.weight;

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var f in fishTypes)
        {
            cumulative += f.weight;
            if (roll <= cumulative) return f;
        }

        return fishTypes[fishTypes.Length - 1];
    }

    void OnConfirm()
    {
        if (_result == null) return;

        confirmButton.interactable = false;
        spinButton.interactable = false;

        ScoreManager.Instance?.SetFishMultiplier(_result.multiplier);

        int fishIndex = System.Array.IndexOf(fishTypes, _result);
        fishSpawner?.SpawnFish(fishIndex);

        arcadeCamera.escEnabled = true;

        arcadeCamera?.ToggleCamera();
    }

    public void ResetForNewRound()
    {
        if (spinButton == null) return;

        spinButton.interactable = true;
        confirmButton.interactable = false;

        if (multiplierText != null)
            multiplierText.text = "?";

        foreach (var img in slotImages)
            if (img != null) img.sprite = null;

        foreach (var label in slotLabels)
            if (label != null) label.text = "-";

        _result = null;
        _isSpinning = false;
    }
}