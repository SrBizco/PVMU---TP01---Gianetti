using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    public int port = 7777;
    public ChatUIManager chatUIManager;

    private TcpListener server;
    private bool isRunning;
    private List<TcpClient> clients = new List<TcpClient>();

    void Start()
    {
        StartServer();
    }

    public void StartServer()
    {
        server = new TcpListener(IPAddress.Any, port);
        server.Start();
        isRunning = true;

        Debug.Log("Servidor iniciado en el puerto " + port);
        Thread acceptThread = new Thread(AcceptClients);
        acceptThread.IsBackground = true;
        acceptThread.Start();
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
            clientThread.IsBackground = true;
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

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("Mensaje recibido de cliente: " + message);

                // Enviar al hilo principal para actualizar la UI
                if (chatUIManager != null)
                {
                    MainThreadDispatcher.Enqueue(() => chatUIManager.AppendMessage(message));
                }

                // Reenviar a todos los clientes excepto al cliente que envió el mensaje
                BroadcastMessage(message, client);
            }
            catch (Exception e)
            {
                Debug.LogError("Error en cliente: " + e.Message);
                break;
            }
        }

        lock (clients)
        {
            clients.Remove(client);
        }

        client.Close();
    }

    public void SendMessageFromHost(string message)
    {
        string fullMessage = $"Host: {message}";

        Debug.Log("Mensaje enviado desde el host: " + fullMessage);

        // Mostrar en UI local del host
        if (chatUIManager != null)
        {
            MainThreadDispatcher.Enqueue(() => chatUIManager.AppendMessage(fullMessage));
        }

        // Enviar a todos los clientes
        BroadcastMessage(fullMessage);
    }

    private void BroadcastMessage(string msg, TcpClient senderClient = null)
    {
        Debug.Log("Broadcasting mensaje: " + msg);

        byte[] data = Encoding.UTF8.GetBytes(msg);

        lock (clients)
        {
            foreach (TcpClient client in clients)
            {
                if (client == senderClient) continue;

                try
                {
                    client.GetStream().Write(data, 0, data.Length);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("No se pudo enviar a cliente: " + e.Message);
                }
            }

            // También enviar el mensaje al host, que es el servidor
            if (senderClient == null && chatUIManager != null)
            {
                MainThreadDispatcher.Enqueue(() => chatUIManager.AppendMessage(msg)); // Mostrar en la UI del host
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        server?.Stop();
    }
}