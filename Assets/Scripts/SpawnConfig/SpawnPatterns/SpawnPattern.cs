using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct PatternItem
{
    public string itemName;
    public Vector2Int relativePosition;
}

[CreateAssetMenu(fileName = "New Spawn Pattern", menuName = "Snake/Spawn Pattern")]
public class SpawnPattern : ScriptableObject
{
    public List<PatternItem> items = new List<PatternItem>();
}