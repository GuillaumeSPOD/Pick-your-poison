using UnityEngine;

public class DynamicImageMovement : MonoBehaviour
{
    public Canvas parentCanvas;

    void Update()
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out mousePos);

        gameObject.GetComponent<RectTransform>().anchoredPosition = mousePos*(-1)*0.05f;
    }
}
