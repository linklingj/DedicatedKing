using UnityEngine;

public interface ISkill
{
    // 스킬 입력(꾹 누르기 등)
    void Press(GameObject player, Transform mouseTransform);
    void Release(GameObject player, Transform mouseTransform);
}
