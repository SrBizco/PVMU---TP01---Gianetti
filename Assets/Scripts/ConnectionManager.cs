using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    public TMP_InputField ipInputField;
    public TMP_InputField portInputField;
    public Button startAsHostButton;
    public Button startAsClientButton;

    public GameObject tcpServerObject;
    public GameObject tcpClientObject;
    public GameObject chatUIObject;
    public GameObject connectionPanel;

    void Start()
    {
        startAsHostButton.onClick.AddListener(StartAsHost);
        startAsClientButton.onClick.AddListener(StartAsClient);
    }

    void StartAsHost()
    {
        int port = int.Parse(portInputField.text);
        tcpServerObject.GetComponent<TCPServer>().port = port;
        tcpServerObject.SetActive(true);

        var ui = chatUIObject.GetComponent<ChatUIManager>();
        var server = tcpServerObject.GetComponent<TCPServer>();

        ui.serverHandler = server;
        server.chatUIManager = ui;

        chatUIObject.SetActive(true);
        connectionPanel.SetActive(false);
    }

    void StartAsClient()
    {
        string ip = ipInputField.text;
        int port = int.Parse(portInputField.text);

        var clientHandler = tcpClientObject.GetComponent<TCPClientHandler>();
        clientHandler.ConnectToServer(ip, port);  // Asignamos la IP y el puerto aquí

        tcpClientObject.SetActive(true);

        chatUIObject.SetActive(true);
        chatUIObject.GetComponent<ChatUIManager>().clientHandler = clientHandler;

        connectionPanel.SetActive(false);
    }
}