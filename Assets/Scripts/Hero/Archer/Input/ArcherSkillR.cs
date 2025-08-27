using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArcherSkillR : MonoBehaviour, ISkill
{
    [Header("호밍 화살 프리팹(프로젝타일)")]
    [SerializeField] private GameObject _homingArrowPrefab;

    [Header("시전/발사 설정")]
    [SerializeField] private float _duration = 4f;         // 전체 지속시간
    [SerializeField] private float _fireInterval = 0.12f;  // 틱 간격
    [SerializeField] private int _arrowsPerTick = 3;       // 틱당 발사 수
    [SerializeField] private float _maxTargetsPerTick = 5; // 후보 최대 수(가까운 순)

    [Header("타겟팅")]
    [SerializeField] private float _searchRadius = 12f;      // 탐색 반경
    [SerializeField] private LayerMask _enemyLayer = ~0;     // 적 레이어
    [SerializeField] private string _enemyTag = "Enemy";     // 적 태그(선택)
    [SerializeField] private bool _requireLineOfSight = false; // 시야 체크(옵션)
    [SerializeField] private LayerMask _losBlockers = ~0;      // 시야 차단 레이어

    [Header("발사 원점")]
    [SerializeField] private Transform _muzzle;              // 없으면 플레이어 위치 사용
    [SerializeField] private Vector3 _muzzleOffset = new Vector3(0f, 1.2f, 0f); // 기본 오프셋

    [Header("데미지/프로젝타일 공유 파라미터")]
    [SerializeField] private float _damage = 50f;            // 화살 1발 데미지(예시)
    [SerializeField] private float _arrowSpeed = 22f;        // m/s
    [SerializeField] private float _turnRateDegPerSec = 720f;// 회전 속도
    [SerializeField] private float _arrowLifeTime = 3.5f;    // 최대 생존 시간
    [SerializeField] private float _hitRadius = 0.25f;       // 히트 반경(구체)

    private Coroutine _routine;

    public void Press(GameObject player, Transform mouseTransform)
    {
        // 범위 프리뷰 등 필요 시 구현
    }

    public void Release(GameObject player, Transform mouseTransform)
    {
        if (_homingArrowPrefab == null || player == null) return;

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }
        _routine = StartCoroutine(FireRoutine(player));
    }

    private IEnumerator FireRoutine(GameObject player)
    {
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            // 1) 후보 수집: 가까운 순으로 상한(_maxTargetsPerTick)만큼
            var targets = FindTargets(player.transform.position, _searchRadius, (int)_maxTargetsPerTick);

            // 2) 틱당 발사 수만큼 대상 선택(후보가 부족하면 중복 허용)
            for (int i = 0; i < _arrowsPerTick; i++)
            {
                Transform target = PickTarget(targets, i);
                Vector3 spawnPos = (_muzzle != null ? _muzzle.position : player.transform.position) + _muzzleOffset;

                var go = Instantiate(_homingArrowPrefab, spawnPos, Quaternion.identity);
                var homing = go.GetComponent<HomingArrow>();
                if (homing != null)
                {
                    homing.Init(target,
                                _arrowSpeed,
                                _turnRateDegPerSec,
                                Mathf.Max(_arrowLifeTime, _duration),
                                _hitRadius,
                                _damage,
                                _enemyLayer,
                                _enemyTag);
                    homing.SetPenetration(true, 0.25f);        // 관통 on, 같은 적 재타격 쿨다운
                    homing.ConfigureSeek(_searchRadius, 120f, 0.1f); // 주변 재탐색 설정
                }
            }

            // 대기
            float t = 0f;
            while (t < _fireInterval)
            {
                t += Time.deltaTime;
                elapsed += Time.deltaTime;
                if (elapsed >= _duration) break;
                yield return null;
            }
        }

        _routine = null;
    }

    private List<Transform> FindTargets(Vector3 origin, float radius, int maxCount)
    {
        var list = new List<(Transform t, float d2)>();
        var cols = Physics.OverlapSphere(origin, radius, _enemyLayer, QueryTriggerInteraction.Ignore);

        foreach (var c in cols)
        {
            if (c == null) continue;
            if (!string.IsNullOrEmpty(_enemyTag) && !c.CompareTag(_enemyTag)) continue;

            var tr = c.attachedRigidbody ? c.attachedRigidbody.transform : c.transform;
            if (tr == null || !tr.gameObject.activeInHierarchy) continue;

            // 시야 체크(옵션)
            if (_requireLineOfSight)
            {
                Vector3 dir = (tr.position - origin);
                float dist = dir.magnitude;
                if (Physics.Raycast(origin + Vector3.up * 1.2f, dir.normalized, out var hit, dist, _losBlockers, QueryTriggerInteraction.Ignore))
                    continue;
            }

            float d2 = (tr.position - origin).sqrMagnitude;
            list.Add((tr, d2));
        }

        list.Sort((a, b) => a.d2.CompareTo(b.d2));

        var result = new List<Transform>(Mathf.Min(maxCount, list.Count));
        for (int i = 0; i < Mathf.Min(maxCount, list.Count); i++)
            result.Add(list[i].t);
        return result;
    }

    private Transform PickTarget(List<Transform> candidates, int index)
    {
        if (candidates == null || candidates.Count == 0) return null;
        // 후보가 _arrowsPerTick보다 적으면 모듈로 중복 선택
        return candidates[index % candidates.Count];
    }
}
