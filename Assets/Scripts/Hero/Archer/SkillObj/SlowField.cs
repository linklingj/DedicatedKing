using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 슬로우 장판: 범위 내 적에게 주기적으로 데미지 + 슬로우 적용
[RequireComponent(typeof(SphereCollider))]
public class SlowField : MonoBehaviour
{
    private float _radius = 3.5f;
    private float _duration = 5f;
    private float _dps = 20f;
    private float _slowPercent = 0.4f; // 0.4 = 40% 감소
    private float _tickInterval = 0.5f;
    [SerializeField] private string _enemyTag = "Enemy";
    [SerializeField] private Transform _visualRoot;           // 비주얼 루트(메시/파티클 부모)
    [SerializeField] private float _visualBaseDiameter = 1f;  // 스케일 1일 때 비주얼의 지름

    private readonly HashSet<Collider> _targets = new HashSet<Collider>();
    private SphereCollider _collider;

    public void Init(float radius, float duration, float dpsValue, float slowPct, float tickInterval)
    {
        _radius = radius;
        _duration = duration;
        _dps = dpsValue;
        _slowPercent = Mathf.Clamp01(slowPct);
        _tickInterval = Mathf.Max(0.05f, tickInterval);

        if (_collider != null)
            _collider.radius = _radius;

        ApplyRadiusToVisual(); // Init으로 반경 변경 시에도 비주얼 크기 동기화
    }

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        _collider.isTrigger = true;
        _collider.radius = _radius;
    }

    private void OnEnable()
    {
        StartCoroutine(CoRun());
    }

    private void Start()
    {
        // 시작 시에도 반경과 비주얼 크기 동기화
        if (_collider != null)
            _collider.radius = _radius;
        ApplyRadiusToVisual();
    }

    // 반경(_radius)에 맞춰 _visualRoot의 균일 스케일을 설정
    private void ApplyRadiusToVisual()
    {
        if (_visualRoot == null) return;

        float diameter = _radius * 2f;
        float baseDiameter = Mathf.Max(0.0001f, _visualBaseDiameter);
        float uniformScale = diameter / baseDiameter;

        _visualRoot.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
    }

    private IEnumerator CoRun()
    {
        float elapsed = 0f;
        float tickTimer = 0f;

        while (elapsed < _duration)
        {
            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;
            tickTimer += deltaTime;

            if (tickTimer >= _tickInterval)
            {
                float damage = _dps * _tickInterval;
                ApplyTick(damage);
                tickTimer = 0f;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void ApplyTick(float damage)
    {
        // HashSet 순회 중 null/비활성 제거
        var toRemove = new List<Collider>();
        foreach (var col in _targets)
        {
            if (col == null || !col.gameObject.activeInHierarchy)
            {
                toRemove.Add(col);
                continue;
            }

            // 1) 데미지 적용
            if (col.TryGetComponent(out TestEnemy enemy))
            {
                enemy.TakeDamage(damage);
                
                float slowDurationPerTick = Mathf.Max(_tickInterval * 1.1f, _tickInterval); // 틱 간 끊김 방지
                var payload = new SlowPayload(_slowPercent, slowDurationPerTick);
                enemy.ApplySlow(payload);
            }

        }

        // 정리
        foreach (var r in toRemove)
            _targets.Remove(r);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other || (_enemyTag.Length > 0 && !other.CompareTag(_enemyTag)))
            return;

        _targets.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other) return;
        _targets.Remove(other);
    }

    // 슬로우 전달용 간단한 페이로드
    public class SlowPayload
    {
        public float Percent;   // 0~1
        public float Duration;  // seconds
        public SlowPayload(float percent, float duration)
        {
            Percent = percent;
            Duration = duration;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, _radius);
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
#endif
}
