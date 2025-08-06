using UnityEngine;

public class HeroSkill : MonoBehaviour
{
    //��� ��ų���� �θ� Ŭ����
    [SerializeField] protected float skillDamage; //��ų ������
    [SerializeField] protected float skillCoolTime; //��ų ��Ÿ��
    [SerializeField] protected bool canUse = false; //��ų ��� ���� ����

    protected virtual void useSkill() { }
}
