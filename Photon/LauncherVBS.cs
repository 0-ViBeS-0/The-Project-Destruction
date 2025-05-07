using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class LauncherVBS : MonoBehaviourPunCallbacks
{
    #region Переменные
    
    [Header("|-# SINGLETON")]
    public static LauncherVBS instance;

    [Header("|-# CONNECTING")]
    [SerializeField] private TMP_Text _disconectCauseText;
    [SerializeField] private string _disconectCause;
    private bool _isOfficialAccount;

    [Header("|-# ROOMS")]
    [SerializeField] private Transform _roomsListContent;
    [SerializeField] private GameObject _roomPrefab;
    private bool _sortRoomByName;
    private bool _sortRoomPyPlayerCount;
    private List<GameObject> _roomListItems = new();
    private List<RoomInfo> _currentRoomList = new();
    [SerializeField] private RectTransform roomRect;

    [Header("|-# CREATE ROOM")]
    [SerializeField] private TMP_InputField _roomNameInputField;
    [SerializeField] private TMP_InputField _maxPlayersInputField;

    [Header("|-# FIND ROOM")]
    [SerializeField] private TMP_InputField _findRoomNameInputField;

    [Header("|-# ON JOIN ROOM")]
    [SerializeField] private string _loadGameSceneGame;
    private bool _inRoom;
    private bool _currentIsInRoom;
    [SerializeField] private string _loadGameSceneMenu;

    [Header("|-# RECONNECTING")]
    private bool _reconnecting;

    [Header("|-# FAILED UI")]
    [SerializeField] private TMP_Text _failedText;
    [SerializeField] private TMP_Text _failedCauseText;
    [SerializeField] private string _failedEvent;

    [Header("|-# SET RATE")]
    [SerializeField] private int _sendRate;
    [SerializeField] private int _serializationRate;

    [Header("|-# STATISTIC")]
    [SerializeField] private TMP_Text _roomCountInLobby;
    [SerializeField] private TMP_Text _playerCountInLobby;
    private int _statRooms;
    private int _statPlayers;

    [Header("|-# DEBUG")]
    [SerializeField] private GameObject debugTextPrefab;
    [SerializeField] private Transform debugContent;
    [SerializeField] private RectTransform debugRect;

    #endregion

    #region Awake

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Выбрать регион

    public void SetRegion(int id, TMP_Text title)
    {
        string[] region = { null, "eu", "ru", "asia" };
        string[] regionT = { "Автоматический", "Европа", "Россия", "Азия" };
        if (PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion != region[id])
        {
            SwitchRegion(region[id]);
        }
        title.text = regionT[id];
    }

    private void SwitchRegion(string Region)
    {
        if (PhotonNetwork.IsConnected)
        {
            DebugText("Disconnected! Switching the Region...");
            MenuManager.instance.OpenMenu("loading");
            _reconnecting = false;
            PhotonNetwork.Disconnect();
            
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = Region;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = Region;
        }
    }

    #endregion

    #region Выбрать задержку для Photon Cloud

    public void SetRate()
    {
        PhotonNetwork.SendRate = _sendRate;
        PhotonNetwork.SerializationRate = _serializationRate;
    }

    #endregion

    #region Подключение/Отключение к Photon

    public void ConnectToServer(bool isOfficialAccount)
    {
        MenuManager.instance.OpenMenu("loading");
        PhotonNetwork.ConnectUsingSettings();
        DebugText("...Connecting to Server...");
        _isOfficialAccount = isOfficialAccount;
    }

    public override void OnConnected()
    {
        MenuManager.instance.OpenPanel("");
        DebugText($"Connected to Region: {PhotonNetwork.CloudRegion}");
        DebugText("Connected to Server!");
        _reconnecting = true;
        if (_isOfficialAccount)
            FireBaseManagerVBS.instance.SetStatus(true);
    }

    public override void OnConnectedToMaster()
    {
        MenuManager.instance.OpenMenu("main");
        DebugText("Connected to Master Server!");
        GameData.MyPhotonUserID = PhotonNetwork.LocalPlayer.UserId;
    }

    public void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
        MenuManager.instance.OpenMenu("loading");
        DebugText("...Disonnecting from Server...");
        _reconnecting = false;
        if (_isOfficialAccount)
            FireBaseManagerVBS.instance.SetStatus(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        LoadMenuScene();
        UIManagerVBS.instance.SetGameUI(false);
        MenuManager.instance.OpenMenu("login");
        _currentIsInRoom = false;
        if (_reconnecting)
        {
            MenuManager.instance.OpenPanel("disconnect");
            DebugText($"Disconnected from Server! Cause: {cause}");
            _disconectCauseText.text = $"Причина:  {cause}";
            _disconectCause = cause.ToString();
            Invoke(nameof(FailedReconnecting), 3f);
        }
        if (_isOfficialAccount)
            FireBaseManagerVBS.instance.SetStatus(false);
        UIManagerVBS.instance.SetActiveUIMobile(false);
    }

    public void Reconnect()
    {
        if (_inRoom)
        {
            PhotonNetwork.ReconnectAndRejoin();
        }
        else
        {
            PhotonNetwork.Reconnect();
        }
        MenuManager.instance.OpenMenu("loading");
        MenuManager.instance.OpenPanel("");
        if (!PhotonNetwork.IsConnected)
        {
            Invoke(nameof(FailedReconnecting), 5f);
        }
    }

    private void FailedReconnecting()
    {
        MenuManager.instance.OpenMenu("login");
        MenuManager.instance.OpenPanel("disconnect");
        DebugText($"Disconnected from Server! Cause: {_disconectCause}");
        _disconectCauseText.text = $"Причина:  {_disconectCause}";
    }

    #endregion

    #region Лобби

    public void JoinToLobby()
    {
        MenuManager.instance.OpenMenu("loading");
        PhotonNetwork.JoinLobby();
        DebugText("...Connecting to Lobby...");
    }

    public void LeaveFromLobby()
    {
        MenuManager.instance.OpenMenu("loading");
        PhotonNetwork.LeaveLobby();
        DebugText("...Disconnecting from Lobby...");
    }

    public override void OnJoinedLobby()
    {
        MenuManager.instance.OpenMenu("lobby");
        DebugText("Connected to Lobby!");
    }

    public override void OnLeftLobby()
    {
        MenuManager.instance.OpenMenu("main");
        DebugText("Disconnected from Lobby!");
    }

    #endregion

    #region Список комнат

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        _currentRoomList = roomList;
        UpdateRoomList(roomList);
    }

    void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (GameObject item in _roomListItems)
        {
            Destroy(item);
        }
        _roomListItems.Clear();

        _statRooms = 0;
        _statPlayers = 0;

        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) continue;

            GameObject roomListItem = Instantiate(_roomPrefab, _roomsListContent);
            roomListItem.GetComponent<RoomVBS>().SetUp(room);
            _roomListItems.Add(roomListItem);
            _statRooms++;
            _statPlayers += room.PlayerCount;
            roomRect.sizeDelta -= Vector2.down * 100;
        }

        _roomCountInLobby.text = _statRooms.ToString();
        _playerCountInLobby.text = _statPlayers.ToString();
    }

    public void SortRoomsByName() 
    {
        if (_sortRoomByName)
        { // По возрастанию (название)
            _currentRoomList = _currentRoomList.OrderBy(room => room.Name).ToList();
            UpdateRoomList(_currentRoomList);
            _sortRoomByName = false;
            DebugText("Sorting by Name (ascending)");
        }
        else
        { // По убыванию (название)
            _currentRoomList = _currentRoomList.OrderByDescending(room => room.Name).ToList();
            UpdateRoomList(_currentRoomList);
            _sortRoomByName = true;
            DebugText("Sorting by Name (descending)");
        }
    }

    public void SortRoomsByPlayerCount() 
    {
        if (_sortRoomPyPlayerCount)
        { // По возрастанию (игроки)
            _currentRoomList = _currentRoomList.OrderBy(room => room.PlayerCount).ToList();
            UpdateRoomList(_currentRoomList);
            _sortRoomPyPlayerCount = false;
            DebugText("Sorting by number of Players (ascending)");
        }
        else
        { // По убыванию (игроки)
            _currentRoomList = _currentRoomList.OrderByDescending(room => room.PlayerCount).ToList();
            UpdateRoomList(_currentRoomList);
            _sortRoomPyPlayerCount = true;
            DebugText("Sorting by Number of Players (descending)");
        }
    }

    #endregion

    #region События с комнатами

    public void CreateRoom()
    {
        string roomName = _roomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            _failedText.text = "Не  удалось  создать  комнату";
            _failedCauseText.text = $"Причина:  Не  указанно  название  комнаты";
            MenuManager.instance.OpenPanel("joinFailed");
            _failedEvent = "createRoom";

            DebugText("Room name is required.");
            return;
        }

        if (!byte.TryParse(_maxPlayersInputField.text, out byte maxPlayers))
        {
            _failedText.text = "Не  удалось  создать  комнату";
            _failedCauseText.text = $"Причина:  Не  указанно  количество  игроков";
            MenuManager.instance.OpenPanel("joinFailed");
            _failedEvent = "createRoom";

            DebugText("Invalid max players value.");
            return;
        }

        MenuManager.instance.OpenMenu("loading");
        MenuManager.instance.OpenPanel("");

        RoomOptions roomOptions = new()
        {
            MaxPlayers = maxPlayers
        };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
        DebugText($"Creating room: {roomName} with max players: {maxPlayers}...");
    }

    public override void OnCreatedRoom()
    {
        MenuManager.instance.OpenMenu("lobby");
        DebugText($"Room created: {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        MenuManager.instance.OpenMenu("lobby");

        _failedText.text = "Не  удалось  создать  комнату";
        _failedCauseText.text = $"Причина:  {message}";
        MenuManager.instance.OpenPanel("joinFailed");
        _failedEvent = "";

        DebugText($"Create room failed: {message}");
    }

    public void JoinToRandomRoom()
    {
        MenuManager.instance.OpenMenu("loading");
        PhotonNetwork.JoinRandomRoom();
        DebugText("...Join to RandomRoom...");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        MenuManager.instance.OpenMenu("lobby");

        _failedText.text = "Не  удалось  подключиться  в  случайную  комнату";
        _failedCauseText.text = $"Причина:  {message}";
        MenuManager.instance.OpenPanel("joinFailed");
        _failedEvent = "";

        DebugText($"Join to RandomRoom failed: {message}");
    }

    public void JoinToRoom(string name)
    {
        MenuManager.instance.OpenMenu("loading");
        PhotonNetwork.JoinRoom(name);
        DebugText("...Join to Room...");
    }

    public void FindRoomAndJoin()
    {
        MenuManager.instance.OpenMenu("loading");
        MenuManager.instance.OpenPanel("");
        PhotonNetwork.JoinRoom(_findRoomNameInputField.text);
        DebugText("...Find Room...");
    }

    public void LeaveFromRoom()
    {
        MenuManager.instance.OpenMenu("loading");
        PhotonNetwork.LeaveRoom();
        LeaveFromLobby();
        DebugText("...Disconnecting from Room...");
        LoadMenuScene();
    }

    public override void OnJoinedRoom()
    {
        DebugText($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
        _inRoom = true;
        _currentIsInRoom = true;
        LoadGameScene();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        MenuManager.instance.OpenMenu("lobby");

        _failedText.text = "Не  удалось  подключиться  в  комнату";
        _failedCauseText.text = $"Причина:  {message}";
        MenuManager.instance.OpenPanel("joinFailed");
        _failedEvent = "";

        DebugText($"Join to Room failed: {message}");
    }

    public override void OnLeftRoom()
    {
        MenuManager.instance.OpenMenu("main");
        DebugText("Disconnected from Room!");
        _inRoom = false;
        _currentIsInRoom = false;
    }

    public void LoadGameScene()
    {
        DebugText("...Loading GAME Scene...");
        PhotonNetwork.LoadLevel(_loadGameSceneGame);
    }

    public void LoadMenuScene()
    {
        DebugText("...Loading MENU Scene...");
        PhotonNetwork.LoadLevel(_loadGameSceneMenu);
        UIManagerVBS.instance.CursorEnable(true);
    }

    public void OKButtonOnFailed()
    {
        MenuManager.instance.OpenPanel(_failedEvent);
        if (!PhotonNetwork.InLobby)
        {
            MenuManager.instance.OpenMenu("loading");
            PhotonNetwork.JoinLobby();
        }
    }

    public void CreateTESTRoom()
    {
        PhotonNetwork.CreateRoom("Test VBS");
        MenuManager.instance.OpenMenu("loading");
    }  // TEST

    public bool InRoom()
    {
        return _inRoom;
    }

    public bool CurrentIsInRoom()
    {
        return _currentIsInRoom;
    }

    #endregion

    #region Другое

    public void SetNickName(string name)
    {
        PhotonNetwork.NickName = name;
    }

    #endregion

    #region Debug

    public void DebugText(string text)
    {
        GameObject currentLog = Instantiate(debugTextPrefab, debugContent);
        var textComponent = currentLog.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;

        debugRect.sizeDelta -= Vector2.down * 100;
    }

    #endregion
}