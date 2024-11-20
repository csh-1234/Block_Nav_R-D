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
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;
    InputManager inputManager;

    public PlacementState(int ID,
                         Grid grid,
                         PreviewSystem preview,
                         ObjectsDatabaseSO database,
                         GridData floorData,
                         GridData furnitureData,
                         ObjectPlacer objectPlacer,
                         InputManager inputManager)
    {
        this.grid = grid;
        this.preview = preview;
        this.database = database;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        this.inputManager = inputManager;

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
        bool canPlace = floorData.CanPlaceObjectAt(gridPosition, 
            database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation)) &&
            furnitureData.CanPlaceObjectAt(gridPosition, 
            database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation));

        if (canPlace)
        {
            GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
                floorData :
                furnitureData;

            Vector3 position = grid.CellToWorld(gridPosition);
            position += new Vector3(0.5f, 0, 0.5f);

            int index = objectPlacer.PlaceObject(
                database.objectsData[selectedObjectIndex].Prefab,
                position,
                Quaternion.Euler(0, 90 * currentRotation, 0));

            selectedData.AddObjectAt(
                gridPosition,
                database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation),
                database.objectsData[selectedObjectIndex].ID,
                index);
        }
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool validity = floorData.CanPlaceObjectAt(gridPosition, 
            database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation)) &&
            furnitureData.CanPlaceObjectAt(gridPosition, 
            database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation));

        preview.UpdatePosition(grid.CellToWorld(gridPosition), validity);
    }
}