using System;
using UnityEngine;

[ExecuteInEditMode]
public class DayManagerVBS : MonoBehaviour
{
    [Header("Time")]
    [Range(0f, 24f)] public float timeOfDay;
    [SerializeField] private float _dayDuration; // Продолжительность дня в секундах

    [Header("Light")]
    [SerializeField] private Gradient _dayNightGradient;
    [SerializeField] private Light _sun;
    [SerializeField] private float _sunIntensity; // Максимальная интенсивность солнца
    [SerializeField] private Light _moon;
    [SerializeField] private float _moonIntensity; // Максимальная интенсивность луны

    [Header("Skybox")]
    public bool canChangeSkybox = true; // Переменная для контроля изменения skybox
    [SerializeField] private Material _daySkybox;
    [SerializeField] private Material _nightSkybox;
    [SerializeField] private float _skyboxLerpSpeed; // Скорость перехода skybox

    private float currentSkyboxLerp;

    [Header("Stars")]
    public bool canChangeStars = true; // Переменная для контроля появления звезд
    [SerializeField] private ParticleSystem _stars;

    [Header("Fog")]
    public bool canChangeFog = true; // Переменная для контроля изменения тумана
    [SerializeField] private Gradient _fogGradient;
    [SerializeField] private float _fogLerpSpeed; // Скорость перехода для тумана

    private void Update()
    {
        if (Application.isPlaying)
        {
            // Обновление времени суток
            timeOfDay += (24f / _dayDuration) * Time.deltaTime;
            if (timeOfDay >= 24) timeOfDay = 0; // Сброс времени на 0 после 24
        }

        UpdateLighting();
        UpdateSunSource();
        UpdateSkybox();
        UpdateStars();
        UpdateFog();
    }

    private void UpdateLighting()
    {
        // Плавное изменение интенсивности света и его цвета в зависимости от времени суток
        float sunFactor = Mathf.Clamp01(1 - Mathf.Abs((timeOfDay - 12) / 12)); // Яркость солнца
        float moonFactor = Mathf.Clamp01(Mathf.Abs((timeOfDay - 12) / 12)); // Яркость луны

        _sun.intensity = Mathf.Lerp(0, _sunIntensity, sunFactor);
        _moon.intensity = Mathf.Lerp(0, _moonIntensity, moonFactor);

        // Изменение цвета солнца в зависимости от времени дня
        _sun.color = _dayNightGradient.Evaluate(Mathf.InverseLerp(0, 24, timeOfDay));

        // Вращение для солнца и луны
        float sunRotation = (timeOfDay - 6) * 15f; // Солнце восходит в 6:00 и заходит в 18:00
        float moonRotation = (timeOfDay - 18) * 15f; // Луна восходит в 18:00 и заходит в 6:00

        _sun.transform.rotation = Quaternion.Euler(sunRotation, 170, 0); // Вращение солнца
        _moon.transform.rotation = Quaternion.Euler(moonRotation, 170, 0); // Вращение луны
    }

    private void UpdateSunSource()
    {
        RenderSettings.sun = timeOfDay >= 6 && timeOfDay <= 18 ? _sun : _moon;
    }

    private void UpdateSkybox()
    {
        if (canChangeSkybox)
        {
            float targetLerp = Mathf.Clamp01(1 - Mathf.Abs((timeOfDay - 12) / 12));
            currentSkyboxLerp = Mathf.Lerp(currentSkyboxLerp, targetLerp, Time.deltaTime * _skyboxLerpSpeed);
            RenderSettings.skybox.Lerp(_nightSkybox, _daySkybox, currentSkyboxLerp);
            DynamicGI.UpdateEnvironment();
        }
    }

    private void UpdateStars()
    {
        if (canChangeStars)
        {
            if (timeOfDay >= 21 || timeOfDay <= 6)
            {
                if (!_stars.isPlaying) _stars.Play();
            }
            else
            {
                if (_stars.isPlaying) _stars.Stop();
            }
        }
    }

    private void UpdateFog()
    {
        if (canChangeFog)
        {
            RenderSettings.fogColor = _fogGradient.Evaluate(Mathf.InverseLerp(0, 24, timeOfDay));
        }
    }
}