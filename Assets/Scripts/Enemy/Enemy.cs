using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float hp;
    public float maxHp;

    public void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0f)
        {
            Destroy(this.gameObject);
        }
    }
}
