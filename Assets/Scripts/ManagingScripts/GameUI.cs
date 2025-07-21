using System.Collections;
using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance {get; private set;}
    
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI yourScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI addedScoreText;

    void OnEnable()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
        
        GameManager.OnGameOver += GameOver;
        Player.UpdateScore += AddedScoreNumber;
    }
    
    void OnDisable()
    {
        GameManager.OnGameOver -= GameOver;
        Player.UpdateScore -= AddedScoreNumber;
    }

    public void UpdateScoreText(int score)
    {
        scoreText.text = score.ToString("D10");
    }

    void AddedScoreNumber(int bodyLength, Vector2 position)
    {
        addedScoreText.gameObject.SetActive(true);
        addedScoreText.gameObject.transform.position = position;
        addedScoreText.text = "+" + ((bodyLength-1)*100);
        StartCoroutine(WaitAndTurnOff(addedScoreText.gameObject));
    }
    
    void GameOver(int score)
    {
        gameOverText.SetActive(true);
        StartCoroutine(WaitAndTurnOff(gameOverText, 2, true, score));
    }

    IEnumerator WaitAndTurnOff(GameObject obj, float time = 0.3f, bool reset = false, int score = 0)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);

        if (reset)
        {
            gameOverPanel.SetActive(true);
            yourScoreText.text = score.ToString("D10");
            bestScoreText.text = score.ToString("D10");
        }
    }
}
