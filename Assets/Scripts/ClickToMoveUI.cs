using UnityEngine;
using UnityEngine.InputSystem;

public class ClickToMoveUI : MonoBehaviour
{
    [SerializeField] private RectTransform uiElementToMove;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            MoveUIObject();
        }
    }

    void MoveUIObject()
    {
        if (uiElementToMove == null) return;

        // 1. Get the immediate parent of your moving object
        RectTransform parentRect = uiElementToMove.parent as RectTransform;
        if (parentRect == null) return;

        // 2. Find the topmost main Canvas component
        Canvas canvas = uiElementToMove.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // 3. Read the mouse screen position
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector2 localPoint;

        // 4. Determine math based on how your canvas draws
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // For Overlay mode, standard translation works best
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mouseScreenPos, null, out localPoint);
        }
        else
        {
            // For Camera and World space, look up the active rendering camera automatically
            Camera targetCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

            // Bypass ScreenPointToLocalPoint if it is failing, use direct transform matrices instead
            Vector3 worldPos = targetCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, canvas.planeDistance));
            localPoint = parentRect.InverseTransformPoint(worldPos);
        }

        // 5. Apply the position flatly on the 2D plane
        uiElementToMove.anchoredPosition = localPoint;
    }
}