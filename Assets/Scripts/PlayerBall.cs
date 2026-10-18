using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerBall : Ball
{
    [SerializeField] private InputActionReference clickAction;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject powerBar;
    [SerializeField] private RectTransform powerBarRect;
    [SerializeField] private RectTransform powerBarTailRect;
    [SerializeField] private float uiMargin;
    [SerializeField] private float aimTimeScale;

    private Vector3 startMousePos;
    private Vector3 currentMousePos;
    private bool wasClickIgnored = false;
    private bool isAiming = false;
    private bool launchQueued = false;
    private Vector3 launchDirection;
    private float dragDistance;
    private float power;


    // Update is called once per frame
    void Update()
    {
        if (isAiming && !UIManager.Instance.isPaused)
        {
            if (launchQueued)
            {
                LaunchPlayer();
                launchQueued = false;
            }

            currentMousePos = GetMouseWorldPosition();

            Vector2 dragVector = startMousePos - currentMousePos;
            Vector3 normalizedLaunchDirection = dragVector.normalized;
            launchDirection = new Vector3(normalizedLaunchDirection.x, 0, normalizedLaunchDirection.y);

            dragDistance = dragVector.magnitude;
            power = Mathf.Max(minPower, Mathf.Min(dragDistance * 2, maxPower));

            powerBarTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (power - 1) / (maxPower - minPower) * 398 + 96);
        }
    }

    void OnEnable()
    {
        clickAction.action.Enable();
        clickAction.action.performed += OnClickPerformed;
        clickAction.action.canceled += OnClickReleased;
    }

    void OnDisable()
    {
        clickAction.action.performed -= OnClickPerformed;
        clickAction.action.canceled -= OnClickReleased;
        clickAction.action.Disable();
    }

    // Once left click is clicked
    void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (canLaunch)
        {
            if (EventSystem.current != null && UIManager.Instance.isPaused)
            {
                wasClickIgnored = true;
            }
            else
            {
                wasClickIgnored = false;
                isAiming = true;
                startMousePos = GetMouseWorldPosition();
                powerBar.SetActive(true);
                MoveToMousePos(powerBarRect);
                SlowDown();
            }
        }
    }

    // Once left click is released
    void OnClickReleased(InputAction.CallbackContext context)
    {
        if (isAiming)
        {
            if(UIManager.Instance.isPaused)
            {
                launchQueued = true;
            }
            else if (!wasClickIgnored)
            {
                LaunchPlayer();
            }
        }
    }

    // When right click is clicked
    public void OnCancelLaunch()
    {
        if (!UIManager.Instance.isPaused)
        {
            wasClickIgnored = true;
            isAiming = false;
            powerBar.SetActive(false);
            SpeedUp();
        }
    }

    void LaunchPlayer()
    {
        isAiming = false;
        powerBar.SetActive(false);
        Launch(launchDirection, power);
        SpeedUp();
    }

    void SlowDown()
    {
        UIManager.Instance.currentTimeScale = aimTimeScale;
        Time.timeScale = UIManager.Instance.currentTimeScale;
    }

    void SpeedUp()
    {
        UIManager.Instance.currentTimeScale = 1;

        if (!UIManager.Instance.isPaused)
        {
            Time.timeScale = UIManager.Instance.currentTimeScale;
        }
    }

    void MoveToMousePos(RectTransform rectTransform)
    {
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) return;

        // 1. Get raw screen mouse coordinates
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // 2. Convert Screen pixels to a normalized value (0 to 1) across the entire monitor
        float monitorXNorm = mouseScreenPos.x / Screen.width;
        float monitorYNorm = mouseScreenPos.y / Screen.height;

        // 3. Read your AspectRatioHandler limits from the camera's viewport rect
        Rect camRect = uiCamera.rect;

        // 4. AUTOMATIC FIX: Calculate padding based on the actual size of the UI object
        // This ensures the visual edges of the box stay clean out of the blackbars
        float halfObjectWidth = rectTransform.rect.width * 0.5f;
        float halfObjectHeight = rectTransform.rect.height * 0.5f;

        float totalXPaddingPixels = halfObjectWidth + uiMargin;
        float totalYPaddingPixels = halfObjectHeight + uiMargin;

        // Convert the structural pixel boundaries into normalized percentages
        float xMarginNorm = totalXPaddingPixels / Screen.width;
        float yMarginNorm = totalYPaddingPixels / Screen.height;

        // 5. Apply the margin to your camera bounds (inset the edges inward)
        float minX = camRect.xMin + xMarginNorm;
        float maxX = camRect.xMax - xMarginNorm;
        float minY = camRect.yMin + yMarginNorm;
        float maxY = camRect.yMax - yMarginNorm;

        if (minX > maxX) minX = maxX = camRect.center.x;
        if (minY > maxY) minY = maxY = camRect.center.y;

        // 6. Clamp the coordinates using our new margin-inset boundaries
        float clampedMonitorX = Mathf.Clamp(monitorXNorm, minX, maxX);
        float clampedMonitorY = Mathf.Clamp(monitorYNorm, minY, maxY);

        // 7. Convert those safe monitor screen percentages back into Canvas Pixels
        RectTransform canvasRectTransform = canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRectTransform.rect.size;

        Vector2 canvasLocalPoint = new Vector2(
            (clampedMonitorX - canvasRectTransform.pivot.x) * canvasSize.x,
            (clampedMonitorY - canvasRectTransform.pivot.y) * canvasSize.y
        );

        // 8. Translate safely from the main Canvas to your object's specific parent space
        Vector3 worldPosOnCanvas = canvasRectTransform.TransformPoint(canvasLocalPoint);
        Vector3 finalParentLocalPoint = parentRect.InverseTransformPoint(worldPosOnCanvas);

        // 9. FORCE FLAT 2D: Reset Z completely so it never falls behind or clips under the camera plane
        rectTransform.anchoredPosition3D = new Vector3(finalParentLocalPoint.x, finalParentLocalPoint.y, 0f);
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 screenPosWithDepth = new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosWithDepth);
        worldPos.z = 0f;

        return worldPos;
    }
}
