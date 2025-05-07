using System;
using UnityEngine;
using UnityEngine.Audio;

public class WeatherManagerVBS : MonoBehaviour
{
    [Tooltip("Camera the rain should hover over, defaults to main camera")]
    public Camera Camera;

    [Tooltip("Whether rain should follow the camera. If false, rain must be moved manually and will not follow the camera.")]
    public bool FollowCamera = true;

    [Tooltip("Light rain looping clip")]
    public AudioClip RainSoundLight;

    [Tooltip("Medium rain looping clip")]
    public AudioClip RainSoundMedium;

    [Tooltip("Heavy rain looping clip")]
    public AudioClip RainSoundHeavy;

    [Tooltip("AudioMixer used for the rain sound")]
    public AudioMixerGroup RainSoundAudioMixer;

    [Tooltip("Intensity of rain (0-1)")]
    [Range(0.0f, 1.0f)]
    public float RainIntensity;

    [Tooltip("Rain particle system")]
    public ParticleSystem RainFallParticleSystem;

    [Tooltip("Particles system for when rain hits something")]
    public ParticleSystem RainExplosionParticleSystem;

    [Tooltip("Particle system to use for rain mist")]
    public ParticleSystem RainMistParticleSystem;

    [Tooltip("The threshold for intensity (0 - 1) at which mist starts to appear")]
    [Range(0.0f, 1.0f)]
    public float RainMistThreshold = 0.5f;

    [Tooltip("Wind looping clip")]
    public AudioClip WindSound;

    [Tooltip("Wind sound volume modifier, use this to lower your sound if it's too loud.")]
    public float WindSoundVolumeModifier = 0.5f;

    [Tooltip("Wind zone that will affect and follow the rain")]
    public WindZone WindZone;

    [Tooltip("X = minimum wind speed. Y = maximum wind speed. Z = sound multiplier.")]
    public Vector3 WindSpeedRange = new Vector3(50.0f, 500.0f, 500.0f);

    [Tooltip("How often the wind speed and direction changes (min and max change interval in seconds)")]
    public Vector2 WindChangeInterval = new Vector2(5.0f, 30.0f);

    [Tooltip("The height above the camera that the rain will start falling from")]
    public float RainHeight = 25.0f;

    [Tooltip("How far the rain particle system is ahead of the player")]
    public float RainForwardOffset = -7.0f;

    [Tooltip("The top y value of the mist particles")]
    public float RainMistHeight = 3.0f;

    private LoopingAudioSource audioSourceRainLight;
    private LoopingAudioSource audioSourceRainMedium;
    private LoopingAudioSource audioSourceRainHeavy;
    private LoopingAudioSource audioSourceRainCurrent;
    private LoopingAudioSource audioSourceWind;
    private float lastRainIntensityValue = -1.0f;
    private float nextWindTime;

    private void Start()
    {
        audioSourceRainLight = new LoopingAudioSource(this, RainSoundLight, RainSoundAudioMixer);
        audioSourceRainMedium = new LoopingAudioSource(this, RainSoundMedium, RainSoundAudioMixer);
        audioSourceRainHeavy = new LoopingAudioSource(this, RainSoundHeavy, RainSoundAudioMixer);
        audioSourceWind = new LoopingAudioSource(this, WindSound, RainSoundAudioMixer);

        InitializeParticleSystem(RainFallParticleSystem);
        InitializeParticleSystem(RainExplosionParticleSystem);
        InitializeParticleSystem(RainMistParticleSystem);
    }

    private void Update()
    {
        UpdateRain();
        UpdateWind();
        CheckForRainChange();

        audioSourceRainLight.Update();
        audioSourceRainMedium.Update();
        audioSourceRainHeavy.Update();
        audioSourceWind.Update();
    }

    private void InitializeParticleSystem(ParticleSystem particleSystem)
    {
        if (particleSystem != null)
        {
            var emission = particleSystem.emission;
            emission.enabled = false;
            var renderer = particleSystem.GetComponent<Renderer>();
            renderer.enabled = false;
        }
    }

