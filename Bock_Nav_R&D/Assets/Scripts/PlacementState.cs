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

    private bool CheckPathValidity(Vector3Int gridPosition, List<Vector2Int> cells)
    {
        if (!PathManager.Instance.HasValidPath)
            return false;

        // 시작점과 목표점 위치 가져오기
        Vector3 spawnPos = PathManager.Instance.GetSpawnPosition();
        Vector3 targetPos = PathManager.Instance.GetTargetPosition();
        
        // 블록이 시작점이나 목표점을 가리는지 확인
        foreach (var cell in cells)
        {
            Vector3Int blockPos = new Vector3Int(
                gridPosition.x + cell.x,
                gridPosition.y,
                gridPosition.z + cell.y
            );
            Vector3 worldPos = grid.CellToWorld(blockPos) + new Vector3(0.5f, 0, 0.5f);
            
            if (Vector3.Distance(worldPos, spawnPos) < 0.1f || 
                Vector3.Distance(worldPos, targetPos) < 0.1f)
            {
                return false;
            }
        }

        // 임시로 노드들을 막고 새로운 경로가 있는지 확인
        Dictionary<ANode, bool> originalStates = new Dictionary<ANode, bool>();
        
        foreach (var cell in cells)
        {
            Vector3Int blockPos = new Vector3Int(
                gridPosition.x + cell.x,
                gridPosition.y,
                gridPosition.z + cell.y
            );
            ANode node = aGrid.ANodeFromWorldPoint(new Vector3(blockPos.x, 0, blockPos.z));
            if (node != null && !originalStates.ContainsKey(node))
            {
                originalStates[node] = node.walkable;
                node.walkable = false;
            }
        }

        // 새로운 경로가 있는지 확인
        PathManager.Instance.UpdatePath();
        bool isValid = PathManager.Instance.HasValidPath;

        // 노드 상태 복원
        foreach (var pair in originalStates)
        {
            pair.Key.walkable = pair.Value;
        }
        PathManager.Instance.UpdatePath();

        return isValid;  // 새로운 경로가 있으면 설치 가능
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

            if (validity && PathManager.Instance != null && PathManager.Instance.HasBothPoints())
            {
                validity = CheckPathValidity(gridPosition, 
                    database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation));
            }
        }

        preview.UpdatePosition(grid.CellToWorld(gridPosition), validity, floor);
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

            if (canPlace && PathManager.Instance != null && PathManager.Instance.HasBothPoints())
            {
                canPlace = CheckPathValidity(gridPosition, 
                    database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation));
            }
        }

        if (canPlace)
        {
            // 설치 직전에 한번 더 경로 체크
            if (database.IsBlock(database.objectsData[selectedObjectIndex].ID) &&
                PathManager.Instance != null && PathManager.Instance.HasBothPoints())
            {
                canPlace = CheckPathValidity(gridPosition, 
                    database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation));
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
                    var cells = database.objectsData[selectedObjectIndex].GetRotatedCells(currentRotation);
                    foreach (var cell in cells)
                    {
                        Vector3Int blockPos = new Vector3Int(
                            gridPosition.x + cell.x,
                            gridPosition.y,
                            gridPosition.z + cell.y
                        );
                        aGrid.UpdateNode(blockPos, true);
                    }
                    PathManager.Instance.UpdatePath();
                }
            }
        }
    }
}