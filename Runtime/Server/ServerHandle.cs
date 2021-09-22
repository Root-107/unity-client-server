using ClientServer.Sending;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientServer
{
    public class ServerHandle
    {
        public static void OnConnect(int client, Packet packet)
        {
            int clientId = packet.ReadInt();
            string userName = packet.ReadString();
            string token = packet.ReadString();

            if (token != Server.Token || String.IsNullOrEmpty(userName))
            {
                Server.DisconnectClient(clientId);
                Debug.Log($"ID: {client} has given a false token, disconnecting");
                return;
            }

            if (client != clientId)
            {
                Debug.Log($"ID: {client} has assumed the wrong client id ({clientId})");
            }

            Server.OnConnect?.Invoke(client);
        }

        public static void OnDisconnect(int client, Packet packet)
        {
            Debug.Log($"Client {client}, disconnected");
        }
    }
}