using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientServer
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
            }
        }

        public void InitialiseServer(int maxPlayers, int frameRate, int tcpPort, int udpPort)
        {
            Application.targetFrameRate = frameRate;
            Server.Start(maxPlayers, tcpPort, udpPort);
        }
    }
}