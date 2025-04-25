using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIManager : MonoBehaviour
{
    public TCPClientHandler clientHandler;
    public TCPServer serverHandler;

    public TMP_InputField inputField;
    public Button sendButton;
    public ScrollRect scrollRect;
    public Transform messageParent;
    public GameObject messagePrefab;

    void Start()
    {
        sendButton.onClick.AddListener(OnSendMessage);
        Debug.Log("UI de chat inicializada.");
    }

    private void OnSendMessage()
    {
        string message = inputField.text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        Debug.Log("Enviando mensaje: " + message);

        if (clientHandler != null)
        {
            clientHandler.SendMessageToServer(message);
        }
        else if (serverHandler != null)
        {
            serverHandler.SendMessageFromHost(message);
        }

        inputField.text = "";
    }

    public void AppendMessage(string message)
    {
        Debug.Log("Mensaje recibido para mostrar en la UI: " + message);
        GameObject newMsg = Instantiate(messagePrefab, messageParent);
        newMsg.GetComponent<TMP_Text>().text = message;
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageParent.GetComponent<RectTransform>());
        scrollRect.verticalNormalizedPosition = 0f;
    }
}