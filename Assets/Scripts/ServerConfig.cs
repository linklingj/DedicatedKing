using UnityEngine;

[CreateAssetMenu(menuName = "Configs/ServerConfig", fileName = "ServerConfig")]
public class ServerConfig : ScriptableObject
{
    public string ip = "127.0.0.1";
    public int port = 8888;
}
