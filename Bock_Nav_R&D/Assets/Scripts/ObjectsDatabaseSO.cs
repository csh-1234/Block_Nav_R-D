using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectsDatabaseSO : ScriptableObject
{
    public List<ObjectData> objectsData;
}

[Serializable]
public class ObjectData
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public List<Vector2Int> OccupiedCells { get; private set; } = new List<Vector2Int>() { Vector2Int.zero };
    [field: SerializeField]
    public GameObject Prefab { get; private set; }

    public List<Vector2Int> GetRotatedCells(int rotationIndex)
    {
        List<Vector2Int> rotatedCells = new List<Vector2Int>();
        foreach (var cell in OccupiedCells)
        {
            // 시계방향 90도 회전 (rotationIndex 횟수만큼)
            var rotated = cell;
            for (int i = 0; i < rotationIndex; i++)
            {
                rotated = new Vector2Int(rotated.y, -rotated.x);
            }
            rotatedCells.Add(rotated);
        }
        return rotatedCells;
    }
}