using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.Collections;
using ClientServer.Sending;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif


namespace ClientServer
{
    public class Client : MonoBehaviour
    {
        public static Client instance;
        public static int dataBufferSize = 4096;

        public int id;
        public string ip = "80.193.23.116";
        public int port = 8080;

        public TCP tcp;
        public UDP udp;

        bool clientReady = false;

        public string token = "";
        public string Token { get => token; set => token = value; }

        private Action onConnected;
        public Action OnConnected { get { return onConnected; } set { onConnected = value; } }

        private Action onDisconnect;
        public Action OnDisconnect { get { return onDisconnect; } set { onDisconnect = value; } }

        public Action onClientReady;

        public Action<bool> connectionCallback;

        public Action OnClientReady { 
            get { return onClientReady; } 
            set {
                onClientReady = value;
                if (clientReady) 
                {
                    onClientReady?.Invoke();
                }
            }
        }

        bool isConnected = false;
        public bool IsConnected { get { return isConnected; } private set { isConnected = value; } }

        public delegate void PacketHandler(Packet packet);
        private static Dictionary<int, PacketHandler> packetHandler;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Instance already exists, destroying instance");
                Destroy(this);
            }

            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            if (packetHandler.Count > 0)
            {
                clientReady = true;
            }

            if (clientReady)
            {
                onClientReady?.Invoke();
            }
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        public void UDPConnectionState(bool state)
        {
            if (!state)
            {
                if (udp.socket != null)
                {
                    udp.socket.Close();
                }
            }
        }

        public void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                tcp.socket.Close();
                udp.socket.Close();

                Debug.Log("Disscconected from server");
            }
            onDisconnect?.Invoke();
        }

        public void ConnectedToServer(string ip = "", int port = 0, Action<bool> connectionCallback = null)
        {
            this.ip = ip;
            this.port = port;
            this.connectionCallback = connectionCallback;

            tcp = new TCP();
            udp = new UDP();

            tcp.Connect(connectionCallback);
        }

        public class TCP
        {
            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;
            private Action<bool> connectionCallback;

            public void Connect(Action<bool> connectionCallback = null)
            {
                this.connectionCallback = connectionCallback;

                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            }

            private void ConnectCallback(IAsyncResult ar)
            {
                ThreadManager.ExecuteOnMainThread(ReturnCallbackResult);

                socket.EndConnect(ar);

                if (!socket.Connected)
                {
                    return;
                }

                stream = socket.GetStream();

                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            private void ReturnCallbackResult()
            {
                connectionCallback?.Invoke(socket.Connected);
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Error Sending TCP data: {e}");
                }
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                try
                {
                    int byteLength = stream.EndRead(ar);
                    if (byteLength <= 0)
                    {
                        instance.Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Debug.Log($"Error receving TCP data: {e}");
                    ThreadManager.ExecuteOnMainThread(() => { Disconnect(); }); 
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;
                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
                {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            packetHandler[packetId](packet);
                        }
                    });

                    packetLength = 0;

                    if (receivedData.UnreadLength() >= 4)
                    {
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            private void Disconnect()
            {
                instance.Disconnect();

                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;

            public UDP()
            {
                endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            }

            public void Connect(int localPort)
            {
                socket = new UdpClient(localPort);
                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using (Packet packet = new Packet())
                {
                    SendData(packet);
                }
            }

            public void SendData(Packet packet)
            {
                try
                {
                    packet.InsertInt(instance.id);
                    if (socket != null)
                    {
                        socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Error receving UDP data: {e}");
                }
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                try
                {
                    byte[] data = socket.EndReceive(ar, ref endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    if (data.Length < 4)
                    {
                        instance.Disconnect();
                        return;
                    }

                    HandleData(data);
                }
                catch (Exception e)
                {
                    Disconnect();
                    Debug.Log($"Error receving UDP data: {e}");
                }
            }

            private void HandleData(byte[] data)
            {
                using (Packet packet = new Packet(data))
                {
                    int packetLength = packet.ReadInt();
                    data = packet.ReadBytes(packetLength);
                }

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(data))
                    {
                        int packetId = packet.ReadInt();
                        packetHandler[packetId](packet);
                    }
                });
            }

            private void Disconnect()
            {
                instance.Disconnect();
                endPoint = null;
                socket = null;
            }
        }

        public void CreateClientPackets(Dictionary<int, PacketHandler> packetHandelers) 
        {
            foreach (KeyValuePair<int, PacketHandler> handler in packetHandelers) 
            {
                packetHandler.Add(handler.Key, handler.Value);
            }
        }

        public void InitialiseClientData()
        {
            packetHandler = new Dictionary<int, PacketHandler>()
            {
                {(int)ServerClientPackets.onConnect, ClientHandle.InitaliseConnection },
                {(int)ServerClientPackets.onDisconnected,ClientHandle.OnDisconnect }
            };

            Debug.Log("Initalized packets");
        }
    }
}