using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform target;
    private Vector3[] path;
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
            UpdatePath();
    }

    public void UpdatePath()
    {
        if (target == null) return;
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            if (PathManager.Instance != null)
            {
                PathManager.Instance.OnPathCalculated(newPath, true);
            }
            DrawPath();
        }
        else
        {
            path = null;
            if (PathManager.Instance != null)
            {
                PathManager.Instance.OnPathCalculated(null, false);
            }
            lineRenderer.positionCount = 0;
        }
    }

    void DrawPath()
    {
        if (path == null || path.Length == 0) return;

        Vector3[] points = new Vector3[path.Length + 2];
        points[0] = transform.position;
        for (int i = 0; i < path.Length; i++)
        {
            points[i + 1] = path[i];
        }
        points[points.Length - 1] = target.position;

        float pathHeight = transform.position.y;
        for (int i = 0; i < points.Length; i++)
        {
            points[i].y = pathHeight;
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}
