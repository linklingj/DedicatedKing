using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum State { Normal, Chase, PreAttack, Attacking, PostAttack, Retreat, ReturnToPath, Cooldown }

    [Header("Normal Option")]
    public WaypointPath path;           // 소속 경로
    public AttackPattern attack;        // 공격 패턴
    public Transform modelRoot;         // 회전용(선택)

    [Header("Detection")]
    public float detectRadius = 8f;     // 플레이어 감지 반경
    public LayerMask playerMask;        // 플레이어 레이어

    [Header("Movement")]
    public float normalSpeed = 2.5f;
    public float chaseSpeed = 3.5f;
    public float turnSpeed = 12f;

    [Header("Other")]
    public bool loopPath = true;
    public float groundY = 0f;

    // 내부 상태
    [SerializeField] State state;
    Rigidbody rb;

    // 🔸 여러 플레이어 대응: 현재 타깃(가장 가까운 HeroMain)
    HeroMain currentTarget;

    int nodeIndex;
    Vector3 externalVelocity;
    float externalDrag = 8f;
    float stateTimer;
    float cooldownTimer;
    Vector3 leashOrigin;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        state = State.Normal;
        nodeIndex = 0;
        leashOrigin = NearestNodeOnPath();
    }

    void Update()
    {
        // 외부 힘 감쇠
        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, externalDrag * Time.deltaTime);
        
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
        
        // 현재 타깃 유효성 점검 및 필요 시 재선정
        MaintainOrRetarget();

        switch (state)
        {
            case State.Normal:       TickNormal(); break;
            case State.Chase:        TickChase(); break;
            case State.PreAttack:    TickPreAttack(); break;
            case State.Attacking:    break; // 코루틴에서 처리
            case State.PostAttack:   TickPostAttack(); break;
            case State.Retreat:      TickRetreat(); break;
            case State.ReturnToPath: TickReturnToPath(); break;
            case State.Cooldown:     TickCooldown(); break;
        }
    }

    // ===== 상태별 =====
    void TickNormal()
    {
        if (!path || path.Count == 0) return;

        // 플레이어 감지 -> Chase
        if (TryDetectAnyHero())
        {
            state = State.Chase;
            return;
        }

        // 순찰
        Vector3 target = path.GetNode(nodeIndex);
        MoveTowards(target, normalSpeed);

        if (Vector3.Distance(Flat(rb.position), Flat(target)) <= path.arriveThreshold)
        {
            nodeIndex++;
            if (nodeIndex >= path.Count)
                nodeIndex = loopPath ? 0 : path.Count - 1;
        }
    }

    void TickChase()
    {
        if (currentTarget == null)
        {
            state = State.ReturnToPath;
            return;
        }

        float distToPlayer = DistXZ(rb.position, currentTarget.transform.position);

        // 🔧 먼저 사거리 체크 → 사거리 안이면 이동 금지(브레이크) + 공격 준비
        if (distToPlayer <= attack.range)
        {
            // 이동 멈추고 바라보기만
            FaceTarget(currentTarget.transform.position);

            if (cooldownTimer <= 0f)
            {
                state = State.PreAttack;
                stateTimer = attack.preDelay; // 전 딜레이
            }
            return; // ★ 여기서 반환: 이 프레임에는 MoveTowards 호출 안 함
        }

        // 사거리 밖이면 그때 이동
        MoveTowards(currentTarget.transform.position, chaseSpeed);

        // 감지 범위 크게 벗어나면 복귀
        if (distToPlayer > detectRadius * 1.5f)
        {
            state = State.ReturnToPath;
        }
    }


    void TickPreAttack()
    {
        // 공격 전 딜레이: 이동 금지(외부 힘만 적용)
        if (currentTarget != null)
            FaceTarget(currentTarget.transform.position);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            StartCoroutine(DoAttackRoutine());
        }
    }

    IEnumerator DoAttackRoutine()
    {
        state = State.Attacking;

        // 실제 타격 타이밍(애니메이션 이벤트로 분리 가능)
        yield return null; // 한 프레임 뒤 타격. 필요 시 WaitForSeconds(shaftTime) 등으로 조정

        Vector3 center = rb.position + transform.forward * (attack.range - attack.hitRadius * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, attack.hitRadius, attack.hitMask);

        foreach (var col in hits)
        {
            // 플레이어 컴포넌트 찾기 (부모에 있을 수도 있어 GetComponentInParent 권장)
            HeroMain hero = col.GetComponentInParent<HeroMain>();
            if (hero != null)
            {
                hero.TakeDamage(attack.damage);
            }
        }

        // 후딜
        state = State.PostAttack;
        stateTimer = attack.postDelay;
    }

    void TickPostAttack()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f) return;

        cooldownTimer = attack.cooldown;

        if (currentTarget != null &&
            DistXZ(rb.position, currentTarget.transform.position) <= attack.retreatCheckDistance)
        {
            state = State.Retreat;
            stateTimer = attack.retreatDuration;
        }
        else
        {
            state = State.Chase;
        }
    }

    void TickRetreat()
    {
        if (currentTarget == null) { state = State.ReturnToPath; return; }

        Vector3 dir = (Flat(rb.position) - Flat(currentTarget.transform.position)).normalized;
        MoveInDirection(dir, normalSpeed);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            state = State.Chase;
        }
    }

    void TickReturnToPath()
    {
        if (!path || path.Count == 0) { state = State.Normal; return; }

        Vector3 nearest = NearestNodeOnPath();
        MoveTowards(nearest, normalSpeed);

        if (Vector3.Distance(Flat(rb.position), Flat(nearest)) <= path.arriveThreshold)
        {
            nodeIndex = NearestNodeIndex();
            leashOrigin = path.GetNode(nodeIndex);
            state = State.Normal;
        }

        // 복귀 중에도 근처에 플레이어가 있으면 다시 추격
        if (currentTarget != null && DistXZ(rb.position, currentTarget.transform.position) <= detectRadius)
        {
            state = State.Chase;
        }
    }

    void TickCooldown()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            state = (currentTarget != null) ? State.Chase : State.ReturnToPath;
        }
    }

    // ===== 타깃 관리(여러 플레이어) =====

    // 현재 타깃이 없거나 무효라면 재선정
    void MaintainOrRetarget()
    {
        // 이미 타깃이 있고 살아있으면 유지
        if (IsTargetValid(currentTarget)) return;

        currentTarget = SelectNearestHeroWithin(detectRadius);

        // 타깃을 새로 잡았고 현재 상태가 Normal이면 바로 추격 시작
        if (currentTarget != null && state == State.Normal)
            state = State.Chase;
    }

    // 감지: 범위 내 아무 히어로라도 있으면 true
    bool TryDetectAnyHero()
    {
        if (currentTarget != null && IsTargetValid(currentTarget))
            return true;

        currentTarget = SelectNearestHeroWithin(detectRadius);
        return currentTarget != null;
    }

    // 현재 타깃 유효성: 존재 && 죽지 않음(필요 시 조건 추가)
    bool IsTargetValid(HeroMain hero)
    {
        if (hero == null) return false;
        return hero.isActiveAndEnabled;
    }

    // 주어진 반경 내에서 가장 가까운 HeroMain 선택 (HeroManager 사용 권장)
    HeroMain SelectNearestHeroWithin(float radius)
    {
        HeroMain best = null;
        float bestSqr = radius * radius + 0.0001f;
        Vector3 pos = rb.position;

        // 1) HeroManager가 있다면 그 리스트 기반
        if (HeroManager.Instance != null)
        {
            var list = HeroManager.Instance.GetAllHeroes();
            for (int i = 0; i < list.Count; i++)
            {
                var h = list[i];
                if (!IsTargetValid(h)) continue;
                float d2 = (Flat(h.transform.position) - Flat(pos)).sqrMagnitude;
                if (d2 <= bestSqr)
                {
                    bestSqr = d2;
                    best = h;
                }
            }
            return best;
        }
        return best;
    }

    // ===== 이동/유틸 =====

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 cur = rb.position;
        Vector3 to = Flat(target) - Flat(cur);
        Vector3 dir = to.sqrMagnitude > 0.0001f ? to.normalized : Vector3.zero;
        MoveInDirection(dir, speed);
        FaceTarget(cur + dir);
    }

    void MoveInDirection(Vector3 dir, float speed)
    {
        Vector3 desired = dir * speed;
        Vector3 delta = (desired + externalVelocity) * Time.deltaTime;
        rb.MovePosition(rb.position + delta);
    }

    void FaceTarget(Vector3 worldTarget)
    {
        Vector3 fwd = (Flat(worldTarget) - Flat(rb.position)).normalized;
        if (fwd.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(fwd, Vector3.up);
        if (modelRoot != null)
            modelRoot.rotation = Quaternion.Slerp(modelRoot.rotation, targetRot, turnSpeed * Time.deltaTime);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    // XZ 평면 거리 헬퍼
    float DistXZ(Vector3 a, Vector3 b) => Vector3.Distance(Flat(a), Flat(b));

    Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    Vector3 NearestNodeOnPath()
    {
        if (!path || path.Count == 0) return rb.position;
        int idx = NearestNodeIndex();
        return path.GetNode(idx);
    }

    int NearestNodeIndex()
    {
        int best = 0;
        float bestD = float.PositiveInfinity;
        for (int i = 0; i < path.Count; i++)
        {
            float d = Vector3.Distance(Flat(rb.position), Flat(path.GetNode(i)));
            if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        if (attack)
        {
            Gizmos.color = Color.red;
            Vector3 center = Application.isPlaying && rb != null
                ? rb.position + transform.forward * (attack.range - attack.hitRadius * 0.5f)
                : transform.position + transform.forward * (attack.range - attack.hitRadius * 0.5f);
            Gizmos.DrawWireSphere(center, attack.hitRadius);
        }
    }

    // 외부에서 호출: 넉백/경직 등
    public void ApplyExternalForce(Vector3 impulse)
    {
        externalVelocity += impulse;
    }
}
