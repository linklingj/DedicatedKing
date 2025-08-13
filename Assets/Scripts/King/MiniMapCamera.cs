using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.LightTransport;

public class MiniMapCamera : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    #region inputValue
    public Camera minimapCamera;      // �̴ϸʿ� ī�޶�
    public Camera mainCamera;         // ���� ���� ī�޶�
    public RectTransform minimapRect; // RawImage�� RectTransform
    public Vector2 cameraWorldMin;    // ī�޶� ���ߴ� ���� �ּ� ��ǥ (���ϴ�)
    public Vector2 cameraWorldMax;    // ī�޶� ���ߴ� ���� �ִ� ��ǥ (����)
    public Vector2 miniMapworldMin;   // �̴ϸ��� ǥ���ϴ� ���� �ּ� ��ǥ (���ϴ�)
    public Vector2 miniMapworldMax;   // �̴ϸ��� ǥ���ϴ� ���� �ִ� ��ǥ (����)
    public Vector2 viewportMin;       // �̴ϸ� viewport �ּ� ��ǥ (���ϴ�)
    public Vector2 viewportMax;       // �̴ϸ��� viewport �ִ� ��ǥ (����)
    public Transform mainCameraTransform; // ���� ī�޶� Transform
    public RectTransform viewportTransform;   // �þ� ǥ�� Transform;
    #endregion
    PointerEventData pointerEventData;
    public bool activate = true; // �����Ϸ��� false�� �ٲټ�

    void Start()
    {
        //------ī�޶� �ʱ� ��ġ------
        float worldX = 0f;
        float worldZ = 0f;
        float height = mainCameraTransform.position.y; // ���� ī�޶� ����
        float angle = 90 - mainCameraTransform.eulerAngles.x; // ���� ������ ������ ���� ī�޶� ���� ���� ���� ����
        //Debug.Log(height);
        //Debug.Log(angle);
        //Debug.Log(height / Mathf.Tan(angle));
        worldZ = worldZ - height / Mathf.Tan(angle); // ź��Ʈ ������� Z�� ����

        // ���� ī�޶� �̵�
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
        if (!activate) { return; } // ��� �Ⱦ� �� ����

        Vector2 localCursor; // Ŭ���� ������ ������ ��� ��ǥ�� ��� ����
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return; // Ŭ���� ���� �̴ϸ� ����� ������ ��ǥ��� ��ȯ ����, �����ϸ� ����

        // viewport ��ġ ����
        RectTransform parentRect = viewportTransform.parent as RectTransform; //�θ��� RectTransform
        Vector2 localPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out localPos))
        {
            // viewport ���� ����
            localPos.x = Mathf.Clamp(localPos.x, viewportMin.x, viewportMax.x);
            localPos.y = Mathf.Clamp(localPos.y, viewportMin.y, viewportMax.y);
            viewportTransform.anchoredPosition = localPos;
        }

        // �߽��� (0,0)�� ��ǥ�� �߽��� (0.5,0.5)�� ��ǥ�� �ٲ� ��
        Vector2 normalized = new Vector2(
            (localCursor.x / minimapRect.rect.width) + 0.5f,
            (localCursor.y / minimapRect.rect.height) + 0.5f
        );

        // ����ȭ ��ǥ�� ���� ��ǥ�� ��ȯ
        float worldX = Mathf.Lerp(miniMapworldMin.x, miniMapworldMax.x, normalized.x); // X ���� ��ǥ
        float worldZ = Mathf.Lerp(miniMapworldMin.y, miniMapworldMax.y, normalized.y); // Z ���� ��ǥ
        Debug.Log("CLickPosition : (" + worldX + ", " + worldZ + ")");

        // X,Z��ǥ �ִ� �ּ� ����
        worldX = Mathf.Clamp(worldX, cameraWorldMin.x, cameraWorldMax.x);
        worldZ = Mathf.Clamp(worldZ, cameraWorldMin.y, cameraWorldMax.y);
        /*if(worldX > cameraWorldMax.x) worldX = cameraWorldMax.x;
        else if(worldX < cameraWorldMin.x) worldX = cameraWorldMin.x;
        if(worldZ > cameraWorldMax.y) worldZ = cameraWorldMax.y;
        else if(worldZ < cameraWorldMin.y) worldZ = cameraWorldMin.y;*/
        Debug.Log("ChangePosition : (" + worldX + ", " + worldZ + ")");

        // ī�޶� ���� ���� Z ��ǥ ����
        float height = mainCameraTransform.position.y; // ���� ī�޶� ����
        float angle = 90 - mainCameraTransform.eulerAngles.x; // ���� ������ ������ ���� ī�޶� ���� ���� ���� ����
        //Debug.Log(height);
        //Debug.Log(angle);
        //Debug.Log(height / Mathf.Tan(angle));
        worldZ = worldZ - height / Mathf.Tan(angle); // ź��Ʈ ������� Z�� ����

        // ���� ī�޶� �̵�
        Vector3 newCameraPos = new Vector3(worldX, mainCameraTransform.position.y, worldZ);
        mainCamera.transform.position = newCameraPos;
    }
}
