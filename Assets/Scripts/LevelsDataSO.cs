using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelsData", menuName = "QuadRushers/Levels Data", order = 1)]
public class LevelsDataSO : ScriptableObject
{
    public List<LevelData> levels = new List<LevelData>();

    public List<LevelData> Levels => levels;

    public int LevelCount => levels.Count;

    public LevelData GetLevel(int index)
    {
        if (index >= 0 && index < levels.Count)
        {
            return levels[index];
        }
        return null;
    }
}
