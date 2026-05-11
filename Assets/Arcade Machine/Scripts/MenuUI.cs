using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject tutorialPanel;
    public GameObject settingsPanel;
    public GameObject leaderPanel;

    void Start()
    {
        ShowMain();
    }
    public void Play()
    {
        SceneManager.LoadScene("Game");
    }
    public void ShowTutorial()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (tutorialPanel) tutorialPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (leaderPanel) leaderPanel.SetActive(false);
    }
    public void ShowMain()
    {
        if (mainPanel) mainPanel.SetActive(true);
        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (leaderPanel) leaderPanel.SetActive(false);
    }
    public void ShowSettings()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
        if (leaderPanel) leaderPanel.SetActive(false);
    }
    public void ShowLeader()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (leaderPanel) leaderPanel.SetActive(true);
    }
    // Update is called once per frame
    public void QuitGame()
    {
        Application.Quit();
    }
}
