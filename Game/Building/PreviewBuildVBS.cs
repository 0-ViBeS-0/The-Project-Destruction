using UnityEngine;

public class PreviewBuildVBS : MonoBehaviour
{
    #region Переменные

    private BuilderVBS _builder;
    [SerializeField] private Material _canMaterial;
    [SerializeField] private Material _cantMaterial;
    [SerializeField] private Renderer _materialPreviewPrefab;

    [SerializeField] private Transform _sphereContainer;
    [SerializeField] private float _sphereRadius;
    private LayerMask _currentLayerMask;
    [SerializeField] private LayerMask _defaultLayerMask;
    [SerializeField] private bool _needEditLayerOnPoint;
    [SerializeField] private LayerMask _editLayerMask;

    [SerializeField] private bool _needMyNerritory;
    [SerializeField] private LayerMask _territoryLayer;
    [SerializeField] private bool _isCreatorTerritory;
    [SerializeField] private float _distanceToCreateTerritory;

    private BuildDataSO _PREbuild;
    private bool _isInTerritory;
    private Collider[] _collidersCache = new Collider[50];
    private Collider[] _collidersTerr = new Collider[5];

    #endregion

    #region Awake/Update

    private void Awake()
    {
        if (!_sphereContainer)
            _sphereContainer = transform;
        if (!_needEditLayerOnPoint)
            _currentLayerMask = _defaultLayerMask;
    }

    private void Update()
    {
        if (!_builder) return;

        if (_needEditLayerOnPoint) LayerEditor();

        if (_needMyNerritory)
            CheckTerritory();

        if (_builder.territoryObject)
        {
            if (_isInTerritory && _builder.isMyTerritory)
            {
                CheckToBuild();
            }
            else
            {
                SetMaterial(_cantMaterial);
                _builder.canBuild = false;
            }
        }
        else
        {
            if (!_isInTerritory && _isCreatorTerritory && CanBuildTerritory())
            {
                CheckToBuild();
            }
            else
            {
                SetMaterial(_cantMaterial);
                _builder.canBuild = false;
            }
        }
    }

    #endregion

    #region Проверка для строительства

    private void CheckToBuild()
    {
        int colliderCount = Physics.OverlapSphereNonAlloc(_sphereContainer.position, _sphereRadius, _collidersCache, _currentLayerMask);
        bool canBuild = false;

        if (colliderCount > 0)
        {
            canBuild = CheckBuildConditions(_collidersCache, colliderCount);
        }
        else if (_PREbuild.CanBuildOnlyOnPoint && _builder.isInPoint)
        {
            canBuild = true;
        }

        SetMaterial(canBuild ? _canMaterial : _cantMaterial);
        _builder.canBuild = canBuild;
    }

    private bool CheckBuildConditions(Collider[] colliders, int count)
    {
        if (_PREbuild.CanBuildOnlyOnGround && AreAllCollidersOnLayer(colliders, count, 6))
            return true;

        if (_PREbuild.CanBuildOnlyOnBuilds && AreAllCollidersOnLayer(colliders, count, 10))
            return true;

        if (_PREbuild.CanBuildOnlyOnPoint && _builder.isInPoint && !AreAnyCollidersOnLayer(colliders, count, 13))
            return true;

        if (_PREbuild.CanBuildOnAll)
            return true;

        return false;
    }

    private void CheckTerritory()
    {
        int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, 0.1f, _collidersCache, _territoryLayer);

        if (colliderCount > 0)
        {
            var territoryManager = _collidersCache[0].GetComponent<TerritoryManagerVBS>();
            _builder.isMyTerritory = territoryManager?.owner == GameData.MyName;
            _isInTerritory = true;
        }
        else
        {
            _isInTerritory = false;
            SetMaterial(_cantMaterial);
            _builder.canBuild = false;
        }
    }

    private bool CanBuildTerritory()
    {
        int colliderCount = Physics.OverlapSphereNonAlloc(transform.position, _distanceToCreateTerritory, _collidersTerr, _territoryLayer);

        if (colliderCount > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    #endregion

    #region Иницилизация

    public void SetPreviewComponents(BuilderVBS builder, BuildDataSO buildData)
    {
        _builder = builder;
        _PREbuild = buildData;
    }

    private void SetMaterial(Material material)
    {
        if (_materialPreviewPrefab != null)
            _materialPreviewPrefab.material = material;
    }

    #endregion

    #region Вспомогательные методы

    private bool AreAllCollidersOnLayer(Collider[] colliders, int count, int layer)
    {
        for (int i = 0; i < count; i++)
        {
            if (colliders[i] != null && colliders[i].gameObject.layer == layer)
                continue;

            return false;
        }
        return true;
    }

    private bool AreAnyCollidersOnLayer(Collider[] colliders, int count, int layer)
    {
        for (int i = 0; i < count; i++)
        {
            if (colliders[i] != null && colliders[i].gameObject.layer == layer)
                return true;
        }
        return false;
    }

    #endregion

    #region Другое

    private void LayerEditor()
    {
        _currentLayerMask = _builder.isInPoint ? _editLayerMask : _defaultLayerMask;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_sphereContainer.position, _sphereRadius);
    }

    #endregion
}