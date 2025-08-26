using UnityEngine;

public class ArcherQArrow : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _maxDistance = 50f;
    [SerializeField] private float _damage = 20f; // 인스펙터 조정 가능하게
    private Vector3 _startPos;

    // 이동 방향을 별도로 보관(회전과 독립)
    private Vector3 _direction;

    // 모델/이펙트 루트(시각 보정용)
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private Vector3 _lookOffsetEuler = new Vector3(0f, 0f, 0f); // 모델 앞방향 보정(자식만)
    [SerializeField] private float _yawOffset = 0f; // 루트 회전 오프셋(이동과 무관, 기본 0)
    [SerializeField] private bool _invertMovement = false; // 이동이 반대로 나가면 체크

    private void Awake()
    {
        // 트리거 충돌 안정화를 위해 키네마틱 Rigidbody 보장
        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    void Start()
    {
        _startPos = transform.position;

        // 초기 방향이 없으면 현재 forward 사용(수평 평탄화)
        _direction = transform.forward;
        _direction.y = 0f;
        _direction = _direction.sqrMagnitude > 1e-6f ? _direction.normalized : Vector3.forward;

        // 루트는 "실제 이동 방향 + Y 오프셋"을 바라봄(시각용)
        transform.rotation = Quaternion.LookRotation(_direction) * Quaternion.AngleAxis(_yawOffset, Vector3.up);

        // 시각만 보정(자식에만 적용)
        if (_visualRoot != null)
            _visualRoot.localRotation = Quaternion.Euler(_lookOffsetEuler);
    }

    void Update()
    {
        // 이동(회전에 영향받지 않도록 _direction 사용)
        var moveDir = _invertMovement ? -_direction : _direction;
        transform.position += moveDir * (_speed * Time.deltaTime);

        // 루트는 항상 이동 방향(+Y 오프셋)을 바라보게 유지(시각용)
        transform.rotation = Quaternion.LookRotation(moveDir) * Quaternion.AngleAxis(_yawOffset, Vector3.up);

        // 최대 거리 초과 시 파괴
        if (Vector3.Distance(_startPos, transform.position) > _maxDistance)
        {
            Destroy(gameObject);
        }
    }

    // 발사 시 외부에서 초기 방향을 지정
    public void Init(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.000001f)
        {
            // 수평 성분만 사용(톱다운/등각 시 안정적)
            direction.y = 0f;
            _direction = direction.normalized;

            var lookDir = _invertMovement ? -_direction : _direction;
            transform.rotation = Quaternion.LookRotation(lookDir) * Quaternion.AngleAxis(_yawOffset, Vector3.up);
            if (_visualRoot != null)
                _visualRoot.localRotation = Quaternion.Euler(_lookOffsetEuler);
        }
    }

    // 스킬에서 데미지 전달
    public void SetDamage(float damage)
    {
        _damage = damage;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other || !other.CompareTag("Enemy")) return;

        // 적에게 데미지 적용(안전하게)
        if (other.TryGetComponent(out TestEnemy enemy))
        {
            enemy.TakeDamage(_damage);
        }
        // 관통이므로 Destroy(gameObject) 하지 않음
    }
}
