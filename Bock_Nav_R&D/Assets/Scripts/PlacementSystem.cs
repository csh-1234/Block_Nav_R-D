using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private GameObject mouseIndicator, cellindicator;
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private ObjectsDatabaseSO database;
    private int selectedObjectIndex = - 1;
    [SerializeField]
    private GameObject gridVisualization;
    [SerializeField]
    private AudioSource source;

    private GridData floorData, furnitureData;

    private Renderer previewRenderer;

    private List<GameObject> placedGameObject = new();

    private void Start()
    {
        stopPlacement();
        floorData = new();
        furnitureData = new();
        previewRenderer = cellindicator.GetComponentInChildren<Renderer>();
    }

    public void StartPlacement(int ID)
    {
        //so에서 해당 id를 가진 오브젝트 탐색 후 없으면 -1 리턴
        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if(selectedObjectIndex < 0)
        {
            Debug.LogError($"no id found{ID}");
            return;
        }
        gridVisualization.SetActive(true);
        cellindicator.SetActive(true);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += stopPlacement;
    }

    private void PlaceStructure()
    {
        if(inputManager.IsPointerOverUI())
        {
            return;
        }
        Vector3 mouseposition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mouseposition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (placementValidity == false)
            return;
        //mouseIndicator.transform.position = mouseposition;
        GameObject newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab); 
        newObject.transform.position = grid.CellToWorld(gridPosition);

        placedGameObject.Add(newObject);
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;
        selectedData.AddObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size, database.objectsData[selectedObjectIndex].ID, placedGameObject.Count - 1);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;
        return selectedData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    }
    private void stopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        cellindicator.SetActive(false);
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= stopPlacement; 
    }

    private void Update()
    {
        if (selectedObjectIndex < 0)
            return;
        Vector3 mouseposition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mouseposition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        previewRenderer.material.color = placementValidity ? Color.white : Color.red;

        mouseIndicator.transform.position = mouseposition;
        cellindicator.transform.position = grid.CellToWorld(gridPosition);
    }

}
