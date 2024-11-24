using UnityEngine;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }

    [SerializeField]
    private Transform spawnPoint;  // 몬스터 스폰 지점
    [SerializeField]
    private Transform targetPoint; // 목표 지점 (중심부)
    [SerializeField]
    private Unit pathUnit;        // 시작 지점에 있는 Unit 참조
    
    public Vector3[] CurrentPath { get; private set; }
    public bool HasValidPath { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 시작 시 경로 생성
        if (spawnPoint != null && targetPoint != null)
        {
            if (pathUnit == null)
            {
                // Start 오브젝트에서 Unit 컴포넌트 찾기
                pathUnit = spawnPoint.GetComponent<Unit>();
            }
            
            if (pathUnit != null)
            {
                pathUnit.SetTarget(targetPoint);
                UpdatePath();
            }
        }
    }

    public void UpdatePath()
    {
        if (spawnPoint != null && targetPoint != null && pathUnit != null)
        {
            pathUnit.transform.position = spawnPoint.position;
            pathUnit.UpdatePath();
        }
    }

    // 몬스터가 사용할 수 있는 현재 경로 가져오기
    public Vector3[] GetCurrentPath()
    {
        return CurrentPath;
    }

    // Unit 스크립트에서 호출할 콜백
    public void OnPathCalculated(Vector3[] path, bool success)
    {
        CurrentPath = path;
        HasValidPath = success;
    }

    public bool HasBothPoints()
    {
        return spawnPoint != null && targetPoint != null;
    }

    // 몬스터 스폰 위치 가져오기
    public Vector3 GetSpawnPosition()
    {
        return spawnPoint ? spawnPoint.position : Vector3.zero;
    }

    // 목표 위치 가져오기
    public Vector3 GetTargetPosition()
    {
        return targetPoint ? targetPoint.position : Vector3.zero;
    }
} 