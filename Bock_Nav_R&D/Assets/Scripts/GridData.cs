using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridData
{
    Dictionary<Vector3Int, PlacementData> placedObjects = new();

    public void AddObjectAt(Vector3Int gridPosition, List<Vector2Int> occupiedCells, int ID, int placedObjectIndex, int floor = 0)
    {
        gridPosition.y = floor;
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, occupiedCells);
        PlacementData data = new PlacementData(positionToOccupy, ID, placedObjectIndex);

        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos)) 
                throw new Exception($"Dictionary already contains this cell position {pos}");
            placedObjects[pos] = data;
        }

        foreach (var vec in placedObjects.Keys)
        {
            Debug.Log($" : {vec.x},{vec.y},{vec.z}");
        }
    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, List<Vector2Int> occupiedCells)
    {
        List<Vector3Int> returnVal = new();
        foreach (var cell in occupiedCells)
        {
            returnVal.Add(new Vector3Int(
                gridPosition.x + cell.x, 
                gridPosition.y,  // 층수 유지
                gridPosition.z + cell.y));
        }
        return returnVal;
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, List<Vector2Int> occupiedCells, int floor = 0)
    {
        gridPosition.y = floor;
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, occupiedCells);
        
        // 2층인 경우, 아래 층에 블럭이 있는지 확인
        if (floor > 0)
        {
            foreach (var pos in positionToOccupy)
            {
                // 현재 위치에 이미 오브젝트가 있는지 확인
                if (placedObjects.ContainsKey(pos))
                    return false;
            }
            return true;  // 2층이고 현재 위치가 비어있으면 true 반환
        }

        // 1층인 경우 현재 위치가 비어있는지만 확인
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                return false;
        }
        return true;
    }

    public int GetRepresentationIndex(Vector3Int gridPosition)
    {
        if (placedObjects.ContainsKey(gridPosition) == false)
            return -1;
        return placedObjects[gridPosition].PlacedObjectIndex;
    }

    public void RemoveObjectAt(Vector3Int gridPosition)
    {
        foreach (var pos in placedObjects[gridPosition].occupiedPositions)
        {
            placedObjects.Remove(pos);
        }
    }
}

public class PlacementData
{
    public List<Vector3Int> occupiedPositions;
    public int ID { get; private set; }
    public int PlacedObjectIndex { get; private set; }

    public PlacementData(List<Vector3Int> occupiedPositions, int iD, int placedObjectIndex)
    {
        this.occupiedPositions = occupiedPositions;
        ID = iD;
        PlacedObjectIndex = placedObjectIndex;
    }
}