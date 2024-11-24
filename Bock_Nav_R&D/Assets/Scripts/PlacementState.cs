using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;
    private int currentRotation = 0;
    Grid grid;
    PreviewSystem preview;
    ObjectsDatabaseSO database;
    GridData BlockData;
    GridData TowerData;
    ObjectPlacer objectPlacer;
    InputManager inputManager;
    AGrid aGrid;

    public PlacementState(int ID, Grid grid, PreviewSystem preview, ObjectsDatabaseSO database, GridData blockData,
            GridData towerData, ObjectPlacer objectPlacer, InputManager inputManager, AGrid aGrid)
    {
        this.grid = grid;
        this.preview = preview;
        this.database = database;
        this.BlockData = blockData;
        this.TowerData = towerData;
        this.objectPlacer = objectPlacer;
        this.inputManager = inputManager;
        this.aGrid = aGrid;

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1)
        {
            preview.StartShowingPlacementPreview(
                database.objectsData[selectedObjectIndex].Prefab,
                database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation),
                currentRotation);
        }

        inputManager.OnRotate += RotateStructure;
    }

    private void RotateStructure()
    {
        if (!inputManager.IsPointerOverUI())
        {
            currentRotation = (currentRotation + 1) % 4;
            preview.UpdateRotation(currentRotation);
        }
    }

    public void EndState()
    {
        preview.StopShowingPreview();
        inputManager.OnRotate -= RotateStructure;
    }

    public void OnAction(Vector3Int gridPosition)
    {
        int floor = 0;
        bool canPlace = false;

        if (database.IsTower(database.objectsData[selectedObjectIndex].ID))
        {
            Vector3Int positionBelow = new Vector3Int(gridPosition.x, 0, gridPosition.z);
            
            if (BlockData.GetRepresentationIndex(positionBelow) != -1)
            {
                floor = 1;
                canPlace = TowerData.CanPlaceObjectAt(gridPosition, 
                    database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), floor);
            }
        }
        else
        {
            canPlace = BlockData.CanPlaceObjectAt(gridPosition, 
                database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), floor) &&
                TowerData.CanPlaceObjectAt(gridPosition, 
                database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), floor);
        }

        if (canPlace)
        {
            GridData selectedData = database.IsBlock(database.objectsData[selectedObjectIndex].ID) ? BlockData : TowerData;
             
            Vector3 position = grid.CellToWorld(gridPosition);
            position += new Vector3(0.5f, floor, 0.5f);

            int index = objectPlacer.PlaceObject(
                database.objectsData[selectedObjectIndex].Prefab,
                position,
                Quaternion.Euler(0, 90 * currentRotation, 0));

            selectedData.AddObjectAt(
                gridPosition,
                database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation),
                database.objectsData[selectedObjectIndex].ID,
                index,
                floor);

            if (database.IsBlock(database.objectsData[selectedObjectIndex].ID))
            {
                aGrid.UpdateGrid();
            }
        }

        if (PathManager.Instance != null && PathManager.Instance.HasBothPoints())
        {
            PathManager.Instance.UpdatePath();
        }
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        int floor = 0;
        bool validity = false;
        
        if (database.IsTower(database.objectsData[selectedObjectIndex].ID))
        {
            Vector3Int positionBelow = new Vector3Int(gridPosition.x, 0, gridPosition.z);
            if (BlockData.GetRepresentationIndex(positionBelow) != -1)
            {
                floor = 1;
                validity = TowerData.CanPlaceObjectAt(gridPosition, 
                    database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), floor);
            }
        }
        else
        {
            validity = BlockData.CanPlaceObjectAt(gridPosition, 
                database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), floor) &&
                TowerData.CanPlaceObjectAt(gridPosition, 
                database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation), floor);
        }

        preview.UpdatePosition(grid.CellToWorld(gridPosition), validity, floor);
    }
}