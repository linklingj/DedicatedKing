using UnityEngine;

public class Archer_Q_Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float maxDistance = 50f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
        if (Vector3.Distance(startPos, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 적에게 데미지 적용
            // other.GetComponent<Enemy>().TakeDamage(damage);
            // 관통이므로 Destroy(gameObject) 하지 않음
        }
    }
}
