using UnityEngine;

public class Archer_Skill_W : MonoBehaviour, ISkill
{
    [Header("Q 스킬 프리펩")]
    [SerializeField] private GameObject Q_arrow;

    public void Press(GameObject player, Transform mouseTransform)
    {
        //범위 표시
    }


    // 스킬 발사(실제 사용, private)
    public void Release(GameObject player, Transform mouseTransform)
    {
        Vector3 direction = (mouseTransform.position - player.transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        Instantiate(this.Q_arrow, player.transform.position, rotation);
    }
}
