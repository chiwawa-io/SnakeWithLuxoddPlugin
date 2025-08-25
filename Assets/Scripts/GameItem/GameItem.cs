using UnityEngine;

[CreateAssetMenu(fileName = "New GameItem", menuName = "Snake/GameItem")]
public class GameItem : ScriptableObject
{
    [Header("Basic Data")]
    public string objName;
    public GameObject prefab;
    
    [Header("Collection Effect")]
    public bool isCollectible;
    [Tooltip("How many base points this item is worth.")]
    public int scoreValue;

    [Header("Obstacle Effect")]
    public bool isObstacle;
    [Tooltip("Does this obstacle kill the player instantly?")]
    public bool isInstaKill;
    
    [Header("Power-Up Effect")]
    public bool isPowerUp;
    public PowerUpEffectType effectType; 
    public float effectDuration;

    [Header("Additional effects")] public bool isPortal;

}

public enum PowerUpEffectType
{
    None, 
    SpeedUp,
    SlowDown,
    Invulnerable,
}
