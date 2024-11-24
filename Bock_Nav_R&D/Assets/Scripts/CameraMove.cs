using Pathfinding.RVO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public GameObject parent;
    Vector3 defPosition;
    Quaternion defRotation;
    float defZoom;
    public float moveSpeed = 5f;  // 이동 속도 조절 변수 추가

    void Start()
    {
        // 기본 위치 저장
        defPosition = transform.position;
        defRotation = parent.transform.rotation;
        defZoom = Camera.main.fieldOfView;
    }
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 moveDirection = new Vector3(-Input.GetAxis("Mouse X"), 0, -Input.GetAxis("Mouse Y"));
            parent.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Camera.main.fieldOfView += (20 * Input.GetAxis("Mouse ScrollWheel"));
        }
        if (Camera.main.fieldOfView < 10)
            Camera.main.fieldOfView = 10;
        else if (Camera.main.fieldOfView > 100)
            Camera.main.fieldOfView = 100;
    }
   
}
