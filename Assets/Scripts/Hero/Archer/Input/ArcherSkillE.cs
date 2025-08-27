using UnityEngine;

public class ArcherSkillE : MonoBehaviour, ISkill
{
    [Header("E 스킬(슬로우 장판)")]
    [SerializeField] private GameObject slowFieldPrefab;
    [SerializeField] private float maxCastRange = 12f;      // 최대 시전 사거리
    [SerializeField] private float radius = 3.5f;           // 장판 반경
    [SerializeField] private float duration = 5f;           // 장판 지속시간
    [SerializeField] private float dps = 20f;               // 초당 피해량
    [SerializeField] private float slowPercent = 0.4f;      // 이동속도 감소 비율(0.4 = 40%)
    [SerializeField] private float tickInterval = 0.5f;     // 데미지/슬로우 틱 주기
    [SerializeField] private LayerMask groundLayer = ~0;    // 지면 레이어(필요시 설정)

    public void Press(GameObject player, Transform mouseTransform)
    {
        // 범위 프리뷰 등 필요 시 사용
    }

    // 스킬 발사(실제 사용) - 연타 시 매번 해당 땅 위치에 개별 생성
    public void Release(GameObject player, Transform mouseTransform)
    {
        if (slowFieldPrefab == null || player == null) return;

        Vector3 playerPos = player.transform.position;
        Vector3 targetPos = mouseTransform != null ? mouseTransform.position : playerPos;
        Vector3 toTarget = targetPos - playerPos;
        if (toTarget.magnitude > maxCastRange)
            targetPos = playerPos + toTarget.normalized * maxCastRange;

        // 지면 히트 정보로 높이/법선 정렬
        const float yOffset = 0.1f; // 지면에서 아주 살짝 띄워 깜빡임 방지
        if (Physics.Raycast(new Vector3(targetPos.x, targetPos.y + 50f, targetPos.z),
                            Vector3.down, out var hit, 200f, groundLayer, QueryTriggerInteraction.Ignore))
        {
            targetPos = hit.point + hit.normal * yOffset;

            var go = Instantiate(slowFieldPrefab, targetPos, Quaternion.identity);

            // 1) 지면 법선에 장판을 밀착 정렬(원형이면 up만 맞추면 OK)
            go.transform.up = hit.normal;

            // 2) 시각적 중첩 완화용 랜덤 Y 회전(선택)
            go.transform.Rotate(0f, Random.Range(0f, 360f), 0f, Space.Self);

            var field = go.GetComponent<SlowField>();
            if (field != null)
            {
                field.Init(radius, duration, dps, slowPercent, tickInterval);
            }
            return;
        }

        // 레이 실패 시 보정 생성
        var fallback = Instantiate(slowFieldPrefab, targetPos, Quaternion.identity);
        var fallbackField = fallback.GetComponent<SlowField>();
        if (fallbackField != null)
        {
            fallbackField.Init(radius, duration, dps, slowPercent, tickInterval);
        }
    }
}
