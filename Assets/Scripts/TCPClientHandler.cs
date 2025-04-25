using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClientHandler : MonoBehaviour
{
    // Estas variables ahora las vamos a manejar desde ConnectionManager
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;

    // Estas variables serán asignadas a través del ConnectionManager
    private string serverIP;
    private int serverPort;
    public ChatUIManager chatUIManager;

    void Start()
    {
        // En este caso no inicializamos nada en Start porque lo manejamos en ConnectionManager
    }

    public void ConnectToServer(string ip, int port)
    {
        serverIP = ip;
        serverPort = port;

        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            isConnected = true;
            Debug.Log("Conectado al servidor en " + serverIP + ":" + serverPort);

            // Comenzamos el hilo de recepción de mensajes
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Error al conectar con el servidor: " + e.Message);
        }
    }

    public void SendMessageToServer(string msg)
    {
        if (!isConnected) return;

        byte[] data = Encoding.UTF8.GetBytes($"Cliente: {msg}");
        stream.Write(data, 0, data.Length);

        Debug.Log("Mensaje enviado al servidor: " + msg);

        // Mostrar localmente también
        if (chatUIManager != null)
        {
            MainThreadDispatcher.Enqueue(() => chatUIManager.AppendMessage("Tú: " + msg));
        }
    }

    private void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];

        while (isConnected)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Debug.LogWarning("Stream cerrado por el servidor.");
                    break;
                }

                string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("Cliente recibió mensaje del servidor: " + msg);

                if (chatUIManager != null)
                {
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        Debug.Log("Cliente encola mensaje en hilo principal: " + msg);
                        chatUIManager.AppendMessage(msg);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error recibiendo mensaje en cliente: " + ex.Message);
                break;
            }
        }
    }

    private void OnApplicationQuit()
    {
        isConnected = false;
        stream?.Close();
        client?.Close();
    }
}