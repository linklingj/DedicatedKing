// <summary> Unity C# 스크립트 (플레이어 이동 전송 & 수신) </summary>
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PlayerNetwork : MonoBehaviour
{
    [Header("Server")]
    public ServerConfig serverConfig;

    [Header("Player")]
    public string playerId = "p01";

    // UDP 소켓
    private UdpClient udp;
    private IPEndPoint serverEndpoint;

    // 이동 입력(시뮬레이션)
    private Vector3 moveSpeed = new(1f, 0, 0);

    void Start()
    {
        serverEndpoint = new IPEndPoint(IPAddress.Parse(serverConfig.ip), serverConfig.port);
        udp = new UdpClient(0); // 로컬 포트는 OS가 할당
        udp.EnableBroadcast = true;

        // 수신 쓰레드
        ThreadPool.QueueUserWorkItem(ReceiveLoop);
    }

    void Update()
    {
        // 예시: 마우스 왼쪽 클릭시 이동
        if (Input.GetMouseButton(0))
        {
            transform.position += moveSpeed * Time.deltaTime;
            SendMove();
        }
    }

    void SendMove()
    {
        // "MOVE:{ID}:{x}:{y}:{z}"
        var msg = $"MOVE:{playerId}:{transform.position.x:F2}:{transform.position.y:F2}:{transform.position.z:F2}";
        var data = Encoding.UTF8.GetBytes(msg);
        udp.SendAsync(data, data.Length, serverEndpoint);
    }

    private void ReceiveLoop(object? state)
    {
        try
        {
            while (true)
            {
                var remote = new IPEndPoint(IPAddress.Any, 0);
                var bytes  = udp.Receive(ref remote);
                var msg    = Encoding.UTF8.GetString(bytes);
                HandleServerMsg(msg);
            }
        }
        catch (SocketException) { /* 종료시 */ }
    }

    private void HandleServerMsg(string msg)
    {
        // 메시지 파싱
        var parts = msg.Split(':');
        if (parts.Length != 5 || parts[0] != "MOVE") return;

        var id = parts[1];
        var x  = float.Parse(parts[2]);
        var y  = float.Parse(parts[3]);
        var z  = float.Parse(parts[4]);

        // 같은 클라이언트(내 자신) 인지 체크
        if (id == playerId) return; // 이미 로컬에서 업데이트 함

        // 다른 플레이어 위치 업데이트
        // 여기서는 Scene 에 다른 GameObject가 존재한다고 가정
        // 예시: FindByTag("OtherPlayer") + SetPosition
        var obj = GameObject.Find(id);
        if (obj != null)
            obj.transform.position = new Vector3(x, y, z);
    }

    void OnDestroy() => udp.Close();
}