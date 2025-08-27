using System.Collections.Generic;
using UnityEngine;

public class HomingArrow : MonoBehaviour
{
    private Transform _target;
    private float _speed;
    private float _turnRateDegPerSec;
    private float _lifeTime;
    private float _hitRadius;
    private float _damage;
    private LayerMask _enemyLayer;
    private string _enemyTag;

    private float _age;
    private Vector3 _lastPos;

    // 관통/재타격 제어
    private bool _penetrate = false;
    private float _perTargetHitCooldown = 0.25f;
    private readonly Dictionary<Collider, float> _lastHitTime = new Dictionary<Collider, float>(32);

    // 재타겟팅(여러 마리 타격) 설정
    [SerializeField] private float _seekRadius = 12f;       // 주변 탐색 반경
    [SerializeField] private float _forwardConeDeg = 120f;  // 전방 콘 각도
    [SerializeField] private float _retargetInterval = 0.1f;// 재탐색 주기
    private float _seekTimer = 0f;

    public void Init(Transform target,
                     float speed,
                     float turnRateDegPerSec,
                     float lifeTime,
                     float hitRadius,
                     float damage,
                     LayerMask enemyLayer,
                     string enemyTag)
    {
        _target = target;
        _speed = speed;
        _turnRateDegPerSec = turnRateDegPerSec;
        _lifeTime = lifeTime;
        _hitRadius = hitRadius;
        _damage = damage;
        _enemyLayer = enemyLayer;
        _enemyTag = enemyTag;

        _age = 0f;
        _lastPos = transform.position;

        // 초기 정렬
        Vector3 dir = AimDirection();
        if (dir.sqrMagnitude > 1e-6f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    // 외부에서 관통 모드/재타격 쿨다운 설정
    public void SetPenetration(bool penetrate, float perTargetCooldownSec = 0.25f)
    {
        _penetrate = penetrate;
        _perTargetHitCooldown = Mathf.Max(0f, perTargetCooldownSec);
    }

    // 재탐색 파라미터(필요시 스킬에서 조정)
    public void ConfigureSeek(float seekRadius, float forwardConeDeg = 120f, float retargetInterval = 0.1f)
    {
        _seekRadius = Mathf.Max(0f, seekRadius);
        _forwardConeDeg = Mathf.Clamp(forwardConeDeg, 0f, 180f);
        _retargetInterval = Mathf.Max(0.02f, retargetInterval);
    }

    private void Update()
    {
        _age += Time.deltaTime;
        if (_age >= _lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // 필요 시 재타겟팅(타겟이 없거나 잃었을 때)
        _seekTimer += Time.deltaTime;
        if ((_target == null) && _seekTimer >= _retargetInterval)
        {
            _seekTimer = 0f;
            TryFindNewTarget();
        }

        // 회전(유도)
        Vector3 dir = AimDirection();
        if (dir.sqrMagnitude > 1e-6f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnRateDegPerSec * Time.deltaTime);
        }

        // 다음 위치
        Vector3 nextPos = transform.position + transform.forward * (_speed * Time.deltaTime);

        // 경로 스윕: 여러 적 동시 히트 지원
        Vector3 moveDir = nextPos - transform.position;
        float dist = moveDir.magnitude;
        if (dist > 0f)
        {
            moveDir /= dist;

            var hits = Physics.SphereCastAll(transform.position, _hitRadius, moveDir, dist, _enemyLayer, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                var col = h.collider;
                if (col == null || !col.gameObject.activeInHierarchy) continue;
                if (!string.IsNullOrEmpty(_enemyTag) && !col.CompareTag(_enemyTag)) continue;

                // per-target 재타격 쿨다운
                if (_lastHitTime.TryGetValue(col, out float lastTime) && (Time.time - lastTime) < _perTargetHitCooldown)
                    continue;

                ApplyDamage(col);
                _lastHitTime[col] = Time.time;

                // 방금 맞춘 대상이면 잠시 타겟을 풀어 다른 적을 노리도록 유도
                var rootTr = col.attachedRigidbody ? col.attachedRigidbody.transform : col.transform;
                if (_penetrate && _target != null && rootTr == _target)
                {
                    _target = null;       // 타겟 해제 → 다음 프레임에 재탐색
                    _seekTimer = _retargetInterval; // 즉시 재탐색 트리거
                }

                if (!_penetrate)
                {
                    transform.position = h.point;
                    Destroy(gameObject);
                    return;
                }
            }
        }

        transform.position = nextPos;
        _lastPos = transform.position;
    }

    private Vector3 AimDirection()
    {
        if (_target == null) return transform.forward; // 타겟 없음 → 직진
        return (_target.position - transform.position).normalized;
    }

    private void ApplyDamage(Collider col)
    {
        // 현재 프로젝트 방식 유지: TestEnemy.TakeDamage 호출
        if (col.TryGetComponent(out TestEnemy enemy))
        {
            enemy.TakeDamage(_damage);
        }
    }

    // 주변에서 새로운 타겟 탐색(전방 콘 우선)
    private void TryFindNewTarget()
    {
        var candidates = Physics.OverlapSphere(transform.position, _seekRadius, _enemyLayer, QueryTriggerInteraction.Ignore);
        if (candidates == null || candidates.Length == 0) return;

        float bestScore = float.NegativeInfinity;
        Transform best = null;

        float cosHalf = Mathf.Cos((_forwardConeDeg * 0.5f) * Mathf.Deg2Rad);

        foreach (var c in candidates)
        {
            if (c == null) continue;
            if (!string.IsNullOrEmpty(_enemyTag) && !c.CompareTag(_enemyTag)) continue;

            var tr = c.attachedRigidbody ? c.attachedRigidbody.transform : c.transform;
            if (tr == null || !tr.gameObject.activeInHierarchy) continue;

            // 최근에 때렸다면 스킵(재타격 쿨다운)
            if (_lastHitTime.TryGetValue(c, out float lastTime) && (Time.time - lastTime) < _perTargetHitCooldown)
                continue;

            Vector3 to = (tr.position - transform.position);
            float dist = to.magnitude;
            if (dist <= 0.0001f) continue;

            Vector3 ndir = to / dist;
            float dirDot = Vector3.Dot(transform.forward, ndir);

            // 전방 콘 내 우선(콘 밖은 낮은 점수)
            if (dirDot < cosHalf) dirDot -= 1.0f; // 콘 밖이면 페널티

            // 점수: 방향 정렬과 거리 반비례를 조합
            float score = dirDot - dist * 0.02f; // 필요시 가중치 조정

            if (score > bestScore)
            {
                bestScore = score;
                best = tr;
            }
        }

        if (best != null)
            _target = best;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _hitRadius);
    }
#endif
}
