using UnityEngine;

public class SkillHandler : MonoBehaviour
{
    public enum SkillSlot { Q, W, E, R }
    private ISkill[] skillSlots = new ISkill[4];
    private KeyCode[] skillKeys = new KeyCode[4];
    
    public void SetSkill(SkillSlot slot, ISkill skill, KeyCode key)
    {
        skillSlots[(int)slot] = skill;
        skillKeys[(int)slot] = key;
    }

    void Update()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null && Input.GetKeyDown(skillKeys[i]))
            {
                // player와 target은 상황에 맞게 전달
                // 예시: this.gameObject, Camera.main.transform
                PressSkill((SkillSlot)i, this.gameObject, Camera.main.transform, skillKeys[i]);
            }
        }
    }

    public void PressSkill(SkillSlot slot, GameObject player, Transform target, KeyCode key)
    {
        skillSlots[(int)slot]?.Press(player, target, key);
    }
}
