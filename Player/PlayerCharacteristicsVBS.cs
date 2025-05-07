using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacteristicsVBS : MonoBehaviour
{
    #region Переменные

    private PlayerControllerVBS _playerController;
    private PhotonView _photonView;

    [HideInInspector] public Image bloodBG;
    public float currentHealth;
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _secondsHealth;
    [HideInInspector] public TMP_Text healthText;
    [HideInInspector] public Image lineHealth;
    private float _currentGhostHealth;
    [HideInInspector] public Image lineGhostHealth;
    [SerializeField] private float _speedForLineHealth;
    [SerializeField] private Color _lifeHealthColor;
    [SerializeField] private Color _shotHealthColor;
    [Space(10)]
    public float currentHunger;
    [SerializeField] private float _maxHunger;
    [SerializeField] private float _secondsHunger;
    [HideInInspector] public TMP_Text hungerText;
    [HideInInspector] public Image lineHunger;
    [Space(10)]
    public float currentThirst;
    [SerializeField] private float _maxThirst;
    [SerializeField] private float _secondsThirst;
    [HideInInspector] public TMP_Text thirstText;
    [HideInInspector] public Image lineThirst;

    #endregion

    #region Awake/STart/FixedUpdate

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        _playerController = GetComponent<PlayerControllerVBS>();

        currentHealth = _maxHealth;
        _currentGhostHealth = _maxHealth;
        currentHunger = _maxHunger;
        currentThirst = _maxThirst;
    }

    private void FixedUpdate()
    {
        if (_photonView.IsMine)
        {
            LifeCicle();
            FollowerLine();

            //if (Input.GetMouseButtonDown(1)) InflictDamage(Random.Range(1f, 70f));
            //if (Input.GetKey(KeyCode.L)) StandUp();
        }
    }

    #endregion

    private void LifeCicle()
    {
        if (_photonView.IsMine)
        {
            if (!_playerController.isShoot)
            {
                if (currentHealth > 0f)
                {
                    if (currentHunger <= 0f)
                    {
                        InflictDamage(100f / _secondsHealth * Time.deltaTime);
                    }
                    if (currentThirst <= 0f)
                    {
                        InflictDamage(150f / _secondsHealth * Time.deltaTime);
                    }
                    if (_currentGhostHealth < 100f && currentHunger > 0f && currentThirst > 0f)
                    {
                        _currentGhostHealth += 100f / _secondsHealth * Time.deltaTime;
                        SetUI(null, lineGhostHealth, _currentGhostHealth, _maxHealth);
                    }
                }
                if (currentHunger > 0f)
                {
                    currentHunger -= 100f / _secondsHunger * Time.deltaTime;
                    SetUI(hungerText, lineHunger, currentHunger, _maxHunger);
                }
                if (currentThirst > 0f)
                {
                    currentThirst -= 100f / _secondsThirst * Time.deltaTime;
                    SetUI(thirstText, lineThirst, currentThirst, _maxThirst);
                }
            }
            else
            {
                InflictDamage(200f / _secondsHealth * Time.deltaTime);
            }
        }
    }

    private void FollowerLine()
    {
        if (_photonView.IsMine)
        {
            if (currentHealth != _currentGhostHealth)
            {
                float updatedHealth = Mathf.MoveTowards(currentHealth, _currentGhostHealth, _speedForLineHealth * Time.deltaTime);
                lineHealth.fillAmount = currentHealth / _maxHealth;
                healthText.text = ((int)currentHealth).ToString();

                UpdateHealth(updatedHealth);
            }
        }
    }

    public void InflictDamage(float damage)
    {
        StartCoroutine(FadeOut((100 - (currentHealth - damage)) / 100));
        RemoveHealth((float)damage);
        SetUI(null, lineGhostHealth, _currentGhostHealth, _maxHealth);
        SetUI(healthText, lineHealth, currentHealth, _maxHealth);
    }

    public IEnumerator FadeOut(float startAlpha)
    {
        Color color = bloodBG.color;
        color.a = startAlpha;
        bloodBG.color = color;

        while (bloodBG.color.a > 0.01f)
        {
            color.a = Mathf.MoveTowards(bloodBG.color.a, 0, 0.05f * Time.deltaTime);
            bloodBG.color = color;

            yield return null;
        }

        color.a = 0;
        bloodBG.color = color;
    }

    private void Shoot()
    {
        if (_photonView.IsMine)
        {
            _playerController.Shoot();
            UIManagerVBS.instance.Shoot();
            lineHealth.color = _shotHealthColor;
            SetHealth(_maxHealth);
        }
    }

    public void StandUp()
    {
        _playerController.StandUp();
        lineHealth.color = _lifeHealthColor;
        SetHealth(_maxHealth / 10);
    }

    public void ChangeCharacteristics(float health, float hunger, float thirst)
    {
        if (_photonView.IsMine)
        {
            if (_currentGhostHealth + health <= _maxHealth) _currentGhostHealth += health; else _currentGhostHealth = _maxHealth;
            SetUI(null, lineGhostHealth, _currentGhostHealth, _maxHealth);

            if (currentHunger + hunger <= _maxHunger) currentHunger += hunger; else currentHunger = _maxHunger;
            SetUI(hungerText, lineHunger, currentHunger, _maxHunger);

            if (currentThirst + thirst <= _maxThirst) currentThirst += thirst; else currentThirst = _maxThirst;
            SetUI(thirstText, lineThirst, currentThirst, _maxThirst);
        }
    }

    private void SetUI(TMP_Text text, Image line, float value, float maxValue)
    {
        if (text) text.text = ((int)value).ToString();
        if (line) line.fillAmount = value / maxValue;
    }

    #region Photon_RPC

    private void SetHealth(float value)
    {
        _photonView.RPC("RPC_SetHealth", RpcTarget.AllBuffered, value);
    }

    private void UpdateHealth(float health)
    {
        _photonView.RPC("RPC_UpdateHealth", RpcTarget.AllBuffered, health);
    }

    private void RemoveHealth(float damage)
    {
        _photonView.RPC("RPC_RemoveHealth", RpcTarget.AllBuffered, damage);
    }

    [PunRPC]
    private void RPC_SetHealth(float value)
    {
        _currentGhostHealth = value;
        currentHealth = value;
    }

    [PunRPC]
    private void RPC_UpdateHealth(float health)
    {
        currentHealth = health;

        if (currentHealth > 100f)
        {
            currentHealth = 100f;
        }
    }

    [PunRPC]
    private void RPC_RemoveHealth(float damage)
    {
        _currentGhostHealth -= damage;
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            if (!_playerController.isShoot) Shoot(); else _playerController.Dead();
        }
    }

    #endregion
}