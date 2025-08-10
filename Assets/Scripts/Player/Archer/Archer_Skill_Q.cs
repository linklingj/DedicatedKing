using UnityEngine;

public class Archer_Skill_Q : MonoBehaviour, ISkill
{
    [Header("Q 스킬 프리펩")]
    [SerializeField] private GameObject Q_arrow;

    public void Press(GameObject player, Transform mouseTransform, KeyCode key)
    {
        AOE(player, mouseTransform); // 범위 표시
        if (Input.GetKeyUp(key))  // 키를 뗄 때 발사 (예시)
        {
            Use(player, mouseTransform);
        }
    }

    // 스킬 범위(AOE) 표시 (private)
    private void AOE(GameObject player, Transform mouseTransform)
    {
        // 범위 표시 구현
    }

    // 스킬 발사(실제 사용, private)
    private void Use(GameObject player, Transform mouseTransform)
    {
        Vector3 direction = (mouseTransform.position - player.transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        Instantiate(this.Q_arrow, player.transform.position, rotation);
    }
}
