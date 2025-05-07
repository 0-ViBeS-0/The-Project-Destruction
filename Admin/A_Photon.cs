using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class A_Photon : MonoBehaviourPunCallbacks
{
    [SerializeField] private string _region;
    [SerializeField] private TMP_Text _regionText;
    [SerializeField] private string _version;
    [SerializeField] private TMP_Text _versionText;
    [SerializeField] private TMP_Text _pingText;
    [SerializeField] private TMP_Text _fpsText;
    private float _deltaTime = 0.0f;
    [SerializeField] private TMP_Text _debugText;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private TMP_Text _isConected;
    [SerializeField] private TMP_Text _inLobby;
    [SerializeField] private TMP_Text _inRoom;

    private void Start()
    {
        _isConected.text = "Is Conected: false";
        _inLobby.text = "In Lobby: false";
    }

    private void Update()
    {
        FpsAndPing();
    }

    public void ConnectToCloud()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = _region;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = _version;

        Debug("Connecting To Master...");
    }

    public void DisconnectCloud()
    {
        PhotonNetwork.Disconnect();

        Debug("Disconnecting From Master...");
    }

    public override void OnConnectedToMaster()
    {
        _regionText.text = "Region: " + _region;
        _versionText.text = "Version: " + PhotonNetwork.GameVersion;
        _isConected.text = "Is Conected: true";

        Debug("Connected To Master!");
        Message("Connected");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _regionText.text = "Region: -";
        _versionText.text = "Version: -";
        _isConected.text = "Is Conected: false";

        Debug("Disconnected From Master!");
        Message("Cause: " + cause);
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();

        Debug("Connecting To Lobby...");
    }

    public void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();

        Debug("Leaving From Lobby...");
    }

    public override void OnJoinedLobby()
    {
        _inLobby.text = "In Lobby: true";

        Debug("Connected To Lobby!");
        Message("Joined");
    }

    public override void OnLeftLobby()
    {
        _inLobby.text = "In Lobby: false";

        Debug("Leaved From Lobby!");
        Message("Left");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

    }

    public void CreateRoom()
    {


        Debug("Creating Room...");
    }

    public override void OnCreatedRoom()
    {
        Debug("Created Room!");
        Message("Created");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug("NOT Created Room!");
        Message("Cause: " + message);
    }

    public void SetRegion(string region)
    {
        _region = region;
    }

    public void SetVersion(string version)
    {
        _version = version;
    }

    private void Debug(string text)
    {
        _debugText.text = text;
    }

    private void Message(string message)
    {
        _messageText.text = message;
    }

    private void FpsAndPing()
    {
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        float fps = 1.0f / _deltaTime;
        _fpsText.text = string.Format("FPS: {0:0.}", fps);
        _pingText.text = "Ping: " + PhotonNetwork.GetPing() + " ms";
    }
}
