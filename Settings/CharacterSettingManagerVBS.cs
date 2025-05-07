using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#region Структуры

[System.Serializable]
public struct CharacterOptions
{
    public int Id;
    public int DefaultId;
    public Image[] Buttons;
    public GameObject[] Options;
}
public delegate void CharacterFunctions(int value);

#endregion

public class CharacterSettingManagerVBS : MonoBehaviour
{
    #region Переменные

    [SerializeField] private Image[] _categoryButtons;

    [SerializeField] private Color _selectedCategoryButtonColor;
    [SerializeField] private Color _normalCategoryButtonColor;

    [SerializeField] private Color _selectedButtonColor;
    [SerializeField] private Color _normalButtonColor;

    [SerializeField] private CharacterOptions[] _characterOption;
    private CharacterFunctions[] _characterFunctions;
    public Transform _pointMeshTransform;
    [SerializeField] private float _smoothTimeForResetRot;
    [SerializeField] private GameObject[] _characterMesh;
    private GameObject _currentMesh;

    #endregion

    public void StartScript(Transform point)
    {
        _pointMeshTransform = point;
        _characterFunctions = new CharacterFunctions[]
        {
            SetCharacter
        };

        LoadingCharacterSettings();
    }

    #region События с панелью редактора персонажа

    public void OpenCharacterSetting()
    {
        SelectCategoryButton(_categoryButtons[0]);
        MenuManager.instance.OpenMenu("characterSetting");
        StartCoroutine(RotateToAngle(180f));
    }

    public void CloseCharacterSetting()
    {
        MenuManager.instance.OpenMenu("main");
        SavingCharacterSettings();
        StartCoroutine(RotateToAngle(180f));
    }

    public void SelectCategoryButton(Image button)
    {
        foreach (Image but in _categoryButtons)
            but.color = _normalCategoryButtonColor;
        button.color = _selectedCategoryButtonColor;
    }

    private IEnumerator RotateToAngle(float targetAngle)
    {
        float currentAngle = _pointMeshTransform.eulerAngles.y;
        float rotationDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

        while (Mathf.Abs(rotationDifference) > 0.01f)
        {
            float rotationStep = _smoothTimeForResetRot * Time.deltaTime;

            if (Mathf.Abs(rotationDifference) < rotationStep)
            {
                rotationStep = Mathf.Abs(rotationDifference);
            }

            float step = Mathf.Sign(rotationDifference) * rotationStep;
            _pointMeshTransform.Rotate(Vector3.up, step);
            currentAngle = _pointMeshTransform.eulerAngles.y;
            rotationDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

            yield return null;
        }
        Vector3 finalRotation = _pointMeshTransform.eulerAngles;
        finalRotation.y = targetAngle;
        _pointMeshTransform.eulerAngles = finalRotation;
    }

    #endregion

    #region События настроек персонажа

    public void SetDefault()
    {

    }

    public void LoadingCharacterSettings()
    {
        for (int i = 0; i < _characterOption.Length; i++)
        {
            int value = LoadSetting("characterOption_" + i, _characterOption[i].DefaultId);
            _characterFunctions[i](value);
        }
    }

    public void SavingCharacterSettings()
    {
        for (int i = 0; i < _characterOption.Length; i++) SaveSetting("characterOption_" + i, _characterOption[i].Id);
    }

    private void SelectButton(Image[] images, int index)
    {
        foreach (var button in images)
            button.color = _normalButtonColor;
        images[index].color = _selectedButtonColor;
    }

    private void SelectOption(GameObject[] options, int index)
    {
        foreach (var option in options)
            option.SetActive(false);
        options[index].SetActive(true);
    }

    #endregion

    public void SetCharacter(int index) // Выбрать персонажа [0]
    {
        GameData.Character = _characterMesh[index];
        _characterOption[0].Id = index;
        SelectButton(_characterOption[0].Buttons, index);

        if (_currentMesh) Destroy(_currentMesh);
        _currentMesh = Instantiate(_characterMesh[index], _pointMeshTransform);
        Destroy(_currentMesh.GetComponent<BodyDataVBS>());
        Destroy(_currentMesh.GetComponent<PhotonAnimatorView>());
        Destroy(_currentMesh.GetComponent<PhotonView>());
    }

    #region Сохранение и Загрузка

    private void SaveSetting(string key, int _value)
    {
        PlayerPrefs.SetInt(key, _value);
        PlayerPrefs.Save();
    }

    private int LoadSetting(string key, int _default)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetInt(key);
        }
        else
        {
            return _default;
        }
    }

    #endregion
}
