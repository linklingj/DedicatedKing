using Unity.VisualScripting;
using UnityEngine;

public class HeroState : MonoBehaviour
{
    //플레이어 정보 및 상태를 다루는 스크립트


    //정보
    #region imformation
    public float hp;
    public float maxhp;
    public string userName;
    public string jobName;

    public Transform trans;
    public Rigidbody rigid;
    public BoxCollider col;
    public Animator ani;
    public Camera cam;
    public GameObject minimapIcon;
    #endregion


    //상태
    #region state
    public bool canMove = true;
    public bool canAttack = true;
    public bool isJumping = false;
    public bool isOnGround = true;
    public bool isInvincible = false; //무적 상태인지 아닌지를 나타내는 변수
    public bool isMoving = false; 
    #endregion
}
