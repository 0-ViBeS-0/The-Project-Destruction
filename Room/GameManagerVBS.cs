using UnityEngine;
using Photon.Pun;


public class GameManagerVBS : MonoBehaviour
{
    #region Переменные

    [Header("|-# SPAWN")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnTransform;
    private GameObject _player;

    #endregion

    #region Start

    private void Start()
    {
        SpawnPlayer();
    }

    #endregion

    #region Создание игрока

    private void SpawnPlayer()
    {
        _player = PhotonNetwork.Instantiate(_playerPrefab.name, _spawnTransform.position, Quaternion.identity);
        UIManagerVBS.instance.AssignPlayer(_player);
    }

    #endregion
}