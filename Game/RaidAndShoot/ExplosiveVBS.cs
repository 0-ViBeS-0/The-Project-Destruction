using System.Collections;
using UnityEngine;

public class ExplosiveVBS : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Collider _collider;
    [SerializeField] private AudioSource _audioSource;
    [Space(10)]
    [SerializeField] private float _radiusForExplosionProps;
    [SerializeField] private LayerMask _propLayerMask;
    [SerializeField] private float _radiusForExplosionEntity;
    [SerializeField] private LayerMask _entityLayerMask;
    [Space(10)]
    [SerializeField] private float _timeToExplosion;
    [SerializeField] private GameObject _explosionFX;
    [SerializeField] private float _audioLenght;

    private Collider[] _propCollidersCache = new Collider[50];
    private Collider[] _entityCollidersCache = new Collider[50];

    private bool _isHit;

    private void OnCollisionEnter(Collision collision)
    {
        if (!_isHit)
        {
            _isHit = true;
            _rb.isKinematic = true;

            StartCoroutine(Explosion());
        }
    }

    private IEnumerator Explosion()
    {
        yield return new WaitForSeconds(_timeToExplosion);

        Instantiate(_explosionFX, transform.position, Quaternion.identity);
        _audioSource.Play();

        int propColliderCount = Physics.OverlapSphereNonAlloc(transform.position, _radiusForExplosionProps, _propCollidersCache, _propLayerMask);
        if (propColliderCount > 0)
        {
            for (int i = 0; i < propColliderCount; i++)
            {
                CheckProp(_propCollidersCache[i]);
            }
        }
        int entityColliderCount = Physics.OverlapSphereNonAlloc(transform.position, _radiusForExplosionEntity, _entityCollidersCache, _entityLayerMask);
        if (entityColliderCount > 0)
        {
            for (int i = 0; i < entityColliderCount; i++)
            {
                CheckEntity(_entityCollidersCache[i]);
            }
        }

        _meshRenderer.enabled = false;
        _collider.enabled = false;
        yield return new WaitForSeconds(_audioLenght);
        Destroy(gameObject);
    }

    private void CheckProp(Collider collider)
    {
        float distance = Vector3.Distance(transform.position, collider.transform.position);
        float damage = distance < 1 ? 100 : 100 / distance;

        BuildParent buildParent = collider.GetComponent<BuildParent>();
        if (buildParent)
        {
            BuildVBS build = buildParent.parent.GetComponent<BuildVBS>();
            if (build)
            {
                build.InflictDamage((int)damage);
            }
        }
    }

    private void CheckEntity(Collider collider)
    {
        float distance = Vector3.Distance(transform.position, collider.transform.position);
        float damage = distance < 1 ? 50 : 50 / distance;

        PlayerCharacteristicsVBS characteristics = collider.GetComponent<PlayerCharacteristicsVBS>();
        if (characteristics)
        {
            characteristics.InflictDamage((int)damage);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radiusForExplosionProps);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radiusForExplosionEntity);
    }
}