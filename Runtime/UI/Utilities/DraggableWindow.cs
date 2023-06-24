using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform windowTransform;
    private Vector2 offset;

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(windowTransform, eventData.position, eventData.pressEventCamera, out offset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(windowTransform.parent.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out newPosition))
        {
            windowTransform.localPosition = newPosition - offset;
        }
    }
}
