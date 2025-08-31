using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack Pattern")]
public class AttackPattern : ScriptableObject
{
    [Header("Attack Normal")] 
    public float range = 2.0f;      //공격 사거리
    public float damage = 1.0f;     //대미지
    public float hitRadius = 2.0f;  //대미지 범위
    public LayerMask hitMask;       //플레이어 레이어
    
    [Header("Attack Timing")]
    public float preDelay = 0.3f;   //공격 전 딜레이
    public float postDelay = 0.5f;  //공격 후 딜레이
    public float cooldown = 0.5f;   //다음 공격까지 쿨

    [Header("Retreat")] //후퇴
    public float retreatCheckDistance = 1.0f;   //공격 직후 플레이어가 이 거리 이내면 후퇴
    public float retreatDuration = 0.3f;        //후퇴 시간
}
