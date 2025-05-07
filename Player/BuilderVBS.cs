using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuilderVBS : MonoBehaviour
{
    #region Переменные

    [HideInInspector] public bool isBuilding;
    [HideInInspector] public bool isSelectedBuild;
    [HideInInspector] public bool isMovedBuild;
    [HideInInspector] public bool canBuild;
    [HideInInspector] public bool isInPoint;

    [Header("|-# BUILD COMPONENTS")]
    [SerializeField] private Transform _cameraTransform;
    private PhotonView _photonView;
    [SerializeField] private Camera _camera;
    private RPC_PlayerVBS _rpc_player;
    private RaycastHit _hitinfoToBuild;
    [SerializeField] private float _rayDistanceBuild;
    [SerializeField] private LayerMask _buildLayerMask;
    private RaycastHit _hitBuildInfo;
    [SerializeField] private float _rayDistanceBuildInfo;
    [SerializeField] private LayerMask _buildInfoLayerMask;

    [Header("|-# TERRITORY")]
    [SerializeField] private GameObject _territoryPrefab;
    [HideInInspector] public GameObject territoryObject;
    private TerritoryManagerVBS _territoryManager;
    [HideInInspector] public bool isMyTerritory;
    private MeshRenderer _territoryMesh;


    [HideInInspector] public BuildDataSO buildData;
    private GameObject _createdPreview;
    private int _currentLocalRotation;
    private int _currentAngleRotation;
    private bool _isRotated;
    private Vector3 _spawnPositionBuild;
    private Quaternion _spawnRotationBuild;

    private GameObject _thisBuildGO;
    [HideInInspector] public GameObject _thisMovedBuild;

    [HideInInspector] public RectTransform buildInfoPanel;
    [HideInInspector] public TMP_Text buildName;
    [HideInInspector] public TMP_Text buildHP;
    [HideInInspector] public Image lineHP;

    [HideInInspector] public RectTransform buildSettingsPanel;
    [HideInInspector] public TMP_Text buildSettingsTitle;
    [HideInInspector] public List<Button> buildSettingsButtons;

    #endregion

    #region Start/FixedUpdate

    private void Start()
    {
        _rpc_player = GetComponent<RPC_PlayerVBS>();
        _photonView = GetComponent<PhotonView>();
    }

    private void FixedUpdate()
    {
        CheckBuildRayCastPhysic();
        if (isSelectedBuild && buildData != null)
            BuildRayCastPhysic();
    }

    #endregion

    #region Назначение позиции и вращения строения

    private void BuildRayCastPhysic()
    {
        Ray ray = new(_cameraTransform.position, _cameraTransform.forward);

        if (Physics.Raycast(ray, out _hitinfoToBuild, _rayDistanceBuild, _buildLayerMask))
        {
            if (_hitinfoToBuild.transform.gameObject.layer == 11)
            {
                isInPoint = true;
                _spawnPositionBuild = _hitinfoToBuild.transform.position + _hitinfoToBuild.transform.right * buildData.Offset.x + _hitinfoToBuild.transform.forward * buildData.Offset.z + _hitinfoToBuild.transform.up * buildData.Offset.y;
                _spawnRotationBuild = Quaternion.Euler(_hitinfoToBuild.transform.eulerAngles.x, _hitinfoToBuild.transform.eulerAngles.y + _currentAngleRotation, _hitinfoToBuild.transform.eulerAngles.z);
            }
            else
            {
                isInPoint = false;
                _spawnPositionBuild = _hitinfoToBuild.point;
                _spawnRotationBuild = Quaternion.Euler(0, _cameraTransform.rotation.eulerAngles.y + _currentLocalRotation, 0);
            }
        }
        else
        {
            isInPoint = false;
            _spawnPositionBuild = _cameraTransform.position + _cameraTransform.forward * _rayDistanceBuild;
            _spawnRotationBuild = Quaternion.Euler(0, _cameraTransform.rotation.eulerAngles.y + _currentLocalRotation, 0);
        }

        if (_createdPreview)
            _createdPreview.transform.SetPositionAndRotation(_spawnPositionBuild, _spawnRotationBuild);
    }

    #endregion

    #region Вкл/Выкл строительство

    public void SetBuildingMode(bool enable)
    {
        isBuilding = enable;

        if (_territoryMesh)
            _territoryMesh.enabled = enable;
    }

    #endregion

    #region Назначить данные/Сбросить данные

    public void SelectBuildForBuilding(BuildDataSO build)
    {
        if (_createdPreview)
            ResetBuildForBuilding();

        isSelectedBuild = true;
        buildData = build;
        _createdPreview = Instantiate(buildData.PreviewPrefab);
        _createdPreview.GetComponent<PreviewBuildVBS>().SetPreviewComponents(this, buildData);

        if (_territoryManager)
            foreach (GameObject buildObj in _territoryManager.builds)
                buildObj.GetComponent<BuildVBS>().BuildChanged(buildData.ID);
    }

    public void ResetBuildForBuilding()
    {
        isSelectedBuild = false;
        buildData = null;
        if (_createdPreview)
        {
            Destroy(_createdPreview);
            _createdPreview = null;
        }
        _currentAngleRotation = 0;
        _currentLocalRotation = 0;

        if (_territoryManager)
            foreach (GameObject buildObj in _territoryManager.builds)
                buildObj.GetComponent<BuildVBS>().HidePoints();
    }

    #endregion

    #region Территория

    private void CreateTerritory()
    {
        territoryObject = PhotonNetwork.Instantiate(_territoryPrefab.name, _spawnPositionBuild + (Vector3.up * 10f), Quaternion.identity);
        _territoryManager = territoryObject.GetComponent<TerritoryManagerVBS>();
        _rpc_player.InitializeTerritory(RpcTarget.AllBuffered, territoryObject);
        _territoryMesh = territoryObject.GetComponent<MeshRenderer>();
        _territoryMesh.enabled = true;
    }

    private void DestroyTerritory()
    {
        PhotonNetwork.Destroy(territoryObject);
    }

    #endregion

    #region Строить/Разрушить/Вращать

    public void BuildSelectedBuild()
    {
        if (isBuilding && isSelectedBuild && canBuild)
        {
            if (!_thisMovedBuild)
            {
                if (!territoryObject) CreateTerritory();
                GameObject build = PhotonNetwork.Instantiate(buildData.BuildPrefab.name, _spawnPositionBuild, _spawnRotationBuild);
                _rpc_player.SetParent(RpcTarget.AllBuffered, build, territoryObject);

                _rpc_player.InitializeBuild(RpcTarget.AllBuffered, build, territoryObject);
            }
            else
            {
                _thisMovedBuild.GetComponent<BuildVBS>().MoveBuild(_spawnPositionBuild, _spawnRotationBuild);
                UIManagerVBS.instance.OnBuildSelectedClick(_thisMovedBuild.GetComponent<BuildVBS>().buildData.ID);
            }
        }
    }

    public void RotateSelectedBuild()
    {
        if (isBuilding && isSelectedBuild)
        {
            if (isInPoint)
            {
                if (buildData.SwitchRotate)
                {
                    _isRotated = !_isRotated;
                    _currentAngleRotation += _isRotated ? buildData.RotateAngle : -buildData.RotateAngle;
                }
                else if (buildData.RotateAngle != 0)
                {
                    _currentAngleRotation += buildData.RotateAngle;
                }
            }
            else
            {
                if (buildData.CanRotateLocal)
                {
                    _currentLocalRotation += buildData.RotateLocalAngle;
                }
            }
        }
    }

    public void MoveCurrentBuild()
    {
        if (isBuilding && !isSelectedBuild)
        {
            if (_thisBuildGO && _territoryManager)
            {
                if (_territoryManager.builds.Contains(_thisBuildGO))
                {
                    UIManagerVBS.instance.OnBuildSelectedClick(_thisBuildGO.GetComponent<BuildVBS>().buildData.ID);
                    _thisMovedBuild = _thisBuildGO;
                    isMovedBuild = true;
                }
            }
        }
    }

    public void RotateCurrentBuild()
    {
        if (isBuilding && !isSelectedBuild)
        {
            if (_thisBuildGO && _territoryManager)
            {
                if (_territoryManager.builds.Contains(_thisBuildGO))
                {
                    Quaternion newRotation = _thisBuildGO.transform.rotation * Quaternion.AngleAxis(_thisBuildGO.GetComponent<BuildVBS>().buildData.RotateAngle, Vector3.up);
                    _thisBuildGO.GetComponent<BuildVBS>().RotateBuild(newRotation);
                }
            }
        }
    }

    public void DestroyCurrentBuild()
    {
        if (isBuilding && !isSelectedBuild)
        {
            if (_thisBuildGO && _territoryManager)
            {
                _territoryManager.DestroyBuild(_thisBuildGO);
            }
        }
    }

    #endregion

    #region Информация о строении

    private void CheckBuildRayCastPhysic()
    {
        Ray ray = new(_cameraTransform.position, _cameraTransform.forward);

        if (Physics.Raycast(ray, out _hitBuildInfo, _rayDistanceBuildInfo, _buildInfoLayerMask))
        {
            _thisBuildGO = _hitBuildInfo.transform.gameObject.GetComponent<BuildParent>().parent;
            SetBuildInfo();
        }
        else
        {
            if (buildInfoPanel.gameObject.activeSelf)
                RemoveBuildInfo();
        }

        SetSizeBuildSettingsPanel();
    }

    private void SetBuildInfo()
    {
        buildInfoPanel.gameObject.SetActive(true);
        buildSettingsPanel.gameObject.SetActive(isBuilding && !isSelectedBuild);

        Vector3 screenPos = _camera.WorldToScreenPoint(_thisBuildGO.transform.position);
        buildInfoPanel.position = screenPos;

        BuildVBS build = _thisBuildGO.GetComponent<BuildVBS>();
        buildSettingsTitle.text = build.buildData.Name;
        buildName.text = build.buildData.Name;
        float current = build._currentHP;
        float max = build.buildData.MaxHP;
        lineHP.fillAmount = current / max;
        buildHP.text = build._currentHP + "/" + build.buildData.MaxHP;

        SetActiveBuildSettingsButtons(build.buildData);
    }

    private void RemoveBuildInfo()
    {
        buildInfoPanel.gameObject.SetActive(false);
        buildSettingsPanel.gameObject.SetActive(false);
    }

    private void SetActiveBuildSettingsButtons(BuildDataSO buildData)
    {
        buildSettingsButtons[0].gameObject.SetActive(buildData.CanImprove);
        buildSettingsButtons[1].gameObject.SetActive(buildData.CanRepair);
        buildSettingsButtons[2].gameObject.SetActive(buildData.CanRotate);
        buildSettingsButtons[3].gameObject.SetActive(buildData.CanMove);
        buildSettingsButtons[4].gameObject.SetActive(buildData.CanDestroy);
    }

    private void SetSizeBuildSettingsPanel()
    {
        int size = 0;
        for (int i = 0; i < buildSettingsPanel.childCount; i++)
            if (buildSettingsPanel.GetChild(i).gameObject.activeSelf)
                size++;
                
        buildSettingsPanel.sizeDelta = new Vector2(buildSettingsPanel.sizeDelta.x, size * 60f);
    }

    #endregion
}