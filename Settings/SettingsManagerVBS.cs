using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

#region Структуры

[System.Serializable]
public struct Setting_B
{
    public int Value;
    public int DefaultValue;
    public Image[] Buttons;
}
public delegate void SettingFunctionB(int value);
[System.Serializable]
public struct Setting_S
{
    public int Value;
    public int DefaultValue;
    public Slider Slider;
    public TMP_InputField InputField;
}
public delegate void SettingFunctionS(float value);

#endregion

public class SettingsManagerVBS : MonoBehaviour
{
    #region Переменные

    public static SettingsManagerVBS instance;

    [Header("|-# PANELS")]
    [SerializeField] private Image[] _panelButtons;

    [SerializeField] private Color _selectedPanelButtonColor;
    [SerializeField] private Color _normalPanelButtonColor;

    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private TMP_Text _InfoText;

    [SerializeField] private Color _selectedButtonColor;
    [SerializeField] private Color _normalButtonColor;

    [Header("|-# GENERAL")]
    [SerializeField] private Setting_B[] generalSettingB;
    private SettingFunctionB[] generalSettingFunctionsB;
    [SerializeField] private Setting_S[] generalSettingS;
    private SettingFunctionS[] generalSettingFunctionsS;

    [SerializeField] private TMP_Text _languageTitle;
    [SerializeField] private TMP_Text _regionTitle;

    [Header("|-# GRAPHICS")]
    [SerializeField] private Setting_B[] graphicSettingB;
    private SettingFunctionB[] graphicSettingFunctionsB;
    [SerializeField] private Setting_S[] graphicSettingS;
    private SettingFunctionS[] graphicSettingFunctionsS;

    [SerializeField] private GameObject _lockersSettingsTextureStreaming;
    [SerializeField] private GameObject[] _lockersSettingsShadowType;
    [SerializeField] private GameObject _lockersSettingsParticles;

    public Camera _mainCamera;
    [HideInInspector] public PostProcessVolume _postProcessVolume;
    [HideInInspector] public PostProcessLayer _postProcessLayer;
    private Bloom _bloom;
    private MotionBlur _motionBlur;
    private Vignette _vignette;
    private Grain _grain;

    [SerializeField] private GameObject[] _lockersSettingsPostProcessingEffects;

    [Header("|-# CONTROL")]
    [SerializeField] private Setting_B[] controlSettingB;
    private SettingFunctionB[] controlSettingFunctionsB;
    [SerializeField] private Setting_S[] controlSettingS;
    private SettingFunctionS[] controlSettingFunctionsS;

    [SerializeField] private GameObject _lockersSettingsMobileType;
    [SerializeField] private GameObject[] _lockersSettingsPCType;

    #endregion

    #region Awake/Start

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeDelegateMethods();
        LoadingSettings();
        UIManagerVBS.instance.InitializeSettings();
    }

    #endregion

    #region Инициализация

    public void InitializeOnStart(Camera cam)
    {
        _mainCamera = cam;
        _postProcessLayer = _mainCamera.gameObject.GetComponent<PostProcessLayer>();
        _postProcessVolume = _mainCamera.gameObject.GetComponent<PostProcessVolume>();

        _postProcessVolume.profile.TryGetSettings(out _bloom);
        _postProcessVolume.profile.TryGetSettings(out _motionBlur);
        _postProcessVolume.profile.TryGetSettings(out _vignette);
        _postProcessVolume.profile.TryGetSettings(out _grain);

        SetPostProcessing(graphicSettingB[13].Value);
    }

    private void InitializeDelegateMethods()
    {
        generalSettingFunctionsB = new SettingFunctionB[]
        {
            SetLanguage,
            SetRegion,
            SetPingEnable,
            SetPlayersNickname,
            SetFrameRateEnable,
            SetVSync
        };
        generalSettingFunctionsS = new SettingFunctionS[]
        {
            SetFrameRate
        };
        graphicSettingFunctionsB = new SettingFunctionB[]
        {
            SetQualityLevel,
            SetTextureQuality,
            SetAnisotropicFiltering,
            SetTextureStreaming,
            SetLightingQuality,
            SetReflectionQuality,
            SetShadowType,
            SetShadowResolution,
            SetShadowProjection,
            SetLODBias,
            SetAntiAliasing,
            SetParticles,
            SetSoftParticles,
            SetPostProcessing,
            SetBloom,
            SetMotionBlur,
            SetVignette,
            SetGrain
        };
        graphicSettingFunctionsS = new SettingFunctionS[]
        {
            SetMemoryBudget,
            SetShadowDistance,
            SetDrawDistance
        };
        controlSettingFunctionsB = new SettingFunctionB[]
        {
            SetControlType,
            SetInverseX,
            SetInverseY,
            SetJoystickType,
            SetRUNButtonMode,
            SetCROUCHButtonMode,
            SetCRAWLButtonMode
        };
        controlSettingFunctionsS = new SettingFunctionS[]
        {
            SetSensivity
        };
    }

    #endregion

    #region События с панелью настройками

    public void OpenSettings()
    {
        SelectButton(_panelButtons[0]);
        MenuManager.instance.OpenMenu("settings");
        MenuManager.instance.OpenPanel("general");
    }

    public void CloseSettings()
    {
        SavingSettings();
        if (LauncherVBS.instance.InRoom())
        {
            MenuManager.instance.OpenMenu("");
            UIManagerVBS.instance.OnPauseButtonClick();
        }
        else
        {
            MenuManager.instance.OpenMenu("main");
        }
        UIManagerVBS.instance.InitializeSettings();
    }

    public void SelectButton(Image button)
    {
        foreach (Image but in _panelButtons) but.color = _normalPanelButtonColor;

        button.color = _selectedPanelButtonColor;
    }

    #endregion

    #region Авто настройка

    public void AutoSettings()
    {
        int systemMemorySize = SystemInfo.systemMemorySize; // Величина оперативной памяти (в мегабайтах)
        int processorCount = SystemInfo.processorCount; // Количество ядер процессора
        int graphicsMemorySize = SystemInfo.graphicsMemorySize; // Величина видеопамяти (в мегабайтах)
        int maxTextureSize = SystemInfo.maxTextureSize; // Максимальный размер текстуры

        if (systemMemorySize < 4000 || processorCount <= 4 || graphicsMemorySize < 1000 || maxTextureSize <= 2048)
            SetQualityLevel(0);
        else if (systemMemorySize < 8000 || processorCount <= 6 || graphicsMemorySize < 2000 || maxTextureSize <= 4096)
            SetQualityLevel(1);
        else if (systemMemorySize < 16000 || processorCount <= 8 || graphicsMemorySize < 4000 || maxTextureSize <= 8192)
            SetQualityLevel(2);
        else
            SetQualityLevel(3);
    }

    #endregion

    #region События настроек

    public void SetDefaultSettings()
    {
        for (int i = 0; i < generalSettingB.Length; i++)
        {
            int value = generalSettingB[i].DefaultValue;
            generalSettingFunctionsB[i](value);
        }
        for (int i = 0; i < generalSettingS.Length; i++)
        {
            int value = generalSettingS[i].DefaultValue;
            generalSettingFunctionsS[i](value);
        }
        for (int i = 0; i < graphicSettingB.Length; i++)
        {
            int value = graphicSettingB[i].DefaultValue;
            graphicSettingFunctionsB[i](value);
        }
        for (int i = 0; i < graphicSettingS.Length; i++)
        {
            int value = graphicSettingS[i].DefaultValue;
            graphicSettingFunctionsS[i](value);
        }
        for (int i = 0; i < controlSettingB.Length; i++)
        {
            int value = controlSettingB[i].DefaultValue;
            controlSettingFunctionsB[i](value);
        }
        for (int i = 0; i < controlSettingS.Length; i++)
        {
            int value = controlSettingS[i].DefaultValue;
            controlSettingFunctionsS[i](value);
        }
    }

    public void LoadingSettings()
    {
        for (int i = 0; i < generalSettingB.Length; i++)
        {
            int value = LoadSetting("generalSettingB_" + i, generalSettingB[i].DefaultValue);
            generalSettingFunctionsB[i](value);
        }
        for (int i = 0; i < generalSettingS.Length; i++)
        {
            int value = LoadSetting("generalSettingS_" + i, generalSettingS[i].DefaultValue);
            generalSettingFunctionsS[i](value);
        }
        for (int i = 0; i < graphicSettingB.Length; i++)
        {
            int value = LoadSetting("graphicSettingB_" + i, graphicSettingB[i].DefaultValue);
            graphicSettingFunctionsB[i](value);
        }
        for (int i = 0; i < graphicSettingS.Length; i++)
        {
            int value = LoadSetting("graphicSettingS_" + i, graphicSettingS[i].DefaultValue);
            graphicSettingFunctionsS[i](value);
        }
        for (int i = 0; i < controlSettingB.Length; i++)
        {
            int value = LoadSetting("controlSettingB_" + i, controlSettingB[i].DefaultValue);
            controlSettingFunctionsB[i](value);
        }
        for (int i = 0; i < controlSettingS.Length; i++)
        {
            int value = LoadSetting("controlSettingS_" + i, controlSettingS[i].DefaultValue);
            controlSettingFunctionsS[i](value);
        }
    }

    public void SavingSettings()
    {
        for (int i = 0; i < generalSettingB.Length; i++) SaveSetting("generalSettingB_" + i, generalSettingB[i].Value);
        for (int i = 0; i < generalSettingS.Length; i++) SaveSetting("generalSettingS_" + i, generalSettingS[i].Value);
        for (int i = 0; i < graphicSettingB.Length; i++) SaveSetting("graphicSettingB_" + i, graphicSettingB[i].Value);
        for (int i = 0; i < graphicSettingS.Length; i++) SaveSetting("graphicSettingS_" + i, graphicSettingS[i].Value);
        for (int i = 0; i < controlSettingB.Length; i++) SaveSetting("controlSettingB_" + i, controlSettingB[i].Value);
        for (int i = 0; i < controlSettingS.Length; i++) SaveSetting("controlSettingS_" + i, controlSettingS[i].Value);
    }

    private void SelectButton(Image[] images, int index)
    {
        foreach (var button in images)
        {
            button.color = _normalButtonColor;
        }
        images[index].color = _selectedButtonColor;
    }

    private float InputFieldStringToFloat(TMP_InputField inputField, Slider slider)
    {
        if (string.IsNullOrEmpty(inputField.text) || !float.TryParse(inputField.text, out float value))
        {
            return Mathf.Clamp(slider.value, slider.minValue, slider.maxValue);
        }

        return Mathf.Clamp(value, slider.minValue, slider.maxValue);
    }

    private void SetActiveLocker(GameObject locker, bool active)
    {
        if(active) locker.SetActive(true);
        else locker.SetActive(false);
    }

    #endregion

    //////////////////////////////////////////////////////////////////////////////////////

    #region GENERAL
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////// Игра
    public void SetLanguage(int index) // Выбор языка B[0]
    {
        GameData.language = index;
        generalSettingB[0].Value = index;
        SetLanguageTitle(index);
    }

    private void SetLanguageTitle(int id)
    {
        _languageTitle.text = id switch
        {
            0 => "Как  в  системе",
            1 => "English",
            2 => "Русский",
            _ => "BUG"
        };
    }
    ////////////////////////////////////////////////////////////////////////////////// Онлайн
    public void SetRegion(int index) // Выбор региона B[1]
    {
        LauncherVBS.instance.SetRegion(index, _regionTitle);
        generalSettingB[1].Value = index;
    }

    public void SetPingEnable(int index) // Показать пинг B[2]
    {
        GameData.pingEnable = index == 1;
        generalSettingB[2].Value = index;
        SelectButton(generalSettingB[2].Buttons, index);
    }

    public void SetPlayersNickname(int index) // Имена игроков в игре B[3]
    {
        GameData.playersNicknameVisible = index == 1;
        generalSettingB[3].Value = index;
        SelectButton(generalSettingB[3].Buttons, index);
    }
    ////////////////////////////////////////////////////////////////////////////////// Частота кадров ФПС
    public void SetFrameRateEnable(int index) // Показать частоту кадров B[4]
    {
        GameData.fpsEnable = index == 1;
        generalSettingB[4].Value = index;
        SelectButton(generalSettingB[4].Buttons, index);
    }

    public void SetFrameRate(float value) // Частота кадров S[0]
    {
        int valueInt = value < 140 ? Mathf.RoundToInt(value / 5) * 5 : Mathf.RoundToInt(value);

        Application.targetFrameRate = valueInt;
        generalSettingS[0].Value = valueInt;
        generalSettingS[0].InputField.text = valueInt.ToString() + "  FPS";
        generalSettingS[0].Slider.value = valueInt;
    }
    public void SetFrameRateInputField() // Частота кадров (Input Field)
    {
        float valueFloat = InputFieldStringToFloat(generalSettingS[0].InputField, generalSettingS[0].Slider);
        SetFrameRate(valueFloat);
    }

    public void SetVSync(int index) // Вертикальная синхронизация (VSync) B[5]
    {
        QualitySettings.vSyncCount = index;
        generalSettingB[5].Value = index;
        SelectButton(generalSettingB[5].Buttons, index);
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #endregion

    #region GRAPHICS
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////// Общее качество графики
    public void SetQualityLevel(int index) // Уровень графики [0]
    {
        QualitySettings.SetQualityLevel(index);
        graphicSettingB[0].Value = index;
        SelectButton(graphicSettingB[0].Buttons, index);
        SetAllSettings(index);
    }

    private void SetAllSettings(int lvl)
    {
        SetTextureQuality(lvl);
        SetAnisotropicFiltering(lvl < 1 ? 0 : (lvl) - 1);
        SetTextureStreaming(lvl < 2 ? 1 : 0);
        SetMemoryBudget(lvl < 2 ? 512 * ((lvl) + 1) : 1024);
        SetLightingQuality(lvl);
        SetReflectionQuality(lvl < 2 ? 0 : 1);
        SetShadowType(lvl < 2 ? lvl : 2);
        SetShadowResolution(lvl);
        SetShadowProjection(1);
        SetShadowDistance(lvl < 1 ? 10 : 75 * ((lvl) + 1));
        SetLODBias(lvl);
        SetAntiAliasing(lvl);
        SetDrawDistance(lvl < 1 ? 30 : 250 * ((lvl) + 1));
        SetParticles(lvl < 2 ? 0 : 1);
        SetSoftParticles(lvl < 2 ? 0 : 1);
        SetPostProcessing(lvl < 2 ? 0 : 1);
    }
    ////////////////////////////////////////////////////////////////////////////////// Текстуры
    public void SetTextureQuality(int index) // Качество текстур B[1]
    {
        int[] textureLimits = { 3, 2, 1, 0 };

        QualitySettings.globalTextureMipmapLimit = textureLimits[index];
        graphicSettingB[1].Value = index;
        SelectButton(graphicSettingB[1].Buttons, index);
    }

    public void SetAnisotropicFiltering(int index) // Анизотропная фильтрация B[2]
    {
        AnisotropicFiltering[] filteringOptions = {
        AnisotropicFiltering.Disable,
        AnisotropicFiltering.Enable,
        AnisotropicFiltering.ForceEnable
        };

        QualitySettings.anisotropicFiltering = filteringOptions[index];
        graphicSettingB[2].Value = index;
        SelectButton(graphicSettingB[2].Buttons, index);
    }

    public void SetTextureStreaming(int index) // Потоковая передача текстур B[3]
    {
        QualitySettings.streamingMipmapsActive = index == 1;
        graphicSettingB[3].Value = index;
        SelectButton(graphicSettingB[3].Buttons, index);

        SetActiveLocker(_lockersSettingsTextureStreaming, !QualitySettings.streamingMipmapsActive);
    }

    public void SetMemoryBudget(float value) // Объём памяти для ППТ S[0]
    {
        int valueInt = Mathf.RoundToInt(value);

        QualitySettings.streamingMipmapsMemoryBudget = valueInt;
        graphicSettingS[0].Value = valueInt;
        graphicSettingS[0].InputField.text = valueInt.ToString() + "  Мб";
        graphicSettingS[0].Slider.value = valueInt;
    }
    public void SetMemoryBudgetInputField() // Объём памяти для ППТ (Input Field)
    {
        float valueFloat = InputFieldStringToFloat(graphicSettingS[0].InputField, graphicSettingS[0].Slider);
        SetMemoryBudget(valueFloat);
    }
    ////////////////////////////////////////////////////////////////////////////////// Освещение
    public void SetLightingQuality(int index) // Качество освещения B[4]
    {
        int[] lightCounts = { 0, 2, 4, 8 };

        QualitySettings.pixelLightCount = lightCounts[index];
        graphicSettingB[4].Value = index;
        SelectButton(graphicSettingB[4].Buttons, index);
    }

    public void SetReflectionQuality(int index) // Качество отражений B[5]
    {
        QualitySettings.realtimeReflectionProbes = index == 1;
        graphicSettingB[5].Value = index;
        SelectButton(graphicSettingB[5].Buttons, index);
    }
    ////////////////////////////////////////////////////////////////////////////////// Тени
    public void SetShadowType(int index) // Тени B[6]
    {
        QualitySettings.shadows = (ShadowQuality)index;
        graphicSettingB[6].Value = index;
        SelectButton(graphicSettingB[6].Buttons, index);

        foreach (GameObject locker in _lockersSettingsShadowType)
        SetActiveLocker(locker, index == 0);
    }

    public void SetShadowResolution(int index) // Разрешение теней B[7]
    {
        QualitySettings.shadowResolution = (ShadowResolution)index;
        graphicSettingB[7].Value = index;
        SelectButton(graphicSettingB[7].Buttons, index);
    }

    public void SetShadowProjection(int index) // Проекция теней B[8]
    {
        QualitySettings.shadowProjection = (ShadowProjection)index;
        graphicSettingB[8].Value = index;
        SelectButton(graphicSettingB[8].Buttons, index);
    }

    public void SetShadowDistance(float value) // Дальность теней S[1]
    {
        int valueInt = Mathf.RoundToInt(value);

        QualitySettings.shadowDistance = valueInt;
        graphicSettingS[1].Value = valueInt;
        graphicSettingS[1].InputField.text = valueInt.ToString() + "  м";
        graphicSettingS[1].Slider.value = valueInt;
    }
    public void SetShadowDistanceInputField() // Дальность теней (Input Field)
    {
        float valueFloat = InputFieldStringToFloat(graphicSettingS[1].InputField, graphicSettingS[1].Slider);
        SetShadowDistance(valueFloat);
    }
    ////////////////////////////////////////////////////////////////////////////////// Детализация
    public void SetLODBias(int index) // Уровень детализации (LOD Bias) B[9]
    {
        float[] lodBiases = { 0.5f, 1.0f, 1.5f, 2.0f };

        QualitySettings.lodBias = lodBiases[index];
        graphicSettingB[9].Value = index;
        SelectButton(graphicSettingB[9].Buttons, index);
    }

    public void SetAntiAliasing(int index) // Сглаживание B[10]
    {
        int[] antiAliasingValues = { 0, 2, 4, 8 };

        QualitySettings.antiAliasing = antiAliasingValues[index];
        if (_postProcessLayer)
            _postProcessLayer.antialiasingMode = (PostProcessLayer.Antialiasing)index;
        graphicSettingB[10].Value = index;
        SelectButton(graphicSettingB[10].Buttons, index);
    }

    public void SetDrawDistance(float value) // Дальность прорисовки S[2] 
    {
        int valueInt = Mathf.RoundToInt(value);

        Camera.main.farClipPlane = valueInt;
        graphicSettingS[2].Value = valueInt;
        graphicSettingS[2].InputField.text = valueInt.ToString() + "  м";
        graphicSettingS[2].Slider.value = valueInt;
    }
    public void SetDrawDistanceInputField() // Дальность прорисовки (Input Field)
    {
        float valueFloat = InputFieldStringToFloat(graphicSettingS[2].InputField, graphicSettingS[2].Slider);
        SetDrawDistance(valueFloat);
    }
    ////////////////////////////////////////////////////////////////////////////////// Частицы
    public void SetParticles(int index) // Частицы B[11] ///////// НЕ ЗАКОНЧЕНО //////////////////////////////////////////
    {
        GameData.particles = index == 1;
        graphicSettingB[11].Value = index;
        SelectButton(graphicSettingB[11].Buttons, index);
        /*
        if (index == 0)
        {
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems) ps.Stop();
        } */

        SetActiveLocker(_lockersSettingsParticles, index == 0);
    }

    public void SetSoftParticles(int index) // Мягкие частицы B[12]
    {
        QualitySettings.softParticles = index == 1;
        graphicSettingB[12].Value = index;
        SelectButton(graphicSettingB[12].Buttons, index);
    }
    ////////////////////////////////////////////////////////////////////////////////// Постобработка
    public void SetPostProcessing(int index) // Постобработка B[13] 
    {
        GameData.postProcessing = index;
        graphicSettingB[13].Value = index;
        SelectButton(graphicSettingB[13].Buttons, index);

        if (_postProcessVolume)
        {
            _postProcessVolume.weight = index;
        }

        foreach (GameObject locker in _lockersSettingsPostProcessingEffects)
            SetActiveLocker(locker, index == 0);
    }

    public void SetBloom(int index) // Сияние B[14]
    {
        GameData.Bloom = index;
        graphicSettingB[14].Value = index;
        SelectButton(graphicSettingB[14].Buttons, index);

        if (_bloom)
            _bloom.active = index == 1;
    }

    public void SetMotionBlur(int index) // Размытие в движении B[15]
    {
        GameData.MotionBlur = index;
        graphicSettingB[15].Value = index;
        SelectButton(graphicSettingB[15].Buttons, index);

        if (_motionBlur)
            _motionBlur.active = index == 1;
    }

    public void SetVignette(int index) // Виньетка B[16]
    {
        GameData.Vignette = index;
        graphicSettingB[16].Value = index;
        SelectButton(graphicSettingB[16].Buttons, index);

        if (_vignette)
            _vignette.active = index == 1;
    }

    public void SetGrain(int index) // Зернистость B[17]
    {
        GameData.Grain = index;
        graphicSettingB[17].Value = index;
        SelectButton(graphicSettingB[17].Buttons, index);

        if (_grain)
            _grain.active = index == 1;
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #endregion

    #region CONTROLS
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////// Общее управление
    public void SetControlType(int index) // Тип управления B[0]
    {
        GameData.controlType = index;
        controlSettingB[0].Value = index;
        SelectButton(controlSettingB[0].Buttons, index);

        SetActiveLocker(_lockersSettingsMobileType, index == 0);
        foreach (GameObject locker in _lockersSettingsPCType)
            SetActiveLocker(locker, index == 1);
    }
    ////////////////////////////////////////////////////////////////////////////////// Экран
    public void SetSensivity(float value) // Чувствительность S[0]
    {
        int valueInt = Mathf.RoundToInt(value);

        GameData.sensitivity = valueInt;
        controlSettingS[0].Value = valueInt;
        controlSettingS[0].InputField.text = value.ToString();
        controlSettingS[0].Slider.value = valueInt;
    }
    public void SetSensivityInputField() // Чувствительность (Input Field)
    {
        float valueFloat = InputFieldStringToFloat(controlSettingS[0].InputField, controlSettingS[0].Slider);
        SetSensivity(valueFloat);
    }

    public void SetInverseX(int index) // Инверсия по X B[1]
    {
        GameData.inverseX = index == 1;
        controlSettingB[1].Value = index;
        SelectButton(controlSettingB[1].Buttons, index);
    }

    public void SetInverseY(int index) // Инверсия по Y B[2]
    {
        GameData.inverseY = index == 1;
        controlSettingB[2].Value = index;
        SelectButton(controlSettingB[2].Buttons, index);
    }
    ////////////////////////////////////////////////////////////////////////////////// Джойстик
    public void SetJoystickType(int index) // Тип джойстика B[3]
    {
        GameData.joystickType = index;
        controlSettingB[3].Value = index;
        SelectButton(controlSettingB[3].Buttons, index);
    }
    ////////////////////////////////////////////////////////////////////////////////// Кнопки - способы активации
    public void SetRUNButtonMode(int index) // Кнопка БЕГ B[4]
    {
        GameData.runButtonMode = index;
        controlSettingB[4].Value = index;
        SelectButton(controlSettingB[4].Buttons, index);
    }

    public void SetCROUCHButtonMode(int index) // Кнопка СЕСТЬ B[5]
    {
        GameData.crouchButtonMode = index;
        controlSettingB[5].Value = index;
        SelectButton(controlSettingB[5].Buttons, index);
    }

    public void SetCRAWLButtonMode(int index) // Кнопка ПОЛЗАТЬ B[6]
    {
        GameData.crawlButtonMode = index;
        controlSettingB[6].Value = index;
        SelectButton(controlSettingB[6].Buttons, index);
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #endregion

    //////////////////////////////////////////////////////////////////////////////////////

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

    #region Помощь по настроке (INFO)

    public void OpenInfo(string info)
    {
        _InfoText.text = info;
        _infoPanel.SetActive(true);
    }

    public void CloseInfo()
    {
        _infoPanel.SetActive(false);
    }

    #endregion
}