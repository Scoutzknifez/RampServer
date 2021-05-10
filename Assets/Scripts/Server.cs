using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int fromClient, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    private static bool closing = false;

    public static void Start(int _maxPlayers, int _port)
    {
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started on {Port}.");
    }

    public static void Stop()
    {
        closing = true;

        tcpListener.Stop();
        udpListener.Close();
    }

    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server Full");
    }

    private static void UDPReceiveCallback(IAsyncResult result)
    {
        if (closing)
        {
            Debug.Log("Server shutdown.");
            return;
        }

        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Debug.LogWarning("Here1.1");
            // ERROR HAPPENS HERE
            /*
             * Error receiving UDP data: System.Net.Sockets.SocketException (0x80004005): An existing connection was forcibly closed by the remote host.
             *  UnityEngine.Debug:Log (object)
             *  Server:UDPReceiveCallback (System.IAsyncResult) (at Assets/Scripts/Server.cs:102)
             *  System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()
             *
             * An existing connection was forcibly closed by the remote host.
             *  UnityEngine.Debug:LogError (object)
             *  Server:UDPReceiveCallback (System.IAsyncResult) (at Assets/Scripts/Server.cs:103)
             *  System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()
             *
             *   at System.Net.Sockets.SocketAsyncResult.CheckIfThrowDelayedException () [0x00014] in <aa976c2104104b7ca9e1785715722c9d>:0 
             *    at System.Net.Sockets.Socket.EndReceiveFrom (System.IAsyncResult asyncResult, System.Net.EndPoint& endPoint) [0x0003b] in <aa976c2104104b7ca9e1785715722c9d>:0 
             *    at System.Net.Sockets.UdpClient.EndReceive (System.IAsyncResult asyncResult, System.Net.IPEndPoint& remoteEP) [0x00036] in <aa976c2104104b7ca9e1785715722c9d>:0 
             *    at Server.UDPReceiveCallback (System.IAsyncResult result) [0x00016] in D:\Unity\Projects\RampServer\Assets\Scripts\Server.cs:67 
             *  UnityEngine.Debug:LogError (object)
             *  Server:UDPReceiveCallback (System.IAsyncResult) (at Assets/Scripts/Server.cs:105)
             *  System.Threading._ThreadPoolWaitCallback:PerformWaitCallback ()
             */
            byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
            Debug.LogWarning("Here1.2");
            udpListener.BeginReceive(UDPReceiveCallback, null);
            Debug.LogWarning("Here1.3");

            if (data.Length < 4)
            {
                return;
            }

            Debug.LogWarning("Here2");
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
            Debug.LogWarning("Here3");
        }
        catch (ObjectDisposedException e)
        {
            Debug.Log($"Server shutdown: {e}");
        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving UDP data: {e}");
            Debug.LogError(e.Message);
            Debug.LogError(e.InnerException);
            Debug.LogError(e.StackTrace);

            // TODO Temp bandaid, possibly permanent?
            // Either this or force disconnect?
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

    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
            };
        Debug.Log("Initialized Packets.");
    }
}
