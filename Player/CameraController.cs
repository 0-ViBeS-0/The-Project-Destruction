using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControllerMobile : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector] public PlayerControllerVBS playerController;
    private Vector2 lastPosition;
    private bool isDragging;

    private void Start()
    {
        lastPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 delta = eventData.position - lastPosition;
        lastPosition = eventData.position;

        float horizontal = delta.x / 50;
        float vertical = delta.y / 50;

        playerController.RotateInputMobile(horizontal, vertical);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPosition = eventData.position;
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        lastPosition = Vector2.zero;
        isDragging = false;
    }
}