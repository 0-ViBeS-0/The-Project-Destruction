using UnityEngine;

public class TacticalMarkersVBS : MonoBehaviour
{
    [SerializeField] private CompassVBS _compass;
    [SerializeField] private GameObject _markerPrefab;
    [HideInInspector] public Transform cameraTransform;
    [SerializeField] private float _offsetY;
    [SerializeField] private LayerMask _layerMask;

    private GameObject _lastMarker;
    private RaycastHit _hit;

    public void PlaceMarker()
    {
        Ray ray = new(cameraTransform.transform.position, cameraTransform.transform.forward);
        if (Physics.Raycast(ray, out _hit, 500f, _layerMask))
        {
            if (_lastMarker) 
            { 
                _compass.RemoveQuestMark(_lastMarker.GetComponent<QuestMarkVBS>());
                Destroy(_lastMarker); 
            }

            Vector3 markerLocation = new(_hit.point.x, _offsetY, _hit.point.z);
            _lastMarker = Instantiate(_markerPrefab, markerLocation, Quaternion.identity);
            _compass.AddQuestMark(_lastMarker.GetComponent<QuestMarkVBS>());
        }
    }
}
