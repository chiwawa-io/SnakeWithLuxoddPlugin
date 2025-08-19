using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI levelNumber;
    
    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerNumber;
    
    [Header("Error Panel")]
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorCodeText;
    [SerializeField] private TextMeshProUGUI errorMessageText;
    [SerializeField] private GameObject errorPanelButton;
    
    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClick;
    
    private int _errorCode;
    private string _errorMessage;
    
    private void OnEnable()
    {
        PlayerDataManager.Instance.OnError += OnError;
        InactivityDetector.ForceStart += StartGame;
        InactivityDetector.UpdateTimers += UpdateTimer;
        
        double v = PlayerDataManager.Instance.CurrentXp * 0.001;
        
        xpSlider.SetValueWithoutNotify((float)v);
        levelNumber.text = PlayerDataManager.Instance.CurrentLevel.ToString();
    }

    private void OnDisable()
    {
        PlayerDataManager.Instance.OnError -= OnError;
        InactivityDetector.ForceStart -= StartGame;
        InactivityDetector.UpdateTimers -= UpdateTimer;
    }
    
    private void OnError(int code, string message)
    {
        _errorCode = code;
        _errorMessage = message;
        
        errorPanel.SetActive(true);
        errorPanelButton.GetComponent<Button>().Select();
        errorCodeText.text = code.ToString();
        errorMessageText.text = message;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
        audioSource.PlayOneShot(buttonClick);
    }

    public void UpdateTimer(int timeLeft)
    {
        timerNumber.text = timeLeft.ToString();
    }

    public void OpenLeaderboard()
    {
        SceneManager.LoadScene("Leaderboard");
        audioSource.PlayOneShot(buttonClick);
    }

    public void OpenAchievements()
    {
        SceneManager.LoadScene("Achievements");
        audioSource.PlayOneShot(buttonClick);
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene("Loading");
        audioSource.PlayOneShot(buttonClick);
    }
    
    public void Quit(bool isQuitWithError = false)
    {
        NetworkManager.Instance.HealthStatusCheckService.Deactivate();
        NetworkManager.Instance.WebSocketService.CloseConnection();
        if (!isQuitWithError) NetworkManager.Instance.WebSocketService.BackToSystem();
        else NetworkManager.Instance.WebSocketService.BackToSystemWithError(_errorMessage, _errorCode.ToString());
        Application.Quit();
        audioSource.PlayOneShot(buttonClick);
    }
}
