using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    public void TakeDamage(float damage)
    {
        Debug.Log("TakeDamage" + damage);
    }
    public void ApplySlow(SlowField.SlowPayload percent)
    {
        Debug.Log("ApplySlow"+ percent);
    }
}
