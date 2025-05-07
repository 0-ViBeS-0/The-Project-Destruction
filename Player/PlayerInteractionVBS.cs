using TMPro;
using UnityEngine;

public class PlayerInteractionVBS : MonoBehaviour
{
    #region Переменные

    [Header("|-# COMPONENTS")]
    [SerializeField] private Transform _cameraTransform;
    private RPC_PlayerVBS _rpc_player;
    private PlayerCharacteristicsVBS _MYplayerCharacteristics;
    private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _hitSounds;

    [Header("|-# Interact RAY")]
    [SerializeField] private float _interactionRayDistance;
    private RaycastHit _hitinfo;
    [SerializeField] private LayerMask _interactionLayer;

    [HideInInspector] public GameObject[] interactionUI;
    [HideInInspector] public TMP_Text interactionText;
    private bool _buttonTrigger = false;

    [Header("|-# Gather RAY")]
    [SerializeField] private float _gatherRayDistance;
    private RaycastHit _hitinfoGather;
    [SerializeField] private LayerMask _gatherLayer;

    #endregion

    #region Awake/FixedUpdate

    private void Awake()
    {
        _rpc_player = GetComponent<RPC_PlayerVBS>();
    }

    private void Start()
    {
        _MYplayerCharacteristics = GetComponent<PlayerCharacteristicsVBS>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        InteractableRayPhysics();
    }

    #endregion

    #region Взаимодействие

    private void InteractableRayPhysics()
    {
        Ray ray = new(_cameraTransform.position, _cameraTransform.forward);

        if (Physics.Raycast(ray, out _hitinfo, _interactionRayDistance, _interactionLayer))
        {
            EventInteraction interact = _hitinfo.collider.GetComponent<EventInteraction>();
            if (interact)
            {
                interactionText.text = interact.GetDescription();
                if (_buttonTrigger)
                    interact.Interact();
            }

            Item item = _hitinfo.collider.GetComponent<Item>();
            if (item)
            {
                interactionText.text = "Подобрать";
                if (_buttonTrigger)
                {
                    if (!item.isPickUped)
                    {
                        item.PickUp();
                        UIManagerVBS.instance.AddItemInInventory(item.itemSO, item.amount);
                    }
                }
            }
            UpdateUI(interact || item);
        }
        else
        {
            UpdateUI(false);
        }
        _buttonTrigger = false;
    }

    private void UpdateUI(bool isVisible)
    {
        foreach (GameObject ui in interactionUI)
            ui.SetActive(isVisible);
        interactionText.gameObject.SetActive(isVisible);
    }

    public void InteractableButton()
    {
        _buttonTrigger = true;
    }

    #endregion

    public void Hit(ItemSO itemSO)
    {
        if (_hitSounds != null) PlaySound(_hitSounds[Random.Range(0, _hitSounds.Length)]);

        Ray ray = new(_cameraTransform.position, _cameraTransform.forward);

        if (Physics.Raycast(ray, out _hitinfoGather, _gatherRayDistance, _gatherLayer))
        {
            ResourceVBS resource = _hitinfoGather.collider.GetComponent<ResourceVBS>();
            if (resource && itemSO)
            {
                foreach (ItemSO instrument in resource.instrumentsForGather)
                {
                    if (itemSO == instrument)
                    {
                        resource.Damage(itemSO.Damage);
                        UIManagerVBS.instance.AddItemInInventory(resource.resource, itemSO.Damage);
                        if (GameData.particles) Instantiate(resource.GetHitFX(), _hitinfoGather.point, Quaternion.identity).transform.LookAt(_cameraTransform);
                    }
                }
            }

            PlayerCharacteristicsVBS characteristics = _hitinfoGather.collider.GetComponent<PlayerCharacteristicsVBS>();
            if (characteristics && itemSO && characteristics != _MYplayerCharacteristics)
            {
                characteristics.InflictDamage(itemSO.Damage);
            }
        }

    }

    private void PlaySound(AudioClip randomClip)
    {
        _audioSource.clip = randomClip;
        _audioSource.Play();
    }
}