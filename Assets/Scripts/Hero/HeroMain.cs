using System;
using UnityEngine;
using UnityEngine.UIElements;

public class HeroMain : MonoBehaviour
{
    #region heroScript
    [SerializeField] private HeroState heroState;       //용사 정보
    [SerializeField] private HeroMovement heroMovement; //움직임
    [SerializeField] private HeroLevel heroLevel;       //용사 레벨
    [SerializeField] private SkillHandler skillHandler; //스킬 관리
    #endregion

    private void Awake()
    {
        #region GetComponent
        if (!TryGetComponent(out HeroState heroState))
        {
            this.gameObject.AddComponent(typeof(HeroState));
            this.heroState = GetComponent<HeroState>();
        }
        if (!TryGetComponent(out HeroMovement heroMovement))
        {
            this.gameObject.AddComponent(typeof(HeroMovement));
            this.heroMovement = GetComponent<HeroMovement>();
        }
        if (!TryGetComponent(out HeroLevel heroLevel))
        {
            this.gameObject.AddComponent(typeof(HeroLevel));
            this.heroLevel = GetComponent<HeroLevel>();
        }
        // if (!TryGetComponent(out SkillHandler skillHandler))
        // {
        //     this.gameObject.AddComponent(typeof(SkillHandler));
        //     this.skillHandler = GetComponent<SkillHandler>();
        // }
        #endregion 
        
        if (HeroManager.Instance != null)
            HeroManager.Instance.RegisterHero(this);
    }

    private void Update()
    {
        if (heroState.isDead)
        {
            Debug.Log("Hero Died");
            Destroy(this.gameObject);
        }
    }
    
    void OnDestroy()
    {
        // HeroManager에서 자기 자신 해제
        if (HeroManager.Instance != null)
            HeroManager.Instance.UnregisterHero(this);
    }
    
    public void TakeDamage(float damage)
    {
        heroState.hp -= damage;
        Debug.Log("take damage");
        if (heroState.hp <= 0f)
        {
            heroState.hp = 0f;
            heroState.isDead = true;
        }
    }
}
