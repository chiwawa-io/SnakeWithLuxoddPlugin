using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Achievements : MonoBehaviour
{
    [Header("AchievementsDataBase")] 
    [SerializeField] private List<AchievementSO> achievementsList = new();
    
    [Header("AchievementsUI")]
    [SerializeField] private Transform achievementsUIParent;
    [SerializeField] private GameObject achievementsUIPrefab;
    [SerializeField] private TextMeshProUGUI timerNumber;

    private int _errorCode;
    private string _errorMessage;
    
    private void OnEnable()
    {
        InactivityDetector.UpdateTimers = UpdateTimer;
        InactivityDetector.Quit = QuitA;
    }

    private void Start()
    {
        LoadAchievements();
    }

    void LoadAchievements()
    {
        foreach (Transform child in achievementsUIParent)
        {
            Destroy(child.gameObject);
        }

        foreach (AchievementSO achievementData in achievementsList)
        {
            var achievementRow = Instantiate(achievementsUIPrefab, achievementsUIParent);

            bool isCompleted = PlayerDataManager.Instance.IsAchievementCompleted(achievementData.id);

            var rowUI = achievementRow.GetComponent<AchievementsRowUI>();
            rowUI.Setup(achievementData, isCompleted);
        }
    }

    public void ReturnToMenu ()
    {
        SceneManager.LoadScene("Menu");
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene("Loading");
    }

    private void QuitA()
    {
        Quit();
    }

    public void Quit(bool isQuitWithError = false)
    {
        NetworkManager.Instance.HealthStatusCheckService.Deactivate();
        NetworkManager.Instance.WebSocketService.CloseConnection();
        if (!isQuitWithError) NetworkManager.Instance.WebSocketService.BackToSystem();
        else NetworkManager.Instance.WebSocketService.BackToSystemWithError(_errorMessage, _errorCode.ToString());
        Application.Quit();
    } 
    
    private void UpdateTimer(int timeLeft)
    {
        timerNumber.text = timeLeft.ToString();
    }
}
