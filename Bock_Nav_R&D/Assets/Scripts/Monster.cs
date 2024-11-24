using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField]
    private float speed = 2f;
    private Vector3[] path;
    private int currentWaypointIndex;

    void Start()
    {
        if (PathManager.Instance != null && PathManager.Instance.HasValidPath)
        {
            path = PathManager.Instance.GetCurrentPath();
            Vector3 startPos = PathManager.Instance.GetSpawnPosition();
            startPos.y = transform.position.y;
            transform.position = startPos;
            currentWaypointIndex = 0;
        }
    }

    void Update()
    {
        if (path == null || currentWaypointIndex >= path.Length) return;

        // 현재 웨이포인트로 이동
        Vector3 targetPosition = path[currentWaypointIndex];
        targetPosition.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (transform.position == targetPosition)
        {
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= path.Length)
            {
                Vector3 finalTarget = PathManager.Instance.GetTargetPosition();
                finalTarget.y = transform.position.y;
                transform.position = Vector3.MoveTowards(transform.position, finalTarget, speed * Time.deltaTime);
                
                if (transform.position == finalTarget)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
} 