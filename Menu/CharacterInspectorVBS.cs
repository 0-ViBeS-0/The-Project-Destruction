using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterInspectorVBS : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private GameObject _object;
    private bool isDragging = false;
    private float rotationSpeed = 0.2f;
    private float previousMousePositionX;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        previousMousePositionX = eventData.position.x;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        float deltaX = eventData.position.x - previousMousePositionX;
        _object.transform.Rotate(Vector3.up, -deltaX * rotationSpeed, Space.World);
        previousMousePositionX = eventData.position.x;
    }
}
