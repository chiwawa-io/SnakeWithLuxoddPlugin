using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnPointList
{
    public string itemName;
    public List<Vector2Int> positions;
}

[CreateAssetMenu(fileName = "New Spawn Point Map", menuName = "Snake/Spawn Point Map")]
public class SpawnPointMap : ScriptableObject
{
    public List<SpawnPointList> spawnPoints;

    public List<Vector2Int> GetPointsFor(string itemName)
    {
        foreach (var list in spawnPoints)
        {
            if (list.itemName == itemName)
            {
                return list.positions;
            }
        }
        return null;
    }
}