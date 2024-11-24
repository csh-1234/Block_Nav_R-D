using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AGrid : MonoBehaviour
{

    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float ANodeRadius;
    ANode[,] grid;

    float ANodeDiameter;
    int gridSizeX, gridSizeY;

    void Awake()
    {
        ANodeDiameter = ANodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / ANodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / ANodeDiameter);
        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    public void UpdateGrid()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new ANode[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        worldBottomLeft.y = transform.position.y;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * ANodeDiameter + ANodeRadius) + Vector3.forward * (y * ANodeDiameter + ANodeRadius);
                worldPoint.y = transform.position.y;
                bool walkable = !(Physics.CheckSphere(worldPoint, ANodeRadius, unwalkableMask));
                grid[x, y] = new ANode(walkable, worldPoint, x, y);
            }
        }
    }

    public List<ANode> GetNeighbours(ANode ANode)
    {
        List<ANode> neighbours = new List<ANode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = ANode.gridX + x;
                int checkY = ANode.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }


    public ANode ANodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        if (grid != null && displayGridGizmos)
        {
            foreach (ANode n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (ANodeDiameter - .1f));
            }
        }
    }
}