using UnityEngine;

public class MenuInitializatorVBS : MonoBehaviour
{
    #region Переменные

    [Header("|-# CHARACTER_SETTING")]
    [SerializeField] private Transform _pointMesh;

    [Header("|-# SETTINGS")]
    [SerializeField] private Camera _mainCamera;

    #endregion

    #region Awake/Start

    private void Awake()
    {
        MenuManager.instance.OpenMenu("loading");
    }

    private void Start()
    {
        SettingsManagerVBS.instance.gameObject.GetComponent<CharacterSettingManagerVBS>().StartScript(_pointMesh);
        SettingsManagerVBS.instance.InitializeOnStart(_mainCamera);
    }

    #endregion
}