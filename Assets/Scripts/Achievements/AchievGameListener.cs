using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievGameListener : MonoBehaviour
{
    [SerializeField] private List<AchievementSO> achievementsList = new();
    
    public static Action<AchievementSO> OnAchievementCompleted;
    private void OnEnable()
    {
        Player.OnAchieved += AchievementComplete;
    }

    private void OnDisable()
    {
        Player.OnAchieved += AchievementComplete;
    }
    
    void AchievementComplete(string id)
    {
        PlayerDataManager.Instance.CompleteAchievement(id);

        var achievementData = GetAchievmentById(id);
        
        if (achievementData != null) OnAchievementCompleted?.Invoke(achievementData);
    }

    AchievementSO GetAchievmentById(string id)
    {
        foreach (var achievementData in achievementsList)
        {
            if (achievementData.id == id) return achievementData;
            else Debug.LogWarning("Not found the Achievement");
        }
        return null;
    }
}
