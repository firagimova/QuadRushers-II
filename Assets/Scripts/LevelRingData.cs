using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelRings", menuName = "QuadRushers/Level Ring Data", order = 2)]
public class LevelRingData : ScriptableObject
{
    [Tooltip("List of predetermined ring positions for this level")]
    public List<Vector3> ringPositions = new List<Vector3>();

    [Tooltip("Optional rotation for each ring (use same index as position)")]
    public List<Vector3> ringRotations = new List<Vector3>();

    public int RingCount => ringPositions.Count;

    public Vector3 GetPosition(int index)
    {
        if (index >= 0 && index < ringPositions.Count)
        {
            return ringPositions[index];
        }
        return Vector3.zero;
    }

    public Quaternion GetRotation(int index)
    {
        if (index >= 0 && index < ringRotations.Count)
        {
            return Quaternion.Euler(ringRotations[index]);
        }
        return Quaternion.identity;
    }
}
