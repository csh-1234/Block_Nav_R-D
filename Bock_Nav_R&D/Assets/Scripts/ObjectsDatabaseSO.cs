using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectsDatabaseSO : ScriptableObject
{
    public List<ObjectData> objectsData;

    // ID 범위 상수 정의
    public const int BLOCK_ID_START = 0;
    public const int BLOCK_ID_END = 5;    // 블럭 ID: 0~5
    public const int TOWER_ID_START = 6;
    public const int TOWER_ID_END = 10;    // 포탑 ID: 6~10

    public bool IsBlock(int ID)
    {
        return ID >= BLOCK_ID_START && ID <= BLOCK_ID_END;
    }

    public bool IsTower(int ID)
    {
        return ID >= TOWER_ID_START && ID <= TOWER_ID_END;
    }
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