using UnityEngine;
using Photon.Pun;
using TMPro;
using Cursor = UnityEngine.Cursor;
using Photon.Realtime;

public class PlayerControllerVBS : MonoBehaviour
{
    #region Переменные
    
    [HideInInspector] public bool isMobile;
    [HideInInspector] public VariableJoystick _joystick;

    [Header("|-# COMPONENTS")]
    [SerializeField] private Transform _cameraTargetTransform;
    [HideInInspector] public Transform _cameraHeadTargetTransform;
    [SerializeField] private Transform _cameraTransform;
    private Camera _camera;
    private Transform _playerTransform;
    private CharacterController _characterController;
    [HideInInspector] public Animator animator;
    private SkinInitializatorVBS _skinInitializator;
    private PlayerCharacteristicsVBS _characteristics;
    private UIManagerVBS _uiManager;
    [SerializeField] private PhotonView _photonView;
    [SerializeField] private TMP_Text _playerNickName;

    [Header("|-# MOVE")]
    public bool canMove = true;

    private float _speed;
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    private bool isRun;
    [SerializeField] private float _crouchSpeed;
    private bool isCrouch;
    [SerializeField] private float _crawlSpeed;
    private bool isCrawl;
    [SerializeField] private float _airControlFactor;

    public bool isShoot;

    private float _xMove;
    private float _yMove;
    private Vector3 _moveDirection;

    [Header("|-# CC [center.y/z/radius/height]")]
    [SerializeField] private float[] _standingCC = new float[4];
    [SerializeField] private float[] _runCC = new float[4];
    [SerializeField] private float[] _crouchCC = new float[4];
    [SerializeField] private float[] _crawlCC = new float[4];

    [Header("|-# GRAVITY")]
    public bool canUseGravity = true;

    [SerializeField] private float _jumpForce;
    [SerializeField] private float _gravityForce;
    [SerializeField] private float _groundedDelay;
    [SerializeField] private float _fallDamageThreshold;
    [SerializeField] private float _damageMultiplier;

    private bool _isGrounded;
    private Vector3 _velocity;
    private float _groundedTimer;
    private float _startFallHeight;
    private bool _isFalling;

    [Header("|-# CAMERA")]
    public bool canRotate = true;

    [SerializeField] private float _sensitivity;
    [SerializeField] private bool _inverseX;
    [SerializeField] private bool _inverseY;
    [Range(-90, 90)][SerializeField] private float _cameraMinY;
    [Range(-90, 90)][SerializeField] private float _cameraMaxY;

    private float _xRotate;
    private float _yRotate;
    private float _xRotation;
    private int _IDCameraPos;
    [SerializeField] private Vector3[] _cameraPosition;

    [Header("|-# IK")]
    [SerializeField] private Transform _cameraTargetIK;
    [HideInInspector] public Transform headTargetIK;

    [Header("|-# SMOOTHNESS")]
    [SerializeField] private float _smoothnessAnimationForBody;

    #endregion

    #region Awake/Start/Update/FixedUpdate

    private void Awake()
    {
        _joystick = GameData.joystick;
    }

    private void Start()
    {
        InitializeComponents();
        InitializeSettings();
    }

    private void Update()
    {
        if (!_photonView.IsMine)
            return;

        MovementInput();
        if (!isMobile)
        {
            RotateInputPC();
            UIManagerVBS.instance.InputPC();
        }

        SetTransforms(_cameraTargetTransform, _cameraHeadTargetTransform);
        SetTransforms(headTargetIK, _cameraTargetIK);
    }

    private void FixedUpdate()
    {
        if (!_photonView.IsMine)
            return;

        if (canMove) Movement();
        if (canUseGravity) Gravity();
        GroundCheck();
    }

    #endregion

    #region Инициализация

    private void InitializeComponents()
    {
        if (_photonView.IsMine)
        {
            _playerTransform = transform;
            _photonView = GetComponent<PhotonView>();
            _characteristics = GetComponent<PlayerCharacteristicsVBS>();
            _characterController = GetComponent<CharacterController>();
            _skinInitializator = GetComponent<SkinInitializatorVBS>();
            _uiManager = UIManagerVBS.instance;
            _camera = _cameraTransform.GetComponent<Camera>();

            _cameraTransform.localPosition = _cameraPosition[0];
            _photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);

            RunStop();
            _speed = _walkSpeed;

            SettingsManagerVBS.instance.InitializeOnStart(_camera);
            _skinInitializator.LoadBody();
        }

        if (!_photonView.IsMine)
        {
            enabled = false;
            _cameraTargetTransform.gameObject.SetActive(false);
            gameObject.GetComponent<PlayerInteractionVBS>().enabled = false;
            gameObject.GetComponent<BuilderVBS>().enabled = false;
        }
    }

    public void InitializeSettings()
    {
        if (!_photonView.IsMine) return;

        SwitchPlatform(GameData.controlType == 0);
        _sensitivity = GameData.sensitivity;
        _inverseX = GameData.inverseX;
        _inverseY = GameData.inverseY;
        SetJoystickType(GameData.joystickType);
        _playerNickName.gameObject.SetActive(GameData.playersNicknameVisible);
    }

    #endregion

    #region Ввод/Управление

    public void SetJoystickType(int index)
    {
        if (index >= 0 && index <= 2)
        {
            _joystick.SetMode((JoystickType)index);
        }
    }

    private void MovementInput()
    {
        if (isMobile)
        {
            if (!isRun && !isCrawl) _xMove = _joystick.Horizontal;
            if (!isRun) _yMove = _joystick.Vertical;
        }
        else
        {
            if (!isRun && !isCrawl) _xMove = Input.GetAxis("Horizontal");
            if (!isRun) _yMove = Input.GetAxis("Vertical");
        }
    }

    public void RotateInputMobile(float horizontal, float vertical)
    {
        _xRotate = horizontal * _sensitivity;
        _yRotate = vertical * _sensitivity;
        RotationMobile();
    }

    private void RotateInputPC()
    {
        _xRotate = Input.GetAxis("Mouse X") * _sensitivity;
        _yRotate = Input.GetAxis("Mouse Y") * _sensitivity;

        if (canRotate) RotationPC();
    }

    #endregion

    #region Движение/Вращение

    private void Movement()
    {
        Vector3 move = transform.right * _xMove + transform.forward * _yMove;
        if (_isGrounded)
        {
            _moveDirection = move * _speed;
        }
        else
        {
            _moveDirection += _airControlFactor * Time.deltaTime * move;
        }
        _characterController.Move(_moveDirection * Time.deltaTime);

        animator.SetFloat("x", Mathf.Lerp(animator.GetFloat("x"), _xMove, _smoothnessAnimationForBody * Time.deltaTime));
        animator.SetFloat("y", Mathf.Lerp(animator.GetFloat("y"), _yMove, _smoothnessAnimationForBody * Time.deltaTime));
    }

    public void RotationMobile()
    {
        _xRotation -= _yRotate * (_inverseY ? -1 : 1);
        _xRotation = Mathf.Clamp(_xRotation, _cameraMinY, _cameraMaxY);
        _cameraTargetTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _playerTransform.Rotate(Vector3.up * (_xRotate * (_inverseX ? -1 : 1)));
    }

    private void RotationPC()
    {
        _xRotation -= _yRotate * (_inverseY ? -1 : 1);
        _xRotation = Mathf.Clamp(_xRotation, _cameraMinY, _cameraMaxY);
        _cameraTargetTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _playerTransform.Rotate(Vector3.up * (_xRotate * (_inverseX ? -1 : 1)));
    }

    #endregion

    #region Гравитация

    private void Gravity()
    {
        _velocity.y += _gravityForce * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void GroundCheck()
    {
        _isGrounded = _characterController.isGrounded;
        animator.SetBool("isGrounded", _isGrounded);
        if (_isGrounded && _velocity.y < 0)
        {
            animator.SetBool("isInAir", false);
            _velocity.y = -2f;
            _groundedTimer = 0f;

            if (_isFalling)
            {
                float fallDistance = _startFallHeight - transform.position.y;
                if (fallDistance > _fallDamageThreshold)
                {
                    float damage = (fallDistance - _fallDamageThreshold) * _damageMultiplier;
                    _characteristics.InflictDamage(damage);
                }
                _isFalling = false;
            }
        }
        else if (!_isGrounded)
        {
            _groundedTimer += Time.deltaTime;
            if (_groundedTimer > _groundedDelay)
            {
                animator.SetBool("isInAir", true);

                if (!_isFalling)
                {
                    _startFallHeight = transform.position.y;
                    _isFalling = true;
                }
            }
        }
    }

    #endregion

    #region События управления

    public void Run()
    {
        if (!_photonView.IsMine) return;

        if (canMove)
        {
            isRun = true;
            if (!isCrouch && !isCrawl) _speed = _runSpeed;
            animator.SetBool("isRun", true);
            _yMove = 1f;
            _xMove = 0f;

            if (!isCrouch && !isCrawl)
                SetCenterRadiusHeight(_runCC[0], _runCC[1], _runCC[2], _runCC[3]);
        }
    }

    public void RunStop()
    {
        if (!_photonView.IsMine) return;

        if (isRun)
        {
            isRun = false;
            if (!isCrouch && !isCrawl) _speed = _walkSpeed;
            animator.SetBool("isRun", false);
            _yMove = 0f;
            _xMove = 0f;

            if (!isCrouch && !isCrawl)
                SetCenterRadiusHeight(_standingCC[0], _standingCC[1], _standingCC[2], _standingCC[3]);
        }
    }

    public void Jump()
    {
        if (!_photonView.IsMine) return;

        if (canMove)
        {
            if (_isGrounded && !isCrouch && !isCrawl)
            {
                _velocity.y = Mathf.Sqrt(_jumpForce * -2f * _gravityForce);
                animator.SetTrigger("Jump");
            }
        }
    }

    public void Crouch()
    {
        if (!_photonView.IsMine) return;

        if (canMove)
        {
            if (_isGrounded && !isCrawl)
            {
                isCrouch = true;
                _speed = _crouchSpeed;
                animator.SetBool("isCrouch", true);

                SetCenterRadiusHeight(_crouchCC[0], _crouchCC[1], _crouchCC[2], _crouchCC[3]);
            }
        }
    }

    public void CrouchStop()
    {
        if (!_photonView.IsMine) return;

        if (isCrouch)
        {
            isCrouch = false;
            if (!isRun) _speed = _walkSpeed; else _speed = _runSpeed;
            animator.SetBool("isCrouch", false);

            SetCenterRadiusHeight(_standingCC[0], _standingCC[1], _standingCC[2], _standingCC[3]);
        }
    }

    public void Crawl()
    {
        if (!_photonView.IsMine) return;

        if (canMove)
        {
            if (_isGrounded && !isCrouch)
            {
                isCrawl = true;
                _speed = _crawlSpeed;
                _xMove = 0f;
                animator.SetBool("isCrawl", true);

                SetCenterRadiusHeight(_crawlCC[0], _crawlCC[1], _crawlCC[2], _crawlCC[3]);
            }
        }
    }

    public void CrawlStop()
    {
        if (!_photonView.IsMine) return;

        if (isCrawl)
        {
            isCrawl = false;
            if (!isRun) _speed = _walkSpeed; else _speed = _runSpeed;
            animator.SetBool("isCrawl", false);

            SetCenterRadiusHeight(_standingCC[0], _standingCC[1], _standingCC[2], _standingCC[3]);
        }
    }

    public void Shoot()
    {
        if (!_photonView.IsMine) return;

        CrouchStop();
        RunStop();
        isShoot = true;
        isCrawl = true;
        _speed = _crawlSpeed;
        _xMove = 0f;
        animator.SetBool("isCrawl", true);

        SetCenterRadiusHeight(_crawlCC[0], _crawlCC[1], _crawlCC[2], _crawlCC[3]);
    }

    public void StandUp()
    {
        if (!_photonView.IsMine) return;

        if (isShoot)
        {
            isShoot = false;
            isCrawl = false;
            if (!isRun) _speed = _walkSpeed; else _speed = _runSpeed;
            animator.SetBool("isCrawl", false);

            SetCenterRadiusHeight(_standingCC[0], _standingCC[1], _standingCC[2], _standingCC[3]);
        }
    }

    public void Dead()
    {

    }

    #endregion

    #region Доступ к движению

    public void CanMove(bool can)
    {
        if (!_photonView.IsMine) return;
        canMove = can;
        animator.SetFloat("x", 0f);
        animator.SetFloat("y", 0f);
        CrouchStop();
        RunStop();
    }

    public void CanRotate(bool can)
    {
        if (!_photonView.IsMine) return;
        canRotate = can;
    }

    public void CanUseGravity(bool can)
    {
        if (!_photonView.IsMine) return;
        canUseGravity = can;
    }

    #endregion

    #region Другое

    private void SwitchPlatform(bool _isMobile)
    {
        if (!_photonView.IsMine) return;

        isMobile = _isMobile;
        UIManagerVBS.instance.SetActiveUIMobile(isMobile);
        if (isMobile) CursorEnable(true); else CursorEnable(false);
    }

    public void SwitchCamera()
    {
        if (!_photonView.IsMine) return;

        if (_cameraPosition.Length - 1 > _IDCameraPos) _IDCameraPos++; else _IDCameraPos = 0;
        _cameraTransform.localPosition = _cameraPosition[_IDCameraPos];
    }

    public void CursorEnable(bool enable)
    {
        if (!_photonView.IsMine) return;
        UIManagerVBS.instance.CursorEnable(enable);
    }

    private void SetCenterRadiusHeight(float centerY, float centerZ, float radius, float height)
    {
        if (!_photonView.IsMine) return;

        _characterController.center = new Vector3(0f, centerY, centerZ);
        _characterController.radius = radius;
        _characterController.height = height;
    }

    private void SetTransforms(Transform transformA, Transform transformB)
    {
        transformA.position = transformB.position;
    }

    public Transform GetCameraTransform()
    {
        return _cameraTransform;
    }

    #endregion

    #region Photon RPC

    [PunRPC]
    public void SetPlayerName(string playerName)
    {
        _playerNickName.text = playerName;
    }

    #endregion
}