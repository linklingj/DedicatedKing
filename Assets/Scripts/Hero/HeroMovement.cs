using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeroMovement : MonoBehaviour
{
    private Camera cam; // 용사 카메라
    private Vector3 targetPos; // 목적지

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] HeroState heroState;


    void Start()
    {
        cam = heroState.cam;
        targetPos = heroState.transform.position;
    }
    

    void FixedUpdate()
    {
        if (!heroState.isMoving) return;

        Vector3 dir = targetPos - heroState.transform.position;
        float distance = dir.magnitude;

        if (distance < 0.5f) // 목표 위치 근처
        {
            heroState.isMoving = false;
            heroState.rigid.linearVelocity = Vector3.zero;
            return;
        }

        dir.Normalize();
        heroState.rigid.linearVelocity = dir * moveSpeed;
    }


    private void OnMove()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            targetPos = hit.point;
            heroState.isMoving = true;
        }
    }
}
