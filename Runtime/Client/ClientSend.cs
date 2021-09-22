namespace ClientServer.Sending
{
    /// <summary>
    /// Send client packet to server
    /// </summary>
    public class ClientSend
    {
        /// <summary>
        /// Send TCP data
        /// </summary>
        /// <param name="packet"></param>
        public static void SendTCPData(Packet packet)
        {
            packet.WriteLength();
            Client.instance.tcp.SendData(packet);
        }

        /// <summary>
        /// Send UDP data
        /// </summary>
        /// <param name="packet"></param>
        public static void SendUDPData(Packet packet)
        {
            packet.WriteLength();
            Client.instance.udp.SendData(packet);
        }
    }
}
