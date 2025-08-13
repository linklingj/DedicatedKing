using Unity.VisualScripting;
using UnityEngine;

public class HeroState : MonoBehaviour
{
    //�÷��̾� ���� �� ���¸� �ٷ�� ��ũ��Ʈ


    //����
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


    //����
    #region state
    public bool canMove = true;
    public bool canAttack = true;
    public bool isJumping = false;
    public bool isOnGround = true;
    public bool isInvincible = false; //���� �������� �ƴ����� ��Ÿ���� ����
    #endregion
}
