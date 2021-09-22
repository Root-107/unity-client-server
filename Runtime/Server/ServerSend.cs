using UnityEngine;

namespace ClientServer.Sending
{
    /// <summary>
    /// Sending server messages to clients
    /// </summary>
    public class ServerSend
    {
        #region UDP
        /// <summary>
        /// Send UDP data
        /// </summary>
        /// <param name="client">Target client id</param>
        /// <param name="packet"></param>
        public static void SendUDPData(int client, Packet packet)
        {
            packet.WriteLength();
            Server.clients[client].udp.SendData(packet);
        }

        /// <summary>
        /// Send UDP data to all
        /// </summary>
        /// <param name="packet"></param>
        public static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }

        /// <summary>
        /// Send UDP data to all excluding (int)clientException
        /// </summary>
        /// <param name="clientException">Excluded client id</param>
        /// <param name="packet"></param>
        public static void SendUDPDataToAll(int clientException, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                if (i != clientException)
                {
                    Server.clients[i].udp.SendData(packet);
                }
            }
        }

        #endregion

        #region TCP
        /// <summary>
        /// Send TCP data
        /// </summary>
        /// <param name="client">Target client id</param>
        /// <param name="packet"></param>
        public static void SendTCPData(int client, Packet packet)
        {
            packet.WriteLength();
            Server.clients[client].tcp.SendData(packet);
        }
        /// <summary>
        /// Send TCP data to all
        /// </summary>
        /// <param name="packet"></param>
        public static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
        /// <summary>
        /// Send TCP data to all excluding (int)clientException
        /// </summary>
        /// <param name="clientException">Excluded client id</param>
        /// <param name="packet"></param>
        public static void SendTCPDataToAll(int clientException, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i < Server.MaxPlayers; i++)
            {
                if (i != clientException)
                {
                    Server.clients[i].tcp.SendData(packet);
                }
            }
        }
        #endregion

        #region Packets
        /// <summary>
        /// Default packet send methoud OnConnect
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public static void OnConnect(int client, string message)
        {
            using (Packet packet = new Packet((int)ServerClientPackets.onConnect))
            {
                packet.Write(message);
                packet.Write(client);

                SendTCPData(client, packet);
            }
        }
        /// <summary>
        /// Default packet send methoud OnDisconnected
        /// </summary>
        /// <param name="_playerId"></param>
        public static void OnDisconnected(int _playerId)
        {
            using (Packet _packet = new Packet((int)ServerClientPackets.onDisconnected))
            {
                _packet.Write(_playerId);

                SendTCPDataToAll(_packet);
            }
        }
        #endregion
    }

}