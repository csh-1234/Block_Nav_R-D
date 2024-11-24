using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PathFinding : MonoBehaviour
{

    PathRequestManager requestManager;
    AGrid grid;

    void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<AGrid>();
    }


    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        ANode startANode = grid.ANodeFromWorldPoint(startPos);
        ANode targetANode = grid.ANodeFromWorldPoint(targetPos);


        if (startANode.walkable && targetANode.walkable)
        {
            Heap<ANode> openSet = new Heap<ANode>(grid.MaxSize);
            HashSet<ANode> closedSet = new HashSet<ANode>();
            openSet.Add(startANode);

            while (openSet.Count > 0)
            {
                ANode currentANode = openSet.RemoveFirst();
                closedSet.Add(currentANode);

                if (currentANode == targetANode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (ANode neighbour in grid.GetNeighbours(currentANode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentANode.gCost + GetDistance(currentANode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetANode);
                        neighbour.parent = currentANode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess)
        {
            waypoints = RetracePath(startANode, targetANode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);

    }

    Vector3[] RetracePath(ANode startANode, ANode endANode)
    {
        List<ANode> path = new List<ANode>();
        ANode currentANode = endANode;

        while (currentANode != startANode)
        {
            path.Add(currentANode);
            currentANode = currentANode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;

    }

    Vector3[] SimplifyPath(List<ANode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    int GetDistance(ANode ANodeA, ANode ANodeB)
    {
        int dstX = Mathf.Abs(ANodeA.gridX - ANodeB.gridX);
        int dstY = Mathf.Abs(ANodeA.gridY - ANodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }


}