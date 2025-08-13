using System.Collections.Generic;
using System.Linq;
using Luxodd.Game.Scripts.Game.Leaderboard;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    [Header("Leaderboard")]
    [SerializeField] private TextMeshProUGUI[] playerNameText;
    [SerializeField] private TextMeshProUGUI[] playerScoreText;
    [SerializeField] private int leaderboardSize;
    
    [Header("Error Panel")]
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorCodeText;
    [SerializeField] private TextMeshProUGUI errorMessageText;
    [SerializeField] private GameObject errorPanelButton;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerNumber;
    
    private int _currentPlayerRank;
    private bool _isCurrentPlayerShown;
    
    private int _errorCode;
    private string _errorMessage;

    private void OnEnable()
    {
        InactivityDetector.Quit += ForceQuit;
        InactivityDetector.UpdateTimers += UpdateTimer;
        NetworkManager.Instance.WebSocketCommandHandler.SendLeaderboardRequestCommand(OnGetLeaderboardSuccess, OnGetLeaderboardFail);
    }

    private void OnDisable()
    {
        InactivityDetector.Quit -= ForceQuit;
    }

    private void OnGetLeaderboardSuccess(LeaderboardDataResponse response)
    {
        if (response.CurrentUserData != null)
        {
            _currentPlayerRank = response.CurrentUserData.Rank;
        }
        
        if (response.Leaderboard != null)
        {
            var playerList = new List<LeaderboardData>();
            if (response.Leaderboard != null)
            {
                playerList = response.Leaderboard.ToList();
            }

            if (response.CurrentUserData != null && response.CurrentUserData.Rank <= _currentPlayerRank)
            {
                var currentPlayer = response.CurrentUserData;
                
                playerList.Insert(currentPlayer.Rank -1, currentPlayer);
                
                if (playerList.Count > leaderboardSize)
                {
                    playerList.RemoveAt(playerList.Count - 1);
                }
            }
            for (int i = 0; i < leaderboardSize; i++)
            {
                if (i < playerList.Count && playerList[i] != null)
                {
                    playerNameText[i].text = playerList[i].PlayerName;
                    playerScoreText[i].text = playerList[i].TotalScore.ToString("D10");
                }
                else
                {
                    playerNameText[i].text = "Empty";
                    playerScoreText[i].text = (0).ToString("D8");
                }
            }
        }

    }

    private void OnGetLeaderboardFail(int code, string message)
    {
        
        _errorCode = code;
        _errorMessage = message;
        
        errorPanel.SetActive(true);
        errorPanelButton.GetComponent<Button>().Select();
        errorCodeText.text = code.ToString();
        errorMessageText.text = message;
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    
    public void ReloadGame()
    {
        SceneManager.LoadScene("Loading");
    }

    void ForceQuit()
    {
        Quit();
    }

    void UpdateTimer(int timeLeft)
    {
        timerNumber.text = timeLeft.ToString();
    }

    public void Quit(bool isQuitWithError = false)
    {
        NetworkManager.Instance.HealthStatusCheckService.Deactivate();
        NetworkManager.Instance.WebSocketService.CloseConnection();
        if (!isQuitWithError) NetworkManager.Instance.WebSocketService.BackToSystem();
        else NetworkManager.Instance.WebSocketService.BackToSystemWithError(_errorMessage, _errorCode.ToString());
        Application.Quit();
    }
}