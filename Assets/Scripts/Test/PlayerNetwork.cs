// <summary> Unity C# 스크립트 (플레이어 이동 전송 & 수신) </summary>
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using MessagePack;
using System.Threading;
using UnityEngine;

public class PlayerNetwork : MonoBehaviour
{

    // 메인 스레드에서 적용할 작업 큐
    private readonly ConcurrentQueue<System.Action> _mainThread = new();
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
        while (_mainThread.TryDequeue(out var work)) work();
        // 예시: 마우스 왼쪽 클릭시 이동
        SendMove();
    }

    void SendMove()
    {
        var pos = transform.position;
        // 소수 둘째 자리로 반올림(선택)
        float rx = Mathf.Round(pos.x * 100f) / 100f;
        float ry = Mathf.Round(pos.y * 100f) / 100f;
        float rz = Mathf.Round(pos.z * 100f) / 100f;

        var packet = new DataPacket() {
            id = playerId,
            move = new MovementData { position= new Position(rx, ry, rz) },
            //skill = new SkillData { a = true, b = false }
        };

        var bytes = MessagePackSerializer.Serialize(packet);
        udp.SendAsync(bytes, bytes.Length, serverEndpoint);
    }

    private void ReceiveLoop(object? state)
    {
        try
        {
            while (true)
            {
                var remote = new IPEndPoint(IPAddress.Any, 0);
                var bytes  = udp.Receive(ref remote);

                DataPacket? p = null;
                try { p = MessagePackSerializer.Deserialize<DataPacket>(bytes); }
                catch { Debug.LogWarning("Deserialize Error"); }
                if (p == null)
                {
                    try
                    {
                        var dp = MessagePackSerializer.Deserialize<DataPacket>(bytes);
                        p = new DataPacket() { id = dp.id, move = dp.move, skill = dp.skill };
                    }
                    catch { continue; }
                }
                if (p == null || string.IsNullOrEmpty(p.id)) continue;

                if (p.move != null)
                {
                    var x = p.move.position.x; var y = p.move.position.y; var z = p.move.position.z;
                    var pid = p.id;
                    // Unity 오브젝트 업데이트는 메인 스레드에서 수행
                    _mainThread.Enqueue(() =>
                    {
                        if (pid == playerId) return; // 자기 자신은 로컬에서 이미 반영
                        var obj = GameObject.Find(pid); //todo: 로직 수정 필요
                        if (obj != null) obj.transform.position = new Vector3(x, y, z);
                    });
                }
            }
        }
        catch (SocketException) { /* 종료시 */ }
    }


    void OnDestroy()
    {
        try { udp?.Close(); } catch { }
        udp = null;
    }
}