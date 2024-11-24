using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class RemovingState : IBuildingState
{
    private int gameObjectIndex = -1;
    Grid grid;
    PreviewSystem previewSystem;
    GridData BlockData;
    GridData TowerData;
    ObjectPlacer objectPlacer;
    AGrid aGrid;

    public RemovingState(Grid grid, PreviewSystem previewSystem, GridData blockData, GridData towerData, ObjectPlacer objectPlacer, AGrid aGrid)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.BlockData = blockData;
        this.TowerData = towerData;
        this.objectPlacer = objectPlacer;
        this.aGrid = aGrid;
        previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        GridData selectedData = null;
        if (BlockData.GetRepresentationIndex(gridPosition) != -1)
        {
            selectedData = BlockData;
        }
        else if (TowerData.GetRepresentationIndex(gridPosition) != -1)
        {
            selectedData = TowerData;
        }

        if (selectedData != null)
        {
            PlacementData placementData = selectedData.GetPlacementData(gridPosition);
            if (placementData != null)
            {
                objectPlacer.RemoveObjectAt(placementData.PlacedObjectIndex);
                
                foreach (var pos in placementData.occupiedPositions)
                {
                    aGrid.UpdateNode(pos, false);
                }
                
                selectedData.RemoveObjectAt(gridPosition);
                
                if (PathManager.Instance != null)
                {
                    PathManager.Instance.UpdatePath();
                }
            }
        }
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)
    {
        return TowerData.GetRepresentationIndex(gridPosition) != -1 ||  BlockData.GetRepresentationIndex(gridPosition) != -1;
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool validity = CheckIfSelectionIsValid(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity);
    }

    public void OnAction(object gridpo)
    {
        throw new NotImplementedException();
    }
}