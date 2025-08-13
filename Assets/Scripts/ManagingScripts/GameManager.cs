using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance  { get; private set; }

    private int _currentScore;
    private int _gameDuration;

    public static Action<int> OnGameOver;
    public static Action<int, string> OnError;
    
    private int _errorCode;
    private string _errorMessage;

    private void OnEnable()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
        
        Player.UpdateScore += UpdateScore;
        InactivityDetector.Quit += ForceQuit;
        
        _gameDuration = (int) Time.time;
    }

    void Start()
    {
        LevelBegin();
    }

    void LevelBegin()
    {
        NetworkManager.Instance.WebSocketCommandHandler.SendLevelBeginRequestCommand(0,OnLevelBeginSuccess, OnLevelBeginError);
    }

    void OnLevelBeginSuccess() {}

    void OnLevelBeginError(int code,string msg)
    {
        OnError?.Invoke(code, msg);
        _errorCode = code;
        _errorMessage = msg;
    }

    private void OnDisable()
    {
        Player.UpdateScore -= UpdateScore;
        InactivityDetector.ForceStart -= ForceQuit;
    }

    void UpdateScore(int lengthOfSnake, Vector2 position)
    {
        _currentScore +=  lengthOfSnake*100;
        GameUI.Instance.UpdateScoreText(_currentScore);
    }

    public void GameOver()
    {
        OnGameOver?.Invoke(_currentScore);
        LevelEnd();
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    void LevelEnd()
    {
        NetworkManager.Instance.WebSocketCommandHandler.SendLevelEndRequestCommand(0, _currentScore, OnLevelEndSuccess, OnLevelEndError);
        PlayerDataManager.Instance.AddExperience((int)(Time.time - _gameDuration)*2);
    }

    void OnLevelEndSuccess()
    {
        PlayerDataManager.Instance.CheckAndSaveScore(_currentScore);
    }

    void OnLevelEndError(int code, string msg)
    {
        OnError?.Invoke(code, msg);
        _errorCode = code;
        _errorMessage = msg;
    }
    
    public void ReloadGame()
    {
        SceneManager.LoadScene("Loading");
    }

    void ForceQuit()
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
}
