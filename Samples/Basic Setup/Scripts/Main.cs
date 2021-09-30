using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private bool runServerInEditor = false;

    [SerializeField] int maxPlayers = 10;
    [SerializeField] int serverFrameRate = 30;
    [SerializeField] int tcpPort = 8580;
    [SerializeField] int udpPort = 8581;
    [SerializeField] string connectionToken = "xcJQg*8c3!pE^xeGikGvVdQ$q*JKCzNFxtm5Cg";
    [SerializeField] string ip = "192.168.1.9";
    
    void Start()
    {
        if(Application.isBatchMode || runServerInEditor)
        {
            CreateServer();
            return;
        }

        CreateClient();
    }

    // Client
    private void CreateClient()
    {
        Dictionary<int, ClientServer.Client.PacketHandler> handler = new Dictionary<int, ClientServer.Client.PacketHandler>()
        {
            //...
        };

        ServerClient.CreateClient(new ClientSettings(connectionToken, ClientReady, ClientConnected, ClientDisconnected));
    }

    private void ClientReady()
    {
        ServerClient.ConnectToServer(ip, tcpPort, udpPort, HandleDidConnect);
    }

    private void HandleDidConnect(bool connected)
    {
        Debug.Log($"Connected to server - {connected}");
    }

    private void ClientConnected()
    {

    }

    private void ClientDisconnected()
    {
        
    }

    // Server
    private void CreateServer()
    {
        Dictionary<int, ClientServer.Server.PacketHandler> handler = new Dictionary<int, ClientServer.Server.PacketHandler>()
        {
            //...
        };

        ServerClient.CreateServer(new ServerSettings(maxPlayers, serverFrameRate, tcpPort, udpPort, handler, connectionToken));
    }
}