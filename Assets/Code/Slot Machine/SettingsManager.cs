using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Volume")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumePercentText; // Shows "75%"
    public float defaultVolume = 1f;

    [Header("Fullscreen")]
    public GameObject fullscreenOnIcon;  // Icon shown when fullscreen is ON
    public GameObject fullscreenOffIcon; // Icon shown when fullscreen is OFF
    public bool defaultFullscreen = true;

    private const string VOLUME_KEY = "Settings_Volume";
    private const string FULLSCREEN_KEY = "Settings_Fullscreen";

    void Start()
    {
        LoadSettings();
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        UpdateFullscreenIcons();
    }
    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        UpdateVolumeText();
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
    }

    void UpdateVolumeText()
    {
        if (volumePercentText != null)
            volumePercentText.text = $"{Mathf.RoundToInt(volumeSlider.value * 100)}%";
    }
    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0);
        UpdateFullscreenIcons();
    }

    void UpdateFullscreenIcons()
    {
        if (fullscreenOnIcon != null)
            fullscreenOnIcon.SetActive(Screen.fullScreen);
        if (fullscreenOffIcon != null)
            fullscreenOffIcon.SetActive(!Screen.fullScreen);
    }
    public void ResetToDefaults()
    {
        volumeSlider.value = defaultVolume;
        AudioListener.volume = defaultVolume;
        PlayerPrefs.SetFloat(VOLUME_KEY, defaultVolume);

        Screen.fullScreen = defaultFullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, defaultFullscreen ? 1 : 0);

        UpdateVolumeText();
        UpdateFullscreenIcons();
    }

    void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, defaultVolume);
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        bool savedFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, defaultFullscreen ? 1 : 0) == 1;
        Screen.fullScreen = savedFullscreen;

        UpdateVolumeText();
        UpdateFullscreenIcons();
    }
}
