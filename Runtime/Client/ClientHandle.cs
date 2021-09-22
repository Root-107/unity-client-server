using ClientServer.Sending;
using System.Net;
using UnityEngine;


namespace ClientServer
{
    public class ClientHandle
    {
        public static void InitaliseConnection(Packet packet)
        {
            string message = packet.ReadString();
            int id = packet.ReadInt();

            Debug.Log($"{message}");
            Client.instance.id = id;
            Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
            Client.instance.OnConnected?.Invoke();
        }

        public static void OnConnect(Packet packet)
        {
            int id = packet.ReadInt();
        }

        public static void OnDisconnect(Packet packet)
        {
            int id = packet.ReadInt();
        }
    }
}
