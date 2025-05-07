using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlotInventoryVBS : MonoBehaviour
{
    #region Переменные

    [Header("|-# COMPONENTS")]
    [SerializeField] private InventoryManagerVBS _inventoryManager;
    [SerializeField] private UIManagerVBS _uiManager;
    private RPC_PlayerVBS _rpc_player;
    private Animator _playerAnimator;
    private PlayerInteractionVBS _playerInteraction;
    private BuilderVBS _builder;
    private ThrowingVBS _throwing;
    private Transform _quickslotContainer;
    private int _currentSlotID;
    [SerializeField] private Sprite _defaultSelectedSprite;
    [SerializeField] private Sprite _selectedSprite;

    private Transform _instrumentsHandR;
    private GameObject _currentObject;

    private Slot activeSlot;

    private Slot[] _fastSlots;
    private Image[] _imageSlots;

    #endregion

    #region Start/Update

    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        InputScrollWheel();
    }

    #endregion

    #region Инициализация

    private void InitializeComponents()
    {
        _quickslotContainer = transform;

        int childCount = _quickslotContainer.childCount;
        _imageSlots = new Image[childCount];
        _fastSlots = new Slot[childCount];

        for (int i = 0; i < childCount; i++)
        {
            _imageSlots[i] = _quickslotContainer.GetChild(i).GetComponent<Image>();
            _fastSlots[i] = _quickslotContainer.GetChild(i).GetComponent<Slot>();
        }
    }

    public void SetPlayerComponents(GameObject player)
    {
        _playerInteraction = player.GetComponent<PlayerInteractionVBS>();
        _builder = player.GetComponent<BuilderVBS>();
        _rpc_player = player.GetComponent<RPC_PlayerVBS>();
        _throwing = player.GetComponent<ThrowingVBS>();
    }

    public void SetBodyDataComponent(BodyDataVBS bodyData)
    {
        _playerAnimator = bodyData.animator;
        _instrumentsHandR = bodyData.itemInRightHand;
    }

    #endregion

    #region Ввод от колёсика мыши

    private void InputScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.05f)
        {
            _imageSlots[_currentSlotID].sprite = _defaultSelectedSprite;

            if (scroll > 0.05f)
            {
                _currentSlotID = (_currentSlotID - 1 + _quickslotContainer.childCount) % _quickslotContainer.childCount;
            }
            if (scroll < -0.05f)
            {
                _currentSlotID = (_currentSlotID + 1) % _quickslotContainer.childCount;
            }

            _imageSlots[_currentSlotID].sprite = _selectedSprite;

            activeSlot = _fastSlots[_currentSlotID];
            CreateItemInHand();
        }
    }

    #endregion

    #region Изменение и назначение активного слота

    public void HandleSlotSelection(int newSlotID)
    {
        if (_currentSlotID == newSlotID)
        {
            ToggleSlotSelection();
        }
        else
        {
            _imageSlots[_currentSlotID].sprite = _defaultSelectedSprite;
            _currentSlotID = newSlotID;
            _imageSlots[_currentSlotID].sprite = _selectedSprite;
            activeSlot = _fastSlots[_currentSlotID];
            CreateItemInHand();
        }
    }

    private void ToggleSlotSelection()
    {
        Image currentSlotImage = _imageSlots[_currentSlotID];
        if (currentSlotImage.sprite == _defaultSelectedSprite)
        {
            currentSlotImage.sprite = _selectedSprite;
            activeSlot = _fastSlots[_currentSlotID];
            CreateItemInHand();
        }
        else
        {
            currentSlotImage.sprite = _defaultSelectedSprite;
            activeSlot = null;
            DestroyItemInHand();
        }
    }

    #endregion

    #region Использовать/создать в руке/удалить в руке - предмет

    public void CheckItemInHand()
    {
        if (!LauncherVBS.instance.CurrentIsInRoom())
            return;

        if (activeSlot)
            CreateItemInHand();
        else
            DestroyItemInHand();
    }

    public void UseItem()
    {
        if (!LauncherVBS.instance.CurrentIsInRoom())
            return;

        if (activeSlot && activeSlot.itemSO && !_inventoryManager.isOpen && !_builder.isBuilding)
        {
            if (activeSlot.itemSO.itemType == ItemType.Default)
            {


                return;
            }
            if (activeSlot.itemSO.itemType == ItemType.Consumable)
            {
                _uiManager.ChangeCharacteristics(activeSlot.itemSO.changeHealth, activeSlot.itemSO.changeHunger, activeSlot.itemSO.changeThirst);

                if (activeSlot.amount <= 1)
                {
                    activeSlot.GetComponentInChildren<DragAndDropItemVBS>().NullifySlotData();
                    CheckItemInHand();
                }
                else
                {
                    activeSlot.amount--;
                    activeSlot.amountText.text = activeSlot.amount.ToString();
                }

                return;
            }
            if (activeSlot.itemSO.itemType == ItemType.Weapon)
            {


                return;
            }
            if (activeSlot.itemSO.itemType == ItemType.Throuable)
            {
                _uiManager.OnThrowButtonClick();

                return;
            }
            if (activeSlot.itemSO.itemType == ItemType.Instrument)
            {
                _playerAnimator.SetBool("Hit", true);

                return;
            }
            if (activeSlot.itemSO.itemType == ItemType.Build)
            {
                _uiManager.OnBuildModeButtonClick();
                _uiManager.OnBuildSelectedClick(activeSlot.itemSO.IDBuild);

                if (activeSlot.amount <= 1)
                {
                    activeSlot.GetComponentInChildren<DragAndDropItemVBS>().NullifySlotData();
                }
                else
                {
                    activeSlot.amount--;
                    activeSlot.amountText.text = activeSlot.amount.ToString();
                }

                return;
            }
        }
    }

    public void StopUseItem()
    {
        _playerAnimator.SetBool("Hit", false);
    }

    public void Hit()
    {
        if (activeSlot)
            _playerInteraction.Hit(activeSlot.itemSO);
    }

    private void CreateItemInHand()
    {
        DestroyItemInHand();
        if (activeSlot && activeSlot.itemSO)
        {
            if (activeSlot.itemSO.HandPrefab)
            {
                _currentObject = PhotonNetwork.Instantiate(activeSlot.itemSO.HandPrefab.name, _playerInteraction.transform.position, Quaternion.identity);
                _rpc_player.SetParent(RpcTarget.AllBuffered, _currentObject, _instrumentsHandR.gameObject);
                _rpc_player.ApplyOriginalTransform(RpcTarget.AllBuffered, _currentObject, activeSlot.itemSO.SpawnPosition, activeSlot.itemSO.SpawnRotation, activeSlot.itemSO.SpawnScale);

                if (activeSlot.itemSO.itemType == ItemType.Instrument)
                {
                    _playerAnimator.SetLayerWeight(1, 0.5f);
                    _uiManager.SetActiveMAINButton(0, true);

                    return;
                }
                if (activeSlot.itemSO.itemType == ItemType.Throuable)
                {
                    _throwing.currentPrefab = activeSlot.itemSO.throuablePrefab;
                    _throwing.force = activeSlot.itemSO.throuableForce;
                    _throwing.upForce = activeSlot.itemSO.throuableUpForce;
                    _throwing.reloadTime = activeSlot.itemSO.reloadTime;
                    _uiManager.SetActiveMAINButton(1, true);

                    return;
                }
            }
            if (activeSlot.itemSO.itemType == ItemType.Consumable)
            {
                _uiManager.SetActiveMAINButton(0, true);

                return;
            }
        }
    }

    private void DestroyItemInHand()
    {
        if (!LauncherVBS.instance.CurrentIsInRoom())
            return;

        _playerAnimator.SetBool("Hit", false);
        if (_currentObject) PhotonNetwork.Destroy(_currentObject);
        _playerAnimator.SetLayerWeight(1, 0f);
        _uiManager.SetActiveMAINButton(0, false);
        _uiManager.SetActiveMAINButton(1, false);
        _uiManager.shootButtons[0].SetActive(false);
        _uiManager.shootButtons[1].SetActive(false);
    }

    #endregion
}