using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridData
{
    Dictionary<Vector3Int, PlacemnetData> placedObjects = new();
    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int placeObjectIndex)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        PlacemnetData data = new PlacemnetData(positionToOccupy, ID, placeObjectIndex);
        foreach (var pos in positionToOccupy)
        {
            if(placedObjects.ContainsKey(pos))
            {
                throw new Exception($"Dictionary already contains this cell position {pos}"); 
            }
            placedObjects[pos] = data;

        }
    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnValue = new();
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                returnValue.Add(gridPosition + new Vector3Int(x, 0, y));
            }
        }
        return returnValue;
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                return false;
        }
        return true;
    }

}


public class PlacemnetData
{
    public List<Vector3Int> occupiedPositions;
    public int Id { get; private set; }
    public int placedObjectIndex { get; private set; }

    public PlacemnetData(List<Vector3Int> occupiedPositions, int id, int placedObjectIndex)
    {
        this.occupiedPositions = occupiedPositions;
        Id = id;
        this.placedObjectIndex = placedObjectIndex;
    }
}
