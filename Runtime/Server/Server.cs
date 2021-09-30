using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using ClientServer.Sending;

namespace ClientServer
{
    public class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int TcpPort { get; private set; }
        public static int UdpPort { get; private set; }
        public static Dictionary<int, ServerClientInstance> clients = new Dictionary<int, ServerClientInstance>();
        public delegate void PacketHandler(int client, Packet packet);

        public static string Token { get => token; set { token = value; } }
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static Action<int> OnConnect;
        public static Action<int> OnDisconnect;
        private static string token = "";

        public static void Start(int maxPlayers, int tcpPort, int udpPort)
        {
            //When starting server set the max players and the port
            MaxPlayers = maxPlayers;
            TcpPort = tcpPort;
            UdpPort = udpPort;
            InitaliseServerData();

            //create a new TCP listener
            tcpListener = new TcpListener(IPAddress.Any, TcpPort);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(UdpPort);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Debug.Log($"Sever started on TCP:{TcpPort}, UDP:{UdpPort}...");
        }

        //Stores the tcp client instance then starts the listen again.
        private static void TCPConnectCallback(IAsyncResult ar)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(ar);

            //Once a client connects, continue listening
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Debug.Log($"Connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    OnConnect?.Invoke(i);
                    return;
                }
            }

            Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Max number of players...");
        }

        public static void DisconnectClient(int id)
        {
            if (clients.ContainsKey(id))
            {
                clients[id].Disconnect();
                OnDisconnect?.Invoke(id);
            }
        }

        private static void HandleClientDisconnect(int id) 
        {
            OnDisconnect?.Invoke(id);
        }

        public static void CloseServer()
        {
            foreach (KeyValuePair<int, ServerClientInstance> c in clients)
            {
                DisconnectClient(c.Key);
            }

            Debug.Log($"Closing server on TCP:{TcpPort}, UDP:{UdpPort}...");
        }

        private static void UDPReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

                byte[] data = udpListener.EndReceive(ar, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    int clientId = packet.ReadInt();
                    if (clientId == 0)
                    {
                        return;
                    }

                    if (clients[clientId].udp.endPoint == null)
                    {
                        clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
                    {
                        clients[clientId].udp.HandleData(packet);
                    }
                }
            }
            catch (Exception e)
            {
                //udpListener.Dispose();
                Debug.Log($"Error receiving UDP data: {e}");
                udpListener.BeginReceive(UDPReceiveCallback, null);
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if (clientEndPoint != null)
                {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data to {clientEndPoint} via UDP: {e}");
            }
        }

        public static void CreateServerPackets(Dictionary<int, PacketHandler> packetHandler)
        {
            foreach (KeyValuePair<int, PacketHandler> handler in packetHandler)
            {
                packetHandlers.Add(handler.Key, handler.Value);
            }
        }

        private static void InitaliseServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                ServerClientInstance instance = new ServerClientInstance(i);
                instance.onDissconect = HandleClientDisconnect;
                clients.Add(i, instance);
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerClientPackets.onConnect, ServerHandle.OnConnect },
                { (int)ServerClientPackets.onDisconnected, ServerHandle.OnDisconnect }
            };
        }

        public static void SetPackets(Dictionary<int, PacketHandler> values)
        {
            foreach (KeyValuePair<int, PacketHandler> item in values)
            {
                packetHandlers.Add(item.Key, item.Value);
            }

            Debug.Log("Initalise Custom packets...");
        }
    }
}