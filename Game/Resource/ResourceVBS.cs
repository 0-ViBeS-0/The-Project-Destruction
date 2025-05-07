using System.Collections;
using UnityEngine;

public class ResourceVBS : MonoBehaviour
{
    #region Переменные

    [Header("|-# COMPONENTS")]
    [SerializeField] private Collider _collider;
    private ResourceManagerVBS _resourceManagerVBS;

    [Header("|-# OBJECT")]
    public int health;
    [SerializeField] private int _maxHealth;
    [SerializeField] private GameObject _hitFX;
    [SerializeField] private GameObject _destroyFX;
    [SerializeField] private AudioClip[] _hitSounds;
    [SerializeField] private AudioClip _destroyClip;
    private AudioSource _audioSource;
    [SerializeField] private GameObject[] _mesh;

    [Header("|-# SETTINGS")]
    public ItemSO resource;
    public ItemSO[] instrumentsForGather;
    [SerializeField] private float _timeForRespawn;
    [SerializeField] private LayerMask _territoryLayer;
    private Collider[] _collidersTerr = new Collider[5];

    #endregion

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _resourceManagerVBS = GetComponentInParent<ResourceManagerVBS>();
    }

    public void Damage(int damage)
    {
        _resourceManagerVBS.ResourceDamage(gameObject, damage);
    }

    private IEnumerator TimeForSpawn(float time)
    {
        for (int i = 0; i < _mesh.Length; i++)
        {
            _mesh[i].SetActive(false);
        }
        _collider.enabled = false;
        health = 0;

        yield return new WaitForSeconds(time);

        int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, 0.1f, _collidersTerr, _territoryLayer);

        if (colliderCount > 0)
        {
            StartCoroutine(TimeForSpawn(_timeForRespawn * 4));
        }
        else
        {
            for (int i = 0; i < _mesh.Length; i++)
            {
                _mesh[i].SetActive(true);
            }
            _collider.enabled = true;
            health = _maxHealth;
        }
    }

    public GameObject GetHitFX()
    {
        return _hitFX;
    }

    public void ResurceDamage(int damage)
    {
        if (health <= 0) return;

        health -= damage;
        if (_hitSounds != null) PlaySound(_hitSounds[Random.Range(0, _hitSounds.Length)]);

        if (health <= 0)
        {
            StartCoroutine(TimeForSpawn(_timeForRespawn));
            PlaySound(_destroyClip);
            if (GameData.particles) Instantiate(_destroyFX, transform.position, Quaternion.LookRotation(Vector3.down));
        }
    }

    private void PlaySound(AudioClip randomClip)
    {
        _audioSource.clip = randomClip;
        _audioSource.Play();
    }
}