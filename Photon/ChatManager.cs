using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatManager : MonoBehaviourPun
{
    #region Переменные

    [SerializeField] private RectTransform _chatPanel;
    [SerializeField] private RectTransform _contentContainer;
    [SerializeField] private GameObject _chatMessagePrefab;

    private ScrollRect _chatScrollRect;
    private TMP_InputField _chatInputField;
    private Button _sendButton;

    #endregion

    #region Awake/Start

    private void Awake()
    {
        _chatScrollRect = _chatPanel.GetChild(0).GetComponent<ScrollRect>();
        _chatInputField = _chatPanel.GetChild(1).GetComponent<TMP_InputField>();
        _sendButton = _chatPanel.GetChild(2).GetComponent<Button>();
    }

    private void Start()
    {
        _sendButton.onClick.AddListener(SendMessage);
        _chatInputField.onSubmit.AddListener(delegate { SendMessage(); });
    }

    #endregion

    private void SendMessage()
    {
        string message = _chatInputField.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            message = $"{PhotonNetwork.NickName}: {message}";
            photonView.RPC("ReceiveMessage", RpcTarget.All, message);
            _chatInputField.text = string.Empty;
            _chatInputField.ActivateInputField();
        }
    }

    #region Photon RPC

    [PunRPC]
    private void ReceiveMessage(string message)
    {
        GameObject newMessage = Instantiate(_chatMessagePrefab, _contentContainer);
        var textComponent = newMessage.GetComponent<TextMeshProUGUI>();
        textComponent.text = message;

        Canvas.ForceUpdateCanvases();
        _chatScrollRect.verticalNormalizedPosition = 0f;
        _contentContainer.sizeDelta += new Vector2(0, 100);
    }

    #endregion
}