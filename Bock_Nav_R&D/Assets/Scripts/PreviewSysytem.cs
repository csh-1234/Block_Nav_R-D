using UnityEngine;
using System.Collections.Generic;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField]
    private float previewYOffset = 0.06f;

    [SerializeField]
    private GameObject cellIndicator;
    private GameObject previewObject;

    [SerializeField]
    private Material previewMaterialPrefab;
    private Material previewMaterialInstance;

    private Renderer cellIndicatorRenderer;

    private GameObject cursorParent;

    private List<Vector2Int> currentOccupiedCells;
    private int currentRotation = 0;

    private void Start()
    {
        previewMaterialInstance = new Material(previewMaterialPrefab);
        cellIndicator.SetActive(false);
        cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
    }

    public void StartShowingPlacementPreview(GameObject prefab, List<Vector2Int> occupiedCells, int rotationIndex)
    {
        currentOccupiedCells = new List<Vector2Int>(occupiedCells);
        currentRotation = rotationIndex;
        previewObject = Instantiate(prefab);
        PreparePreview(previewObject);
        PrepareCursor(currentOccupiedCells);
        cellIndicator.SetActive(true);
        UpdateRotation(currentRotation);
    }

    private void PrepareCursor(List<Vector2Int> occupiedCells)
    {
        if (occupiedCells.Count == 0)
            return;

        // 부모 오브젝트 생성 (기존 cellIndicator는 이 아래로 들어갈 것임)
        if (cursorParent == null)
        {
            cursorParent = new GameObject("Cursor Parent");
            cursorParent.transform.SetParent(transform);
        }

        // 기존 커서 자식들 제거
        foreach (Transform child in cursorParent.transform)
        {
            Destroy(child.gameObject);
        }

        // 각 점유 셀마다 인디케이터 생성
        foreach (var cell in occupiedCells)
        {
            GameObject newCell = Instantiate(cellIndicator, cursorParent.transform);
            newCell.transform.localPosition = new Vector3(cell.x, 0, cell.y);
            newCell.transform.localScale = Vector3.one; // 각 셀은 1x1 크기
            newCell.SetActive(true);
        }
    }

    private void PreparePreview(GameObject previewObject)
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = previewMaterialInstance;
            }
            renderer.materials = materials;
        }
    }

    public void StopShowingPreview()
    {
        if (cursorParent != null)
        {
            foreach (Transform child in cursorParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (previewObject != null)
            Destroy(previewObject);
    }

    public void UpdatePosition(Vector3 position, bool validity)
    {
        if (previewObject != null)
        {
            MovePreview(position);
            ApplyFeedbackToPreview(validity);
        }

        MoveCursor(position);
        ApplyFeedbackToCursor(validity);
    }

    private void ApplyFeedbackToPreview(bool validity)
    {
        Color c = validity ? Color.white : Color.red;
        c.a = 0.5f;
        previewMaterialInstance.color = c;
    }

    private void ApplyFeedbackToCursor(bool validity)
    {
        Color c = validity ? Color.white : Color.red;
        c.a = 0.5f;
        cellIndicatorRenderer.material.color = c;
    }

    private void MoveCursor(Vector3 position)
    {
        if (cursorParent != null)
            cursorParent.transform.position = position;
    }

    private void MovePreview(Vector3 position)
    {
        previewObject.transform.position = new Vector3(
            position.x + 0.5f,
            position.y + previewYOffset,
            position.z + 0.5f);
    }

    internal void StartShowingRemovePreview()
    {
        cellIndicator.SetActive(true);
        PrepareCursor(new List<Vector2Int>() { Vector2Int.zero });
        ApplyFeedbackToCursor(false);
    }

    private List<Vector2Int> GetCurrentOccupiedCells()
    {
        if (currentOccupiedCells == null)
            return new List<Vector2Int>() { Vector2Int.zero };

        // 현재 회전을 적용한 셀 반환
        List<Vector2Int> rotatedCells = new List<Vector2Int>();
        foreach (var cell in currentOccupiedCells)
        {
            var rotated = cell;
            for (int i = 0; i < currentRotation; i++)
            {
                rotated = new Vector2Int(rotated.y, -rotated.x);
            }
            rotatedCells.Add(rotated);
        }
        return rotatedCells;
    }

    public void UpdateRotation(int rotationIndex)
    {
        currentRotation = rotationIndex;
        if (previewObject != null)
        {
            previewObject.transform.rotation = Quaternion.Euler(0, 90 * currentRotation, 0);
        }

        // 회전된 셀 위치 계산
        List<Vector2Int> rotatedCells = new List<Vector2Int>();
        foreach (var cell in currentOccupiedCells)
        {
            var rotated = cell;
            for (int i = 0; i < currentRotation; i++)
            {
                rotated = new Vector2Int(rotated.y, -rotated.x);
            }
            rotatedCells.Add(rotated);
        }

        // 커서 업데이트
        if (cursorParent != null)
        {
            foreach (Transform child in cursorParent.transform)
            {
                Destroy(child.gameObject);
            }
            
            var currentPosition = cursorParent.transform.position;
            PrepareCursor(rotatedCells);
            cursorParent.transform.position = currentPosition;
        }
    }
}