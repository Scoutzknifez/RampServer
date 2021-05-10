using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    #region TCP
    private static void SendTCPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    private static void SendTCPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }
    private static void SendTCPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }
    #endregion

    #region UDP
    private static void SendUDPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void SendUDPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }
    private static void SendUDPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }
    #endregion

    #region Packets
    public static void Welcome(int toClient, string msg)
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write(toClient);
            packet.Write(msg);

            SendTCPData(toClient, packet);
        }
    }

    public static void SpawnPlayer(int toClient, Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(player.id);
            packet.Write(player.username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTCPData(toClient, packet);
        }
    }

    public static void PlayerPosition(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void PlayerRotation(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            SendUDPDataToAll(player.id, packet);
        }
    }

    public static void PlayerDisconnected(int playerId)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            packet.Write(playerId);

            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerHealth(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerHealth))
        {
            packet.Write(player.id);
            packet.Write(player.health);

            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerRespawned(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRespawned))
        {
            packet.Write(player.id);

            SendTCPDataToAll(packet);
        }
    }

    public static void SpawnLevelPiece(int toClient, LevelPiece levelPiece)
    {
        using (Packet packet = new Packet((int)ServerPackets.levelPieceSpawned))
        {
            packet.Write(levelPiece.position);
            packet.Write(levelPiece.rotation);

            // Write the amount of vectors, then the vertices
            packet.Write(levelPiece.vertices.Length);
            foreach (Vector3 vector in levelPiece.vertices)
            {
                packet.Write(vector);
            }

            packet.Write(levelPiece.faces.Length);
            foreach (ArrayPacker packer in levelPiece.faces)
            {
                packet.Write(packer);
            }

            packet.Write(levelPiece.sharedVertices.Length);
            foreach (ArrayPacker packer in levelPiece.sharedVertices)
            {
                packet.Write(packer);
            }

            packet.Write(levelPiece.materialName);

            SendTCPData(toClient, packet);
        }
    }

    public static void BallSpawn(Ball ball)
    {
        using (Packet packet = new Packet((int)ServerPackets.ballSpawn))
        {
            packet.Write(ball.id);
            packet.Write(ball.isActive());
            packet.Write(ball.transform.position);
            packet.Write(ball.transform.rotation);
            packet.Write(ball.transform.localScale);

            SendTCPDataToAll(packet);
        }
    }

    public static void BallSpawn(int toClient, Ball ball)
    {
        using (Packet packet = new Packet((int)ServerPackets.ballSpawn))
        {
            packet.Write(ball.id);
            packet.Write(ball.isActive());
            packet.Write(ball.transform.position);
            packet.Write(ball.transform.rotation);
            packet.Write(ball.transform.localScale);

            SendTCPData(toClient, packet);
        }
    }

    public static void BallActive(Ball ball)
    {
        using (Packet packet = new Packet((int)ServerPackets.ballActive))
        {
            packet.Write(ball.id);
            packet.Write(ball.isActive());
            packet.Write(ball.transform.position);
            packet.Write(ball.transform.rotation);
            packet.Write(ball.transform.localScale);

            SendTCPDataToAll(packet);
        }
    }

    public static void BallRoll(Ball ball)
    {
        using (Packet packet = new Packet((int)ServerPackets.ballRoll))
        {
            packet.Write(ball.id);
            packet.Write(ball.transform.position);
            packet.Write(ball.transform.rotation);

            SendUDPDataToAll(packet);
        }
    }

    public static void BallCollided(Vector3 location)
    {
        using (Packet packet = new Packet((int)ServerPackets.ballCollided))
        {
            packet.Write(location);

            SendTCPDataToAll(packet);
        }
    }
    #endregion
}
