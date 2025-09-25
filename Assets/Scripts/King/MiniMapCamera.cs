using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.LightTransport;

public class MiniMapCamera : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    #region inputValue
    public Camera minimapCamera;      // 미니맵 카메라
    public Camera mainCamera;         // 메인 화면 카메라
    public RectTransform minimapRect; // RawImage의 RectTransform
    public Vector2 cameraWorldMin;    // 카메라가 움직일 수 있는 최소 좌표 (왼쪽 아래)
    public Vector2 cameraWorldMax;    // 카메라가 움직일 수 있는 최대 좌표 (오른쪽 위)
    public Vector2 miniMapworldMin;   // 미니맵이 표현하는 월드 최소 좌표 (왼쪽 아래)
    public Vector2 miniMapworldMax;   // 미니맵이 표현하는 월드 최대 좌표 (오른쪽 위)
    public Vector2 viewportMin;       // 미니맵 viewport 최소 좌표 (왼쪽 아래)
    public Vector2 viewportMax;       // 미니맵 viewport 최대 좌표 (오른쪽 위)
    public Transform mainCameraTransform; // 메인 카메라 Transform
    public RectTransform viewportTransform;   // 뷰포트 표시 Transform
    #endregion

    PointerEventData pointerEventData;
    public bool activate = true; // 비활성화하려면 false로 변경

    void Start()
    {
        // ------ 카메라 초기 위치 ------
        float worldX = 0f;
        float worldZ = 0f;
        float height = mainCameraTransform.position.y; // 메인 카메라 높이
        float angle = 90 - mainCameraTransform.eulerAngles.x; // 메인 카메라의 X축 회전 각도 기반 계산

        worldZ = worldZ - height / Mathf.Tan(angle); // 투영 보정을 위한 Z값 계산

        // 메인 카메라 이동
        Vector3 newCameraPos = new Vector3(worldX, mainCameraTransform.position.y, worldZ);
        mainCamera.transform.position = newCameraPos;
        //--------------------------
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MouseOn(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MouseOn(eventData);
    }

    private void MouseOn(PointerEventData eventData)
    {
        if (!activate) { return; } // 비활성화 상태면 실행 안 함

        Vector2 localCursor; // 클릭한 지점의 로컬 좌표
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return; // 클릭 지점이 미니맵 영역이 아니면 리턴

        // viewport 위치 계산
        RectTransform parentRect = viewportTransform.parent as RectTransform; // 부모 RectTransform
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPos))
        {
            // viewport 이동 제한
            localPos.x = Mathf.Clamp(localPos.x, viewportMin.x, viewportMax.x);
            localPos.y = Mathf.Clamp(localPos.y, viewportMin.y, viewportMax.y);
            viewportTransform.anchoredPosition = localPos;
        }

        // 중심 (0,0) 좌표를 (0.5,0.5) 좌표로 변환
        Vector2 normalized = new Vector2(
            (localCursor.x / minimapRect.rect.width) + 0.5f,
            (localCursor.y / minimapRect.rect.height) + 0.5f
        );

        // 화면 좌표 → 월드 좌표 변환
        float worldX = Mathf.Lerp(miniMapworldMin.x, miniMapworldMax.x, normalized.x); // X 좌표
        float worldZ = Mathf.Lerp(miniMapworldMin.y, miniMapworldMax.y, normalized.y); // Z 좌표
        Debug.Log("ClickPosition : (" + worldX + ", " + worldZ + ")");

        // X, Z 좌표 최소/최대 제한
        worldX = Mathf.Clamp(worldX, cameraWorldMin.x, cameraWorldMax.x);
        worldZ = Mathf.Clamp(worldZ, cameraWorldMin.y, cameraWorldMax.y);
        Debug.Log("ChangePosition : (" + worldX + ", " + worldZ + ")");

        // 카메라 보정용 Z 좌표 계산
        float height = mainCameraTransform.position.y; // 메인 카메라 높이
        float angle = 90 - mainCameraTransform.eulerAngles.x; // 카메라 각도
        worldZ = worldZ - height / Mathf.Tan(angle); // 투영 보정 Z값

        // 메인 카메라 이동
        Vector3 newCameraPos = new Vector3(worldX, mainCameraTransform.position.y, worldZ);
        mainCamera.transform.position = newCameraPos;
    }
}
