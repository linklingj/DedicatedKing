using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.LightTransport;

public class MiniMapCamera : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    #region inputValue
    public Camera minimapCamera;      // 미니맵용 카메라
    public Camera mainCamera;         // 실제 메인 카메라
    public RectTransform minimapRect; // RawImage의 RectTransform
    public Vector2 cameraWorldMin;    // 카메라가 비추는 월드 최소 좌표 (좌하단)
    public Vector2 cameraWorldMax;    // 카메라가 비추는 월드 최대 좌표 (우상단)
    public Vector2 miniMapworldMin;   // 미니맵이 표시하는 월드 최소 좌표 (좌하단)
    public Vector2 miniMapworldMax;   // 미니맵이 표시하는 월드 최대 좌표 (우상단)
    public Vector2 viewportMin;       // 미니맵 viewport 최소 좌표 (좌하단)
    public Vector2 viewportMax;       // 미니맵이 viewport 최대 좌표 (우상단)
    public Transform mainCameraTransform; // 메인 카메라 Transform
    public RectTransform viewportTransform;   // 시야 표시 Transform;
    #endregion
    PointerEventData pointerEventData;
    public bool activate = true; // 정지하려면 false로 바꾸셈

    void Start()
    {
        //------카메라 초기 위치------
        float worldX = 0f;
        float worldZ = 0f;
        float height = mainCameraTransform.position.y; // 메인 카메라 높이
        float angle = 90 - mainCameraTransform.eulerAngles.x; // 땅과 수직인 직선과 메인 카메라가 보는 방향 사이 각도
        //Debug.Log(height);
        //Debug.Log(angle);
        //Debug.Log(height / Mathf.Tan(angle));
        worldZ = worldZ - height / Mathf.Tan(angle); // 탄젠트 계산으로 Z축 보정

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
        if (!activate) { return; } // 기능 안쓸 때 리턴

        Vector2 localCursor; // 클릭한 지점의 내부의 상대 좌표를 담는 변수
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return; // 클릭한 곳이 미니맵 위라면 내부의 좌표계로 변환 해줌, 실패하면 리턴

        // viewport 위치 조정
        RectTransform parentRect = viewportTransform.parent as RectTransform; //부모의 RectTransform
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPos))
        {
            // viewport 범위 제한
            localPos.x = Mathf.Clamp(localPos.x, viewportMin.x, viewportMax.x);
            localPos.y = Mathf.Clamp(localPos.y, viewportMin.y, viewportMax.y);
            viewportTransform.anchoredPosition = localPos;
        }

        // 중심이 (0,0)인 좌표를 중심이 (0.5,0.5)인 좌표로 바꿔 줌
        Vector2 normalized = new Vector2(
            (localCursor.x / minimapRect.rect.width) + 0.5f,
            (localCursor.y / minimapRect.rect.height) + 0.5f
        );

        // 정규화 좌표를 월드 좌표로 변환
        float worldX = Mathf.Lerp(miniMapworldMin.x, miniMapworldMax.x, normalized.x); // X 월드 좌표
        float worldZ = Mathf.Lerp(miniMapworldMin.y, miniMapworldMax.y, normalized.y); // Z 월드 좌표
        Debug.Log("CLickPosition : (" + worldX + ", " + worldZ + ")");

        // X,Z좌표 최대 최소 보정
        worldX = Mathf.Clamp(worldX, cameraWorldMin.x, cameraWorldMax.x);
        worldZ = Mathf.Clamp(worldZ, cameraWorldMin.y, cameraWorldMax.y);
        /*if(worldX > cameraWorldMax.x) worldX = cameraWorldMax.x;
        else if(worldX < cameraWorldMin.x) worldX = cameraWorldMin.x;
        if(worldZ > cameraWorldMax.y) worldZ = cameraWorldMax.y;
        else if(worldZ < cameraWorldMin.y) worldZ = cameraWorldMin.y;*/
        Debug.Log("ChangePosition : (" + worldX + ", " + worldZ + ")");

        // 카메라 각에 따른 Z 좌표 보정
        float height = mainCameraTransform.position.y; // 메인 카메라 높이
        float angle = 90 - mainCameraTransform.eulerAngles.x; // 땅과 수직인 직선과 메인 카메라가 보는 방향 사이 각도
        //Debug.Log(height);
        //Debug.Log(angle);
        //Debug.Log(height / Mathf.Tan(angle));
        worldZ = worldZ - height / Mathf.Tan(angle); // 탄젠트 계산으로 Z축 보정

        // 메인 카메라 이동
        Vector3 newCameraPos = new Vector3(worldX, mainCameraTransform.position.y, worldZ);
        mainCamera.transform.position = newCameraPos;
    }
}
