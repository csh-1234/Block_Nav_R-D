using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ANode :IHeapItem<ANode> 
    {
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public ANode parent;
    int heapIndex;

    public float directionPreference;

    public ANode(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        directionPreference = 0;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public void CalculateDirectionPreference(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - worldPosition).normalized;
        
        if (parent != null)
        {
            Vector3 currentDirection = (worldPosition - parent.worldPosition).normalized;
            directionPreference = Vector3.Dot(currentDirection, directionToTarget);
        }
        else
        {
            directionPreference = 1;
        }
    }

    public int CompareTo(ANode nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = directionPreference.CompareTo(nodeToCompare.directionPreference);
            if (compare == 0)
            {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }
        }
        return -compare;
    }
}