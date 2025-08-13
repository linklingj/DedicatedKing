using UnityEngine;
using UnityEngine.InputSystem;

public class HeroMovement : MonoBehaviour
{
    private Camera cam; // 용사 카메라
    private Vector3 targetPos; // 목적지

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] HeroState heroState;

    InputAction rightClickAction;

    void Awake()
    {
        rightClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");
        rightClickAction.Enable();
    }

    void Start()
    {
        cam = heroState.cam;
        targetPos = heroState.transform.position;
    }

    void Update() // ← 입력은 여기서!
    {
        if (rightClickAction.triggered || Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                targetPos = hit.point;
                heroState.isMoving = true;
            }
        }
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

    void OnDisable() => rightClickAction.Disable();
}
