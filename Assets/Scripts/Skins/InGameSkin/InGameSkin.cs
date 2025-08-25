using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New GameItem", menuName = "Snake/In-game Skin Data")]
public class InGameSkin : ScriptableObject
{
    [Header("Basic Data")]
    public string skinName;
    public List<Sprite> idleState = new();
}