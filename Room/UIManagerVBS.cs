using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.EventSystems;
using System.Collections.Generic;

#region Структуры

[System.Serializable]
public struct KeyCodesData
{
    public KeyCode CurrentKeyCode;
    public KeyCode DefaultKeyCode;
    public Button ChangeButton;
}

#endregion

public class UIManagerVBS : MonoBehaviour
{
    #region Переменные

    [Header("|-# SINGLETON")]
    public static UIManagerVBS instance;

    [Header("|-# COMPONENTS")]
    [SerializeField] private InventoryManagerVBS _inventoryManager;
    [SerializeField] private QuickSlotInventoryVBS _quickSlotInventory;
    [SerializeField] private CameraControllerMobile _cameraController;
    [SerializeField] private CompassVBS _compass;
    [SerializeField] private TacticalMarkersVBS _tacticalMarkers;
    private RPC_PlayerVBS _rpc_player;
    private PlayerControllerVBS _playerController;
    private BuilderVBS _builder;
    private PlayerInteractionVBS _playerInteraction;
    private PlayerCharacteristicsVBS _playerCharacteristics;
    private ThrowingVBS _throwing;

    [Header("|-# UI")]
    [SerializeField] private GameObject _UIGameConrollers;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _UIMobile;
    [SerializeField] private VariableJoystick _joystick;
    [SerializeField] private GameObject _chat;
    [SerializeField] private GameObject[] _MAINButtons;
    public GameObject[] shootButtons;
    [SerializeField] private GameObject[] _runButtons;
    private bool _run;
    [SerializeField] private GameObject[] _crouchButtons;
    private bool _crouch;
    [SerializeField] private GameObject[] _crawlButtons;
    private bool _crawl;
    [SerializeField] private GameObject _inputFastSlots;

    private bool _isPaused;

    [Header("|-# FPS/PING")]
    [SerializeField] private TMP_Text _fpsText;
    [SerializeField] private TMP_Text _pingText;
    private float _deltaTime = 0.0f;

    [Header("|-# BUILDING")]
    [SerializeField] private List<BuildDataSO> _build;
    [SerializeField] private Image[] _buildsButtons;
    [SerializeField] private GameObject[] _buildButtons;
    [SerializeField] private GameObject _buildPanel;
    [SerializeField] private Color _selectedButtonColor;
    [SerializeField] private Color _normalButtonColor;

    private Image[] _categoriesButtons;
    private GameObject[] _categoriesPanels;
    private TMP_Text _buildInfo;

    [Header("|-# Builder")]
    [SerializeField] private RectTransform _buildInfoPanel;
    [SerializeField] private RectTransform _buildSettingsPanel;

    [Header("|-# PlayerInteraction")]
    [SerializeField] private GameObject[] _interactionUI;
    [SerializeField] private TMP_Text _interactionText;

    [Header("|-# PlayerCharacteristics")]
    [SerializeField] private Image _bloodBG;
    [SerializeField] private RectTransform _playerCharacteristicPanel;

    [Header("|-# INPUT")]
    [SerializeField] private KeyCodesData[] _keyCode;
    private string _waitingForKey = "";
    private Button _currentButton;

    #endregion

    #region Awake/Start/Update

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

        _chat.SetActive(false);
        _UIMobile.SetActive(false);
        _pausePanel.SetActive(false);
    }

    private void Start()
    {
        InitializeComponents();
        LoadKeyCodes();
        InitializeKeyCodes();
    }

    private void Update()
    {
        FpsAndPing();
        AssignmentButton();
    }

    #endregion

    #region Инициализация

    private void InitializeComponents()
    {
        GameData.joystick = _joystick;

        int panelCount = _buildPanel.transform.GetChild(0).childCount;

        _categoriesButtons = new Image[panelCount];
        _categoriesPanels = new GameObject[panelCount];

        for (int i = 0; i < panelCount; i++)
        {
            _categoriesButtons[i] = _buildPanel.transform.GetChild(0).GetChild(i).GetComponent<Image>();
            _categoriesPanels[i] = _buildPanel.transform.GetChild(1).GetChild(i).gameObject;
        }
        _buildInfo = _buildPanel.transform.GetChild(1).GetChild(panelCount).GetComponent<TMP_Text>();
    }

    public void AssignPlayer(GameObject player)
    {
        _rpc_player = player.GetComponent<RPC_PlayerVBS>();
        _playerController = player.GetComponent<PlayerControllerVBS>();
        _builder = player.GetComponent<BuilderVBS>();
        _playerInteraction = player.GetComponent<PlayerInteractionVBS>();
        _playerCharacteristics = player.GetComponent<PlayerCharacteristicsVBS>();
        _throwing = player.GetComponent<ThrowingVBS>();

        AssignInteraction();
        AssignBuilder();
        AssignPlayerCharacteristics();
        SetGameUI(true);
        MenuManager.instance.OpenMenu("");

        _cameraController.playerController = _playerController;
        _quickSlotInventory.SetPlayerComponents(player);
        _inventoryManager.SetDragAndDropPLAYER(player);
        _compass.player = player.transform; 
        _tacticalMarkers.cameraTransform = _playerController.GetCameraTransform();
    }

    private void AssignInteraction()
    {
        _playerInteraction.interactionUI = _interactionUI;
        _playerInteraction.interactionText = _interactionText;
    }

    private void AssignBuilder()
    {
        _builder.buildInfoPanel = _buildInfoPanel;
        _builder.buildName = _buildInfoPanel.GetChild(0).GetComponent<TMP_Text>();
        _builder.lineHP = _buildInfoPanel.GetChild(1).GetChild(0).GetComponent<Image>();
        _builder.buildHP = _buildInfoPanel.GetChild(2).GetComponent<TMP_Text>();

        _builder.buildSettingsPanel = _buildSettingsPanel;
        _builder.buildSettingsTitle = _buildSettingsPanel.GetChild(0).GetComponent<TMP_Text>();

        int startBuildButton = 1;
        for (int i = startBuildButton; i < (_buildSettingsPanel.childCount + 1 - startBuildButton); i++)
        {
            _builder.buildSettingsButtons.Add(_buildSettingsPanel.GetChild(i).GetComponent<Button>());
        }
    }

    private void AssignPlayerCharacteristics()
    {
        _playerCharacteristics.bloodBG = _bloodBG;
        _playerCharacteristics.healthText = _playerCharacteristicPanel.GetChild(1).GetChild(3).GetComponent<TMP_Text>();
        _playerCharacteristics.lineHealth = _playerCharacteristicPanel.GetChild(1).GetChild(2).GetComponent<Image>();
        _playerCharacteristics.lineGhostHealth = _playerCharacteristicPanel.GetChild(1).GetChild(1).GetComponent<Image>();

        _playerCharacteristics.hungerText = _playerCharacteristicPanel.GetChild(2).GetChild(2).GetComponent<TMP_Text>();
        _playerCharacteristics.lineHunger = _playerCharacteristicPanel.GetChild(2).GetChild(1).GetComponent<Image>();

        _playerCharacteristics.thirstText = _playerCharacteristicPanel.GetChild(3).GetChild(2).GetComponent<TMP_Text>();
        _playerCharacteristics.lineThirst = _playerCharacteristicPanel.GetChild(3).GetChild(1).GetComponent<Image>();
    }

    public void AssignBodyData(BodyDataVBS bodyData)
    {
        _quickSlotInventory.SetBodyDataComponent(bodyData);
    }

    public void InitializeSettings()
    {
        foreach (var button in _runButtons) button.SetActive(false);
        if (GameData.runButtonMode == 0) _runButtons[0].SetActive(true);
        else _runButtons[1].SetActive(true);

        foreach (var button in _crouchButtons) button.SetActive(false);
        if (GameData.crouchButtonMode == 0) _crouchButtons[0].SetActive(true);
        else _crouchButtons[1].SetActive(true);

        foreach (var button in _crawlButtons) button.SetActive(false);
        if (GameData.crawlButtonMode == 0) _crawlButtons[0].SetActive(true);
        else _crawlButtons[1].SetActive(true);

        _fpsText.gameObject.SetActive(GameData.fpsEnable);
        _pingText.gameObject.SetActive(GameData.pingEnable);
    }

    #endregion

    #region ГЛАВНЫЕ СОБЫТИЯ КНОПОК

    public void OnUseButtonDown(BaseEventData eventData)
    {
        if (!_playerController.isShoot)
        {
            _quickSlotInventory.UseItem();
        }
    }

    public void OnUseButtonUp(BaseEventData eventData)
    {
        if (!_playerController.isShoot)
        {
            _quickSlotInventory.StopUseItem();
        }
    }

    public void OnRunButtonDown(BaseEventData eventData)
    {
        if (!_playerController.isShoot)
        {
            _playerController.Run();
        }
    }

    public void OnRunButtonUp(BaseEventData eventData)
    {
        if (!_playerController.isShoot)
        {
            _playerController.RunStop();
        }
    }

    public void OnRunButtonClick()
    {
        if (!_playerController.isShoot)
        {
            _run = !_run;
            if (_run) _playerController.Run();
            else _playerController.RunStop();
        }
    }

    public void OnJumpButtonClick()
    {
        if (!_playerController.isShoot)
        {
            _playerController.Jump();
        }
    }

    public void OnCrouchButtonDown(BaseEventData eventData)
    {
        if (!_playerController.isShoot)
        {
            _playerController.Crouch();
        }
    }

    public void OnCrouchButtonUp(BaseEventData eventData)
    {
        if (!_playerController.isShoot)
        {
            _playerController.CrouchStop();
        }
    }

    public void OnCrouchButtonClick()
    {
        if (!_playerController.isShoot)
        {
            _crouch = !_crouch;
            if (_crouch) _playerController.Crouch();
            else _playerController.CrouchStop();
        }
    }

    public void OnCrawlButtonDown(BaseEventData eventData)
    {
        if (!_playerController.isShoot)
        {
            _playerController.Crawl();
        }
    }

    public void OnCrawlButtonUp(BaseEventData eventData)
    {
        if (!_playerController.isShoot)
        {
            _playerController.CrawlStop();
        }
    }

    public void OnCrawlButtonClick()
    {
        if (!_playerController.isShoot)
        {
            _crawl = !_crawl;
            if (_crawl) _playerController.Crawl();
            else _playerController.CrawlStop();
        }
    }

    public void OnSwitchCameraButtonClick()
    {
        _playerController.SwitchCamera();
    }

    public void OnChatOpenButtonClick()
    {
        _chat.SetActive(true);
        _playerController.CanRotate(false);
        if (!_playerController.isMobile)
        {
            _playerController.CursorEnable(true);
            _playerController.CanMove(false);
        }
    }

    public void OnChatCloseButtonClick()
    {
        _chat.SetActive(false);
        _playerController.CanRotate(true);
        if (!_playerController.isMobile)
        {
            _playerController.CursorEnable(false);
            _playerController.CanMove(true);
        }
    }

    public void OnPauseButtonClick()
    {
        _isPaused = true;
        MenuManager.instance.OpenMenu("");
        _UIGameConrollers.SetActive(false);
        _pausePanel.SetActive(true);
        if (!_playerController.isMobile)
            _playerController.CursorEnable(true);
        _playerController.CanRotate(false);
    }

    public void OnResumeButtonClick()
    {
        _isPaused = false;
        MenuManager.instance.OpenMenu("");
        _UIGameConrollers.SetActive(true);
        _pausePanel.SetActive(false);
        _playerController.InitializeSettings();
        if (!_playerController.isMobile)
            _playerController.CursorEnable(false);
        _playerController.CanRotate(true);
    }

    public void OnSettingsButtonClick()
    {
        SettingsManagerVBS.instance.OpenSettings();
        _UIGameConrollers.SetActive(false);
        _pausePanel.SetActive(false);
    }

    public void OnExitButtonClick()
    {
        if (!_playerController.isShoot)
        {
            _isPaused = false;
            LauncherVBS.instance.LeaveFromRoom();
            _UIGameConrollers.SetActive(false);
            _pausePanel.SetActive(false);
        }
    }

    public void OnBuildModeButtonClick()
    {
        if (!_playerController.isShoot)
        {
            if (_builder.isBuilding)
            {
                _builder.SetBuildingMode(false);
                _builder.ResetBuildForBuilding();
                EnableUI(_buildPanel, false);
                EnableUIs(_buildButtons, false);
                foreach (Image button in _buildsButtons)
                    button.color = _normalButtonColor;
                if (!_playerController.isMobile)
                    _playerController.CursorEnable(false);
                SetActiveMAINButton(3, false);
            }
            else
            {
                _builder.SetBuildingMode(true);
                EnableUI(_buildPanel, true);
                SelectCategory(0);
                if (!_playerController.isMobile)
                    _playerController.CursorEnable(true);

                EnableUIs(_buildButtons, true);
                SetActiveMAINButton(3, true);
            }
        }
    }

    public void OnRotateSelectedBuildButtonClick()
    {
        _builder.RotateSelectedBuild();
    }

    public void OnBuildSelectedClick(int index)
    {
        if (_builder.isMovedBuild)
        {
            _builder.isMovedBuild = false;
            _builder._thisMovedBuild = null;
        }

        if (_buildsButtons[index].color == _selectedButtonColor)
        {
            OnBuildDeselectClick();
            return;
        }

        if (_builder.isBuilding)
            _builder.SelectBuildForBuilding(_build[index]);
        _buildInfo.text = _build[index].Name + " " + _build[index].Resources;

        foreach (Image button in _buildsButtons)
            button.color = _normalButtonColor;
        _buildsButtons[index].color = _selectedButtonColor;

        if (!_playerController.isMobile)
            _playerController.CursorEnable(false);
    }

    private void OnBuildDeselectClick()
    {
        _builder.ResetBuildForBuilding();
        foreach (Image button in _buildsButtons)
            button.color = _normalButtonColor;
        if (!_playerController.isMobile)
            _playerController.CursorEnable(true);
    }

    public void OnBuildSelectedBuildButtonClick()
    {
        _builder.BuildSelectedBuild();
    }

    public void OnRotateCurrentBuildButtonClick()
    {
        _builder.RotateCurrentBuild();
    }

    public void OnMoveCurrentBuildClick()
    {
        _builder.MoveCurrentBuild();
    }

    public void OnDestroyCurrentButtonClick()
    {
        _builder.DestroyCurrentBuild();
    }

    public void OnInteractionButtonClick()
    {
        if (!_playerController.isShoot)
        {
            _playerInteraction.InteractableButton();
        }
    }

    public void OnSetActiveInventoryButtonClick()
    {
        if (_inventoryManager.isOpen)
        {
            _inventoryManager.SetInventoryActive(false);
            if (!_playerController.isMobile)
                _playerController.CursorEnable(false);
            else
                SetActiveUIMobile(true);
            _inputFastSlots.SetActive(true);
        }
        else
        {
            _inventoryManager.SetInventoryActive(true);
            if (!_playerController.isMobile)
                _playerController.CursorEnable(true);
            else
                SetActiveUIMobile(false);
            _inputFastSlots.SetActive(false);
        }
    }

    public void OnItemSelectedClick(int index)
    {
        _quickSlotInventory.HandleSlotSelection(index);
    }

    public void OnThrowButtonClick()
    {
        _throwing.Throw();
    }

    #endregion

    #region Управление ПК

    public void InputPC()
    {
        if (_isPaused)
            return;

        // Controller

        if (GameData.runButtonMode == 0)
        {
            if (Input.GetKeyDown(_keyCode[0].CurrentKeyCode)) // [0]
                _playerController.Run();
            if (Input.GetKeyUp(_keyCode[0].CurrentKeyCode)) // [0]
                _playerController.RunStop();
        }
        else
        {
            if (Input.GetKeyDown(_keyCode[0].CurrentKeyCode)) // [0]
                OnRunButtonClick();
        }

        if (Input.GetKeyDown(_keyCode[1].CurrentKeyCode)) // [1]
            _playerController.Jump();

        if (GameData.crouchButtonMode == 0)
        {
            if (Input.GetKeyDown(_keyCode[2].CurrentKeyCode)) // [2]
                _playerController.Crouch();
            if (Input.GetKeyUp(_keyCode[2].CurrentKeyCode)) // [2]
                _playerController.CrouchStop();
        }
        else
        {
            if (Input.GetKeyDown(_keyCode[2].CurrentKeyCode)) // [2]
                OnCrouchButtonClick();
        }

        if (GameData.crawlButtonMode == 0)
        {
            if (Input.GetKeyDown(_keyCode[3].CurrentKeyCode)) // [3]
                _playerController.Crawl();
            if (Input.GetKeyUp(_keyCode[3].CurrentKeyCode)) // [3]
                _playerController.CrawlStop();
        }
        else
        {
            if (Input.GetKeyDown(_keyCode[3].CurrentKeyCode)) // [3]
                OnCrawlButtonClick();
        }

        if (Input.GetKeyUp(_keyCode[4].CurrentKeyCode)) // [4]
            _playerController.SwitchCamera();

        if (Input.GetKey(_keyCode[5].CurrentKeyCode)) // [5]
            OnChatOpenButtonClick();

        if (Input.GetKeyDown(_keyCode[6].CurrentKeyCode)) // [6]
            OnPauseButtonClick();

        // Build

        if (Input.GetKeyDown(_keyCode[7].CurrentKeyCode)) // [7]
            OnBuildModeButtonClick();

        if (Input.GetKeyDown(_keyCode[8].CurrentKeyCode)) // [8]
            OnRotateSelectedBuildButtonClick();

        if (Input.GetKeyDown(_keyCode[9].CurrentKeyCode)) // [9]
            OnDestroyCurrentButtonClick();

        if (Input.GetKeyDown(_keyCode[10].CurrentKeyCode)) // [10]
            OnBuildDeselectClick();

        if (Input.GetMouseButtonDown(0))
            OnBuildSelectedBuildButtonClick();

        if (Input.GetKeyDown(KeyCode.K))
            OnMoveCurrentBuildClick();

        // Interact

        if (Input.GetKeyDown(KeyCode.E))
            OnInteractionButtonClick();

        // FastSlots

        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                OnItemSelectedClick(i);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!_playerController.isShoot)
            {
                _quickSlotInventory.UseItem();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!_playerController.isShoot)
            {
                _quickSlotInventory.StopUseItem();
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            OnSetActiveInventoryButtonClick();
        }

        // Throwing

        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnThrowButtonClick();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            _tacticalMarkers.PlaceMarker();
        }
    }

    #endregion

    #region Изменение назначения клавиш

    private void InitializeKeyCodes()
    {
        for (int i = 0; i < _keyCode.Length; i++)
            _keyCode[i].ChangeButton.GetComponentInChildren<TMP_Text>().text = _keyCode[i].CurrentKeyCode.ToString();
    }

    public void SetDefaultKeyCodes()
    {
        for (int i = 0; i < _keyCode.Length; i++)
        {
            _keyCode[i].CurrentKeyCode = _keyCode[i].DefaultKeyCode;
            SaveKeyCode("KeyCode" + i, _keyCode[i].DefaultKeyCode.ToString());
            _keyCode[i].ChangeButton.GetComponentInChildren<TMP_Text>().text = _keyCode[i].DefaultKeyCode.ToString();
        }
    }

    public void StartKeyAssignmentButton(Button currentButton)
    {
        _waitingForKey = "key";
        _currentButton = currentButton;

        currentButton.GetComponentInChildren<TMP_Text>().text = "Press any key...";
    }

    private void AssignmentButton()
    {
        if (!string.IsNullOrEmpty(_waitingForKey))
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        AssignKey(keyCode);
                        break;
                    }
                }
            }
        }
    }

    private void AssignKey(KeyCode keyCode)
    {
        for (int i = 0; i < _keyCode.Length; i++)
            if (_keyCode[i].ChangeButton == _currentButton)
            {
                _keyCode[i].CurrentKeyCode = keyCode;
                _currentButton.GetComponentInChildren<TMP_Text>().text = keyCode.ToString();
                SaveKeyCode("KeyCode" + i, keyCode.ToString());
            }

        _waitingForKey = "";
    }

    private void SaveKeyCode(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    private void LoadKeyCodes()
    {
        for (int i = 0; i < _keyCode.Length; i++)
        {
            if (PlayerPrefs.HasKey("KeyCode" + i))
            {
                string savedKey = PlayerPrefs.GetString("KeyCode" + i);
                if (System.Enum.TryParse(savedKey, out KeyCode savedKeyCode))
                    _keyCode[i].CurrentKeyCode = savedKeyCode;
            }
            else
                _keyCode[i].CurrentKeyCode = _keyCode[i].DefaultKeyCode;
        }
    }

    #endregion

    #region Другое

    public void SetGameUI(bool active)
    {
        _UIGameConrollers.SetActive(active);
    }

    public void SetActiveUIMobile(bool active)
    {
        _UIMobile.SetActive(active);
    }

    private void FpsAndPing()
    {
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        float fps = 1.0f / _deltaTime;
        _fpsText.text = string.Format("FPS: {0:0.}", fps);
        _pingText.text = "Ping: " + PhotonNetwork.GetPing() + " ms";
    }

    private void EnableUI(GameObject ui, bool enable)
    {
        ui.SetActive(enable);
    }

    private void EnableUIs(GameObject[] ui, bool enable)
    {
        foreach (GameObject s in ui)
            s.SetActive(enable);
    }

    /////// BUILDS
    public void SelectCategory(int index)
    {
        foreach (Image button in _categoriesButtons)
            button.color = _normalButtonColor;
        _categoriesButtons[index].color = _selectedButtonColor;

        foreach (GameObject panel in _categoriesPanels)
            panel.SetActive(false);
        _categoriesPanels[index].SetActive(true);
    }
    //////////////

    public void AddItemInInventory(ItemSO item, int amount)
    {
        _inventoryManager.AddItem(item, amount);
    }

    public void ChangeCharacteristics(int health, int hunger, int thirst)
    {
        _playerCharacteristics.ChangeCharacteristics(health, hunger, thirst);
    }

    public void Shoot()
    {
        if (_builder.isBuilding)
        {
            _builder.SetBuildingMode(false);
            _builder.ResetBuildForBuilding();
            EnableUI(_buildPanel, false);
            EnableUIs(_buildButtons, false);
            foreach (Image button in _buildsButtons)
                button.color = _normalButtonColor;
            if (!_playerController.isMobile)
                _playerController.CursorEnable(false);
        }
    }

    public QuickSlotInventoryVBS GetQuickSlotInventory()
    {
        return _quickSlotInventory;
    }

    public void SetActiveMAINButton(int index, bool enable)
    {
        _MAINButtons[index].SetActive(enable);
    }

    public BuildDataSO GetBuildData(int index)
    {
        return _build[index];
    }

    public void CursorEnable(bool enable)
    {
        Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = enable;
    }

    #endregion
}