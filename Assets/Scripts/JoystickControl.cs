using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickControl : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public RectTransform background;
    public RectTransform handle;
    public PlayerController player;
    public float limit = 110f;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos = Vector2.ClampMagnitude(pos, limit);
            handle.anchoredPosition = new Vector2(pos.x, 0f);

            float inputX = pos.x / limit;

            player.SetMobileMove(inputX);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        handle.anchoredPosition = Vector2.zero;
        player.SetMobileMove(0f);
    }
}