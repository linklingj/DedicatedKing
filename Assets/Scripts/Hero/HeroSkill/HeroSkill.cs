using UnityEngine;

public class HeroSkill : MonoBehaviour
{
    //모든 스킬들의 부모 클래스
    [SerializeField] protected float skillDamage; //스킬 데미지
    [SerializeField] protected float skillCoolTime; //스킬 쿨타임
    [SerializeField] protected bool canUse = false; //스킬 사용 가능 여부

    protected virtual void useSkill() { }
}
