using UnityEngine;

[CreateAssetMenu(fileName = "New GameItem", menuName = "Snake/Skin Item")]
public class SkinSO : ScriptableObject
{
    [Header("Basic Data")]
    public string skinName;
    public Sprite head;
    public Sprite skinSprite;
    public Sprite skinBg;
    public Sprite inGameSprite;

    [Header("Level Unlock Criteria")] 
    public bool isUnlockedThroughLevel;
    public int level;
    
    [Header("Buy criteria")]
    public int costInShards;
}