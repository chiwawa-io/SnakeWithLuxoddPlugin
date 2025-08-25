using System.Collections.Generic;

public class PlayerData
{
    public int BestScore;
    public int Level;
    public int Xp;
    public string CurrentSkin;
    public int SkinShards;
    public HashSet<string> CompletedAchievementIds { get; set; } = new();
    public HashSet<string> OwnedSkins { get; set; } = new();

    public PlayerData(int score, int level, int xp,  HashSet<string> completedAchievementIds, HashSet<string> ownedSkins, string currentSkin, int skinShards)
    {
        BestScore = score;
        Level = level;
        Xp = xp;
        CompletedAchievementIds = completedAchievementIds ?? new();
        OwnedSkins = ownedSkins ?? new();
        CurrentSkin = currentSkin;
        SkinShards = skinShards;
    }
    
    public PlayerData() {}
}
