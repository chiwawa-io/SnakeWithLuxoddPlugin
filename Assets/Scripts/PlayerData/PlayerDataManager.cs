using System;
using System.Collections.Generic;
using Luxodd.Game.Scripts.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    private static int _bestScore;
    private static int _currentXp;
    private static int _currentLevel;
    public int BestScore => _bestScore;
    public int CurrentXp => _currentXp;
    public int CurrentLevel => _currentLevel;
    
    private HashSet<string> _completedAchievementIds = new();

    public Action<int, string> OnError;

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
    public void LoadData()
    {
        NetworkManager.Instance.WebSocketCommandHandler.SendGetUserDataRequestCommand(OnDataLoadSuccess, OnDataLoadError);
    }

    public void CheckAndSaveScore(int score)
    {
        if (score > _bestScore)
        {
            _bestScore = score;
            
            SaveData();
        }
        else
        {
            SaveData();
        }
    }

    private void SaveData()
    {
        PlayerData newPlayerData = new PlayerData(_bestScore, _currentLevel, _currentXp, _completedAchievementIds);
            
        string json = JsonConvert.SerializeObject(newPlayerData);
            
        NetworkManager.Instance.WebSocketCommandHandler.SendSetUserDataRequestCommand(json, OnDataSaveSuccess, OnDataSaveError);    }

    public void AddExperience(int experience)
    {
        _currentXp += experience;
        if (_currentXp >= 1000)
        {
            _currentXp -= 1000;
            _currentLevel++;
        }
    }

    public bool IsAchievementCompleted(string achievementId)
    {
        return _completedAchievementIds.Contains(achievementId);
    }

    public void CompleteAchievement(string achievementId)
    {
        if (_completedAchievementIds.Add(achievementId))
        {
            SaveData();
        }
    }

    void OnDataSaveSuccess () {}

    void OnDataLoadSuccess(object response)
    {
        var userDataPayload = (UserDataPayload)response;
        var userDataRaw = userDataPayload.Data;
        var userDataObject = (JObject)userDataRaw;
        
        if (userDataObject == null)
        {
            _bestScore = 0;
            _currentXp = 0;
            _currentLevel = 0;
        }
        else
        {
            var loadedPlayerData = JsonConvert.DeserializeObject<PlayerData>(userDataObject["user_data"]?.ToString() ?? string.Empty);
            _bestScore = loadedPlayerData.BestScore;
            _currentLevel =  loadedPlayerData.Level;
            _currentXp = loadedPlayerData.Xp;
            _completedAchievementIds = loadedPlayerData.CompletedAchievementIds ?? new HashSet<string>();
        }
    }

    void OnDataSaveError(int code, string message)
    {
        OnError?.Invoke(code, message);
    }

    void OnDataLoadError(int code, string message)
    {
        _bestScore = 0;
        OnError?.Invoke(code, message);
    }
}
