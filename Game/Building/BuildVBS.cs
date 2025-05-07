using Photon.Pun;
using UnityEngine;

#region Структуры

[System.Serializable]
public struct PointsAndID
{
    public GameObject[] _buildPoints;
    public int[] _idForVisiblePoints;
}

#endregion

public class BuildVBS : MonoBehaviour
{
    #region Переменные

    private PhotonView _photonView;
    [HideInInspector] public TerritoryManagerVBS territoryManager;

    [Header("|-# BUILD")]
    public int ID;
    [Space(10)]
    public int _currentLVL;
    public int _currentHP;
    public BuildDataSO buildData;
    [SerializeField] private PointsAndID[] _PAID;
    [SerializeField] private GameObject _hitFX;

    [Header("|-# SUPPORT")]
    [SerializeField] private bool _needSupport;
    [SerializeField] private Transform[] _supportCheckers;
    [SerializeField] private LayerMask _checkerLayerMask;

    private Collider[] _collidersCache = new Collider[20];

    #endregion

    #region Start

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
    }

    #endregion

    #region Инициализация

    public void SetBuildComponents(BuildDataSO _buildData)
    {
        buildData = _buildData;
        _currentHP = _buildData.MaxHP;
        _currentLVL = 1;

        VisiblePoints(_buildData.ID);
    }

    public void BuildChanged(int id)
    {
        VisiblePoints(id);
    }

    #endregion

    #region Проверка опоры

    public bool CheckSupports()
    {
        if (!_needSupport)
            return true;

        foreach (var check in _supportCheckers)
        {
            int colliderCount = Physics.OverlapSphereNonAlloc(check.position, 0.1f, _collidersCache, _checkerLayerMask);
            if (colliderCount > 0)
                return true;
        }
        return false;
    }

    #endregion

    #region События с точками строения

    private void VisiblePoints(int id)
    {
        foreach (var paid in _PAID)
            foreach (int idForPoints in paid._idForVisiblePoints)
                if (id == idForPoints)
                    foreach (GameObject point in paid._buildPoints)
                        point.SetActive(true);
    }

    public void HidePoints()
    {
        foreach (var paid in _PAID)
            foreach (GameObject point in paid._buildPoints)
                point.SetActive(false);
    }

    #endregion

    #region Другое

    private void OnDrawGizmosSelected()
    {
        if (!_needSupport)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_supportCheckers[0].position, 0.1f);
    }

    public GameObject GetHitFX()
    {
        return _hitFX;
    }

    #endregion

    #region PhotonPUN

    public void MoveBuild(Vector3 newPosition, Quaternion newRotation)
    {
        _photonView.RPC("RPC_MoveBuild", RpcTarget.AllBuffered, newPosition, newRotation);
    }

    public void RotateBuild(Quaternion newRotation)
    {
        _photonView.RPC("RPC_RotateBuild", RpcTarget.AllBuffered, newRotation);
    }

    public void InflictDamage(int damage)
    {
        _photonView.RPC("RPC_InflictDamage", RpcTarget.AllBuffered, damage);
    }

    [PunRPC]
    private void RPC_MoveBuild(Vector3 newPosition, Quaternion newRotation)
    {
        transform.SetPositionAndRotation(newPosition, newRotation);
    }

    [PunRPC]
    private void RPC_RotateBuild(Quaternion newRotation)
    {
        transform.rotation = newRotation;
    }

    [PunRPC]
    private void RPC_InflictDamage(int damage)
    {
        if (_currentHP <= 0) return;

        _currentHP -= damage;
        if (_currentHP <= 0)
        {
            territoryManager.CheckUpdatesSupport();
            territoryManager.DestroyBuild(gameObject);
        }
    }

    #endregion
}