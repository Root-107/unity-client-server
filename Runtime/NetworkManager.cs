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

        public void InitaliseServer(int maxPlayers, int frameRate, int port)
        {
            Application.targetFrameRate = frameRate;
            Server.Start(maxPlayers, port);
        }
    }
}