using Newtonsoft.Json;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    private static int _bestScore;
    
    public int BestScore => _bestScore;

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        LoadData();
    }

    void LoadData()
    {
        NetworkManager.Instance.WebSocketCommandHandler.SendGetUserDataRequestCommand(OnDataLoadSuccess, OnDataLoadError);
    }

    public void CheckAndSaveScore(int score)
    {
        if (score > _bestScore)
        {
            _bestScore = score;
            
            PlayerData newPlayerData = new PlayerData(score);
            
            string json = JsonConvert.SerializeObject(newPlayerData);
            
            NetworkManager.Instance.WebSocketCommandHandler.SendSetUserDataRequestCommand(json, OnDataSaveSuccess, OnDataSaveError);
        }
    }
    
    void OnDataSaveSuccess () {}
    
    void OnDataLoadSuccess (object data) {}
    
    void OnDataSaveError (int code, string message) {}
    
    void OnDataLoadError (int code, string message) {}
}
