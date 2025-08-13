using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class HeroMain : MonoBehaviour
{
    #region heroScript
    [SerializeField] private HeroState heroState; //��� ���� ����
    [SerializeField] private HeroMovement heroMovement; //��� �̵�
    [SerializeField] private HeroLevel heroLevel; //��� �̵�
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
        
        
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
