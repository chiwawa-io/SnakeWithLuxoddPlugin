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
    private static int _skinShards;
    private static string _currentSkin;
    public int BestScore => _bestScore;
    public int CurrentXp => _currentXp;
    public int CurrentLevel => _currentLevel;
    
    private HashSet<string> _completedAchievementIds = new();
    private HashSet<string> _ownedSkinIds = new();

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
        PlayerData newPlayerData = new PlayerData(_bestScore, _currentLevel, _currentXp, _completedAchievementIds,  _ownedSkinIds, _currentSkin, _skinShards);
            
        string json = JsonConvert.SerializeObject(newPlayerData);
            
        NetworkManager.Instance.WebSocketCommandHandler.SendSetUserDataRequestCommand(json, OnDataSaveSuccess, OnDataSaveError);    
    }

    public void AddExperience(int experience)
    {
        _currentXp += experience;
        if (_currentXp >= 1000)
        {
            _currentXp -= 1000;
            _currentLevel++;
            
            if (_currentLevel>5) BuySkin("HellFire");
            if (_currentLevel > 10) BuySkin("AtomicBreak");
        }
    }

    public bool IsAchievementCompleted(string achievementId)
    {
        return _completedAchievementIds.Contains(achievementId);
    }
    public bool IsSkinOwned(string achievementId)
    {
        return _ownedSkinIds.Contains(achievementId);
    }

    public void CompleteAchievement(string achievementId)
    {
        if (_completedAchievementIds.Add(achievementId))
        {
            _skinShards += 5;
            SaveData();
        }
    }
    public void BuySkin(string id)
    {
        if (_ownedSkinIds.Add(id))
        {
            SaveData();
        }
    }

    public void SetSkin(string skin)
    {
        _currentSkin = skin;
        SaveData();
    }

    public string GetCurrentSkin() => _currentSkin;
    public int GetShardsNum() => _skinShards;
    public void Buy(int cost) => _skinShards -= cost;

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
            _currentSkin = "Default";
        }
        else
        {
            var loadedPlayerData = JsonConvert.DeserializeObject<PlayerData>(userDataObject["user_data"]?.ToString() ?? string.Empty);
            _bestScore = loadedPlayerData.BestScore;
            _currentLevel =  loadedPlayerData.Level;
            _currentXp = loadedPlayerData.Xp;
            _completedAchievementIds = loadedPlayerData.CompletedAchievementIds ?? new HashSet<string>();
            _ownedSkinIds = loadedPlayerData.OwnedSkins ?? new HashSet<string>();
            _ownedSkinIds.Add("Default");
            _currentSkin = loadedPlayerData.CurrentSkin ??  "Default";
            _skinShards = loadedPlayerData.SkinShards;
            
            if (_currentLevel>5) BuySkin("HellFire");
            if (_currentLevel > 10) BuySkin("AtomicBreak");
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
