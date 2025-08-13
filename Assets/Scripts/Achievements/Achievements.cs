using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Achievements : MonoBehaviour
{
    [Header("AchievementsDataBase")] 
    [SerializeField] private List<AchievementSO> achievementsList = new();
    
    [Header("AchievementsUI")]
    [SerializeField] private Transform achievementsUIParent;
    [SerializeField] private GameObject achievementsUIPrefab;
    private void Start()
    {
        LoadAchievements();
    }

    void LoadAchievements()
    {
        foreach (Transform child in achievementsUIParent)
        {
            Destroy(child.gameObject);
        }

        foreach (AchievementSO achievementData in achievementsList)
        {
            var achievementRow = Instantiate(achievementsUIPrefab, achievementsUIParent);

            bool isCompleted = PlayerDataManager.Instance.IsAchievementCompleted(achievementData.id);

            var rowUI = achievementRow.GetComponent<AchievementsRowUI>();
            rowUI.Setup(achievementData, isCompleted);
        }
    }

    public void ReturnToMenu ()
    {
        SceneManager.LoadScene("Menu");
    }
}
