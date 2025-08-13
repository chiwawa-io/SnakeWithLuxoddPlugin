using TMPro;
using UnityEngine;

public class AchievementsRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI achievementName;
    [SerializeField] private TextMeshProUGUI achievementDescription;
    [SerializeField] private TextMeshProUGUI achievementIsCompleted;
    public void Setup(AchievementSO achievementData, bool isCompleted)
    {
        achievementName.text = achievementData.displayName;
        achievementDescription.text = achievementData.description;
        achievementIsCompleted.text = isCompleted ? "Completed" : "Not Completed";
    }
}
