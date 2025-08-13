using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance {get; private set;}
    
    [Header("Life UI")]
    [SerializeField] private Slider lifeSlider;
    
    [Header("Game Time Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI addedScoreText;
    
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private TextMeshProUGUI yourScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI timer;    
    
    [Header("Error Panel")]
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorCodeText;
    [SerializeField] private TextMeshProUGUI errorMessageText;
    [SerializeField] private GameObject errorPanelButton;
    
    private bool _isGameOver;

    void OnEnable()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
        
        GameManager.OnGameOver += GameOver;
        GameManager.OnError += OnError;
        Player.UpdateScore += AddedScoreNumber;
        Player.EventMessages += PrintMessage;
        Player.UpdateLife += UpdateLife;
        InactivityDetector.UpdateTimers += UpdateTimer;
    }
    
    void OnDisable()
    {
        GameManager.OnGameOver -= GameOver;
        GameManager.OnError -= OnError;
        Player.UpdateScore -= AddedScoreNumber;
        Player.EventMessages -= PrintMessage;
        Player.UpdateLife -= UpdateLife;
        InactivityDetector.UpdateTimers -= UpdateTimer;
    }

    public void UpdateScoreText(int score)
    {
        scoreText.text = score.ToString("D10");
    }

    private void UpdateLife(int lives)
    {
        lifeSlider.value = lives;
    }
    
    void AddedScoreNumber(int bodyLength, Vector2 position)
    {
        addedScoreText.gameObject.SetActive(true);
        addedScoreText.gameObject.transform.position = position;
        
        if (bodyLength>0)
            addedScoreText.text = "+" + (bodyLength*100);
        else
            addedScoreText.text = "" + (bodyLength*100);
        StartCoroutine(WaitAndTurnOff(addedScoreText.gameObject));
    }
    void PrintMessage(string message, Vector2 position)
    {
        addedScoreText.gameObject.SetActive(true);
        addedScoreText.gameObject.transform.position = position;
        addedScoreText.text = message;
        StartCoroutine(WaitAndTurnOff(addedScoreText.gameObject));
    }
    
    void GameOver(int score)
    {
        gameOverText.SetActive(true);
        StartCoroutine(WaitAndTurnOff(gameOverText, 2, true, score));
        
        _isGameOver = true;
    }

    void OnError(int code, string message)
    {
        errorPanel.SetActive(true);
        errorPanelButton.GetComponent<Button>().Select();
        errorCodeText.text = code.ToString();
        errorMessageText.text = message;
    }

    void UpdateTimer(int timeLeft)
    {
      if (_isGameOver) timer.text = timeLeft.ToString();
    }

    IEnumerator WaitAndTurnOff(GameObject obj, float time = 0.3f, bool reset = false, int score = 0)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);

        if (reset)
        {
            gameOverPanel.SetActive(true);
            restartButton.GetComponent<Button>().Select();
            yourScoreText.text = score.ToString("D10");
            bestScoreText.text = PlayerDataManager.Instance.BestScore.ToString("D10");
        }
    }
}
