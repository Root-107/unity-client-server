using ClientServer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSettings 
{
    public int maxPlayers;
    public int frameRate;
    public string token;
    public int port;
    public Action<int> onConnect;
    public Action<int> onDisconect;
    public Dictionary<int, Server.PacketHandler> packets;

    /// <summary>
    /// Object used for server initalisation
    /// </summary>
    /// <param name="maxPlayers">Max clients that can join the server</param>
    /// <param name="frameRate">Fixed frame rate the server will run at</param>
    /// <param name="port">Port the server will run on</param>
    /// <param name="packets">Values the server will use to determin message handelers</param>
    public ServerSettings(
        int maxPlayers = 20, 
        int frameRate = 30, 
        int port = 8080, 
        Dictionary<int, Server.PacketHandler> packets = null,
        string token = "")
    {
        this.maxPlayers = maxPlayers;
        this.frameRate = frameRate;
        this.port = port;
        this.packets = packets;
        this.token = token;
    }
}


public class ClientSettings
{
    public string token;
    public Action onClientReady;
    public Action onConnect = null;
    public Action onDisconnect = null; 
    public Dictionary<int, Client.PacketHandler> packets;

    /// <summary>
    /// Object used for client initalisation
    /// </summary>
    /// <param name="token">Token for autorising the connection, without the correct token the server will dissconnect the user</param>
    /// <param name="onClientReady">Callback for when the client has initalised</param>
    /// <param name="onConnect">Callback for when the client has connected</param>
    /// <param name="onDisconnect">Callback for when the client has disconnected</param>
    /// <param name="packets">Values the client will use to determin message handelers</param>
    public ClientSettings(
        string token = "0000",
        Action onClientReady = null, 
        Action onConnect = null,
        Action onDisconnect = null,
        Dictionary<int, Client.PacketHandler> packets = null)
    {
        this.token = token;
        this.onClientReady = onClientReady;
        this.onConnect = onConnect;
        this.onDisconnect = onDisconnect;
        this.packets = packets;
    }
}

public static class ServerClient
{
    public static bool isServer = false;

    public static Client client { get; private set; }

    public delegate void ServerEvent(int client);
    /// <summary>
    /// Called on new client connection
    /// </summary>
    public static event ServerEvent clientConnection;
    /// <summary>
    /// Called on client disconnect
    /// </summary>
    public static event ServerEvent clientDisconnect;

    public static void CreateServer(ServerSettings settings) 
    {
        isServer = true;
        CreateThreadManager();
        new GameObject("NetworkManager").AddComponent<NetworkManager>();
        NetworkManager.instance.InitialiseServer(settings.maxPlayers, settings.frameRate, settings.port);

        if (settings.packets != null)
        {
            Dictionary<int, Server.PacketHandler> packets = CheckPackets(settings.packets);
            Server.SetPackets(settings.packets);
        }

        Server.Token = settings.token;

        Server.OnConnect = OnConnect;
        Server.OnDisconnect = OnDisconnect;
    }

    private static void OnConnect(int client) 
    {
        clientConnection?.Invoke(client);
    }

    private static void OnDisconnect(int client) 
    {
        clientDisconnect?.Invoke(client);
    }

    public static void CreateClient(ClientSettings settings)
    {
        CreateThreadManager();
        new GameObject("Client").AddComponent<Client>();
        client = Client.instance;

        client.Token = settings.token;
        client.OnConnected = settings.onConnect;
        client.OnDisconnect = settings.onDisconnect;
        client.OnClientReady = settings.onClientReady;

        client.InitialiseClientData();

        if (settings.packets != null)
        {
            Dictionary<int, Client.PacketHandler> packets = CheckPackets(settings.packets);
            client.CreateClientPackets(packets);
        }
    }

    private static Dictionary<int, T> CheckPackets<T>(Dictionary<int, T> packets) 
    {
        int connectId = (int)ServerClientPackets.onConnect;
        int disconnectId = (int)ServerClientPackets.onDisconnected;

        if (packets.ContainsKey(connectId))
        {
            Debug.LogError($"Packet key {connectId} is reserved for onConnect, removing duplicate packet.");
            packets.Remove(connectId);
        }

        if (packets.ContainsKey(disconnectId))
        {
            Debug.LogError($"Packet key {disconnectId} is reserved for onDisconnected, removing duplicate packet.");
            packets.Remove(disconnectId);
        }

        return packets;
    }

    /// <summary>
    /// Connect to server will call back on success or fail
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="connectionCallback">If the connection times out will pass back false</param>
    public static void ConnectToServer(string ip, int port, Action<bool> connectionCallback = null)
    {
        client.ConnectedToServer(ip, port, connectionCallback);
    }

    public static void DisconnectFromServer(string message = "") 
    {
        Client.instance.Disconnect();
    }

    private static void CreateThreadManager() 
    {
        new GameObject("ThreadManager").AddComponent<ThreadManager>();
    }
}
