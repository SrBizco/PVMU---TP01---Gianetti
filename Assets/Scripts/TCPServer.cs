using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    public int port = 7777;
    private TcpListener server;
    private bool isRunning;
    private List<TcpClient> clients = new List<TcpClient>();

    void Start()
    {
        StartServer();
    }

    public void StartServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            isRunning = true;
            Debug.Log("Servidor iniciado en el puerto " + port);
            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Error al iniciar servidor: " + e.Message);
        }
    }

    private void AcceptClients()
    {
        while (isRunning)
        {
            TcpClient client = server.AcceptTcpClient();
            lock (clients)
            {
                clients.Add(client);
            }
            Debug.Log("Nuevo cliente conectado");
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        while (isRunning)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("Mensaje recibido del cliente: " + msg);

                BroadcastMessage(msg, client);
            }
            catch
            {
                break;
            }
        }

        lock (clients)
        {
            clients.Remove(client);
        }
        client.Close();
    }

    private void BroadcastMessage(string msg, TcpClient sender)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        lock (clients)
        {
            foreach (TcpClient client in clients)
            {
                if (client != sender)
                {
                    try
                    {
                        client.GetStream().Write(data, 0, data.Length);
                    }
                    catch { }
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        isRunning = false;
        server?.Stop();
    }
}