# Client Server
Creating a server and client instance.

## Table of Contents
- [Client Server](#client-server)
  - [Table of Contents](#table-of-contents)
- [Setup](#setup)
  - [Sever](#sever)
  - [Client](#client)
  - [Packets and Handlers](#packets-and-handlers)
    - [Example packets](#example-packets)
  - [Avalable Events](#avalable-events)
- [Sending Messages](#sending-messages)
  - [Examples](#examples)
- [Handling Messages](#handling-messages)
  - [Example](#example)

# Setup
## Sever
All variables passed into ServerSettings have default values.
```csharp
ServerClient.CreateServer(new ServerSettings(int maxPlayers, int frameRate, int tcpPort, int udpPort, Dictionary<int, Server.PacketHandler> packets));
```

## Client
```csharp
ServerClient.CreateClient(new ClientSettings(string token, Action onClientReady, Action onConnect, Action onDisconnect, Dictionary<int, Client.PacketHandler> packets)));
```

## Packets and Handlers
Packets are handled by reading the int value stored in the packet, this is the last value writen before it is sent, this should be managed by en enum value.
Packed 1000 and 2000 are reserved for connect and disconnect.
```csharp
public enum AppPackets
{
    SpawnItem = 1,
    SpawnPlayer = 2,
    ItemPosition = 3
}
```
Send Packet `(int)AppPackets.SpawnItem` => then gets passed to its associated handler.

### Example packets
```csharp
//Client
Dictionary<int, ClientServer.Client.PacketHandler> clientPackets = new Dictionary<int, ClientServer.Client.PacketHandler>()
{
    {(int)AppPackets.SpawnItem, ClientHandle.SpawnItem},
    {(int)AppPackets.ItemPosition, ClientHandle.ItemPosition}
};
//Server
Dictionary<int, ClientServer.Server.PacketHandler> serverPackets = new Dictionary<int, ClientServer.Server.PacketHandler>()
{
    {(int)AppPackets.SpawnItem,  ServerHandle.RequestSpawnItem},
};
```

## Avalable Events
You can listen on the server for the connection and disconnection of players.
```csharp
ServerClient.clientConnection += HandleClientConnect;
ServerClient.clientDisconnect += HandleClientDisconnect;

private void HandleClientConnect(int client) 
{
    ...
}

private void HandleClientDisconnect(int client) 
{
    ...
}
```

# Sending Messages
Create a new class that will contain static methods. In there add `using ClientServer.Sending;`

This will alow you to use the `ClientSend` and `ServerSend` classes.

## Examples
```csharp
// Server sending message to client to spawn an item
public static void SpawnItem(int index, int id, Vector3 pos, Vector3 rotation, int client)
{
    using (Packet packet = new Packet((int)AppPackets.SpawnItem))
    {
        packet.Write(index);
        packet.Write(id);
        packet.Write(pos);
        packet.Write(rotation);
        packet.Write(client);

        ServerSend.SendTCPDataToAll(packet);
    }
}

// Client sending message to server requesting to spawn an item
public static void RequestSpawnItem(string item, Vector3 positon, Vector3 rotation)
{
    using (Packet packet = new Packet((int)AppPackets.SpawnItem)) 
    {
        packet.Write(item);
        packet.Write(position);
        packet.Write(rotation);

        ClientSend.SendTCPData(packet);
    };
}
```

# Handling Messages
Messages will be passed to the methods defined in the server and client packets.

## Example
Client packet
```csharp
{(int)AppPackets.SpawnItem, ClientHandle.SpawnItem},
```

Handler Method
```csharp
public static void SpawnItem(Packet packet)
{
    int index = packet.ReadInt();
    int item = packet.ReadInt();
    Vector3 positon = packet.ReadVector3();
    Vector3 rotation = packet.ReadVector3();
    int client = packet.ReadInt();

    ItemManager.SpawnClientItem(item, positon, rotation, client, index);
}
```
