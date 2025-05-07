using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class SkinInitializatorVBS : MonoBehaviourPun
{
    #region Переменные

    private RPC_PlayerVBS _rpc_player;
    private PlayerControllerVBS _playerController;
    [HideInInspector] public BodyDataVBS _bodyData;

    private GameObject _characterGO;
    private Transform _playerSkinTransform;
    private SkinnedMeshRenderer _playerSkin;

    [Header("|-# CLOTHS")]
    [SerializeField] private List<GameObject> _cloths = new();

    #endregion

    #region Инициализация персонажа

    public void LoadBody()
    {
        _rpc_player = GetComponent<RPC_PlayerVBS>();
        _playerController = transform.GetComponent<PlayerControllerVBS>();
        SetSexMesh();
    }

    private void SetSexMesh()
    {
        _characterGO = PhotonNetwork.Instantiate(GameData.Character.name, transform.position, transform.rotation);
        _bodyData = _characterGO.GetComponent<BodyDataVBS>();
        UIManagerVBS.instance.AssignBodyData(_bodyData);
        InitializeComponentsSex();
        _rpc_player.SetParent(RpcTarget.AllBuffered, _characterGO, gameObject);
    }

    private void InitializeComponentsSex()
    {
        _playerSkinTransform = _bodyData.playerSkin;
        _playerSkin = _playerSkinTransform.GetComponent<SkinnedMeshRenderer>();
        _playerController.animator = _bodyData.animator;
        _playerController._cameraHeadTargetTransform = _bodyData.cameraHeadTargetTransform;
        _playerController.headTargetIK = _bodyData.headTargetTransform;
        SetClothes();
    }

    #endregion

    #region Инициализация одежды

    public void InitializeCloths(List<GameObject> cloths)
    {
        _cloths = new List<GameObject>(cloths);
    }

    private void SetClothes()
    {
        if (_cloths != null)
            foreach (GameObject cloth in _cloths)
                AddCloth(cloth);
    }

    private void AddCloth(GameObject clothPrefab)
    {
        GameObject clothObj = PhotonNetwork.Instantiate(clothPrefab.name, _playerSkinTransform.position, _playerSkinTransform.rotation);
        _rpc_player.SetParent(RpcTarget.AllBuffered, clothObj, _characterGO);
        _rpc_player.ApplyOriginalTransform(RpcTarget.AllBuffered, clothObj, _characterGO.transform.localPosition, _characterGO.transform.localRotation, _characterGO.transform.localScale);
        SkinnedMeshRenderer[] renderers = clothObj.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            renderer.bones = _playerSkin.bones;
            renderer.rootBone = _playerSkin.rootBone;
        }
    }

    #endregion
}
