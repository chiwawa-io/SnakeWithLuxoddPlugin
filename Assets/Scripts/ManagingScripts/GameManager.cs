using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance  { get; private set; }

    private int _currentScore;

    public static Action<int> OnGameOver;

    private void OnEnable()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
        
        Player.UpdateScore += UpdateScore;
    }
    
    private void OnDisable()
    {
        Player.UpdateScore -= UpdateScore;
    }

    void UpdateScore(int lengthOfSnake, Vector2 position)
    {
        _currentScore +=  lengthOfSnake*100;
        GameUI.Instance.UpdateScoreText(_currentScore);
    }

    public void GameOver()
    {
        OnGameOver?.Invoke(_currentScore);
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
