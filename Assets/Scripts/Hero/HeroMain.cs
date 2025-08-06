using UnityEngine;
using UnityEngine.UIElements;

public class HeroMain : MonoBehaviour
{
    #region heroScript
    [SerializeField] private HeroState heroState; //용사 상태 정보
    [SerializeField] private HeroMovement heroMovement; //용사 이동
    [SerializeField] private HeroLevel heroLevel; //용사 이동
    [SerializeField] private HeroSkillQ heroSkillQ;
    [SerializeField] private HeroSkillW heroSkillW;
    [SerializeField] private HeroSkillE heroSkillE;
    [SerializeField] private HeroSkillR heroSkillR;
    #endregion

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
