using UnityEngine;

public class ArcherSkillQ : MonoBehaviour, ISkill
{
    [Header("Q 스킬 프리펩")]
    [SerializeField] private GameObject _Qarrow;
    [SerializeField] private float _damage = 20f;
    [SerializeField] private float _duration = 10f;

    public void Press(GameObject player, Transform mouseTransform)
    {
        //범위 표시
    }


    // 스킬 발사(실제 사용, private)
    public void Release(GameObject player, Transform mouseTransform)
    {
        if (_Qarrow == null || player == null || mouseTransform == null) return;

        // 기존: (mouse - player) → 반대로 계산해서 넘김
        Vector3 dir = (player.transform.position - mouseTransform.position);
        dir.y = 0f;
        Vector3 direction = dir.sqrMagnitude > 1e-6f ? dir.normalized : player.transform.forward;

        Quaternion rotation = Quaternion.LookRotation(direction);

        var go = Instantiate(this._Qarrow, player.transform.position, rotation);

        // 발사 방향/데미지 전달
        if (go.TryGetComponent(out ArcherQArrow arrow))
        {
            arrow.Init(direction);
            arrow.SetDamage(_damage);
        }
    }
}