    private void UpdateRain()
    {
        if (RainFallParticleSystem != null && FollowCamera)
        {
            var shape = RainFallParticleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.ConeVolume;
            RainFallParticleSystem.transform.position = Camera.transform.position + new Vector3(0.0f, RainHeight, RainForwardOffset);
            RainFallParticleSystem.transform.rotation = Quaternion.Euler(0.0f, Camera.transform.rotation.eulerAngles.y, 0.0f);

            if (RainMistParticleSystem != null)
            {
                var mistShape = RainMistParticleSystem.shape;
                mistShape.shapeType = ParticleSystemShapeType.Hemisphere;
                RainMistParticleSystem.transform.position = Camera.transform.position + new Vector3(0, RainMistHeight, 0);
            }
        }
    }

    private void UpdateWind()
    {
        if (EnableWind())
        {
            WindZone.gameObject.SetActive(true);
            WindZone.transform.position = Camera.transform.position + (Camera.orthographic ? Vector3.zero : new Vector3(0.0f, WindZone.radius, 0.0f));

            if (Time.time > nextWindTime)
            {
                WindZone.windMain = UnityEngine.Random.Range(WindSpeedRange.x, WindSpeedRange.y);
                WindZone.windTurbulence = WindZone.windMain;
                WindZone.transform.rotation = Camera.orthographic
                    ? Quaternion.Euler(0.0f, UnityEngine.Random.Range(0, 2) == 0 ? 90.0f : -90.0f, 0.0f)
                    : Quaternion.Euler(UnityEngine.Random.Range(-30.0f, 30.0f), UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);

                nextWindTime = Time.time + UnityEngine.Random.Range(WindChangeInterval.x, WindChangeInterval.y);
                audioSourceWind.Play(WindZone.windMain / WindSpeedRange.z * WindSoundVolumeModifier);
            }
        }
        else
        {
            WindZone?.gameObject.SetActive(false);
            audioSourceWind.Stop();
        }
    }

    private bool EnableWind() => WindZone != null && WindSpeedRange.y > 1.0f;

    private void CheckForRainChange()
    {
        if (Math.Abs(lastRainIntensityValue - RainIntensity) > 0.01f)
        {
            lastRainIntensityValue = RainIntensity;
            if (RainIntensity <= 0.01f) StopRain();
            else StartRain();
        }
    }

    private void StopRain()
    {
        audioSourceRainCurrent?.Stop();
        RainFallParticleSystem?.Stop();
        RainMistParticleSystem?.Stop();
    }

    private void StartRain()
    {
        audioSourceRainCurrent?.Stop();
        audioSourceRainCurrent = RainIntensity >= 0.67f ? audioSourceRainHeavy : RainIntensity >= 0.33f ? audioSourceRainMedium : audioSourceRainLight;
        audioSourceRainCurrent.Play(1.0f);

        SetParticleEmission(RainFallParticleSystem, RainFallEmissionRate());
        SetParticleEmission(RainMistParticleSystem, RainIntensity >= RainMistThreshold ? MistEmissionRate() : 0.0f);
    }

    private void SetParticleEmission(ParticleSystem particleSystem, float emissionRate)
    {
        if (particleSystem != null)
        {
            var emission = particleSystem.emission;
            emission.enabled = particleSystem.GetComponent<Renderer>().enabled = true;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(emissionRate);
            if (!particleSystem.isPlaying) particleSystem.Play();
        }
    }

    private float RainFallEmissionRate() => (RainFallParticleSystem.main.maxParticles / RainFallParticleSystem.main.startLifetime.constant) * RainIntensity;

    private float MistEmissionRate() => (RainMistParticleSystem.main.maxParticles / RainMistParticleSystem.main.startLifetime.constant) * RainIntensity * RainIntensity;
}

public class LoopingAudioSource
{
    private AudioSource audioSource;
    private float targetVolume;

    public LoopingAudioSource(MonoBehaviour script, AudioClip clip, AudioMixerGroup mixer)
    {
        audioSource = script.gameObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = mixer;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.0f;
        audioSource.Stop();
        targetVolume = 1.0f;
    }

    public void Play(float targetVolume)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.volume = 0.0f;
            audioSource.Play();
        }
        this.targetVolume = targetVolume;
    }

    public void Stop()
    {
        targetVolume = 0.0f;
    }

    public void Update()
    {
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime);
        if (audioSource.volume == 0.0f && audioSource.isPlaying) audioSource.Stop();
    }
}
