using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [Tooltip("이 경로의 고유 ID (비워두면 GameObject 이름 사용)")]
    public string pathId;

    [Tooltip("적이 순찰할 노드들(월드 좌표)")]
    public Transform[] nodes;

    [Tooltip("노드를 도달했다고 간주할 거리")]
    public float arriveThreshold = 0.2f;

    public int Count => nodes?.Length ?? 0;
    public Vector3 GetNode(int index) => nodes[index % Count].position;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(pathId))
            pathId = gameObject.name;
    }

    private void OnDrawGizmos()
    {
        if (nodes == null || nodes.Length == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < nodes.Length; i++)
        {
            Vector3 a = nodes[i].position;
            Vector3 b = nodes[(i + 1) % nodes.Length].position;
            Gizmos.DrawSphere(a, 0.15f);
            Gizmos.DrawLine(a, b);
        }
    }
}