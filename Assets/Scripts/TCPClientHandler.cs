using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClientHandler : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    public int serverPort = 7777;
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;

    void Start()
    {
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            isConnected = true;
            Debug.Log("Conectado al servidor");
            receiveThread = new Thread(ReceiveMessages);
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

        byte[] data = Encoding.UTF8.GetBytes(msg);
        stream.Write(data, 0, data.Length);
    }

    private void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];

        while (isConnected)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("Mensaje recibido del servidor: " + msg);
            }
            catch
            {
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