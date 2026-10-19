using TMPro;
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
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private float abilityPower;
    [SerializeField] private float aimTimeScale;
    // [SerializeField] private float uiMargin;

    private Vector3 startMousePos;
    private Vector3 currentMousePos;
    private bool wasClickIgnored = false;
    private bool isAiming = false;
    private bool launchQueued = false;

    // Update is called once per frame
    void Update()
    {
        currentDirection = GetCurrentDirection();

        if (points >= maxPoints)
        {
            points = maxPoints;
            canAbility = true;
        }

        pointsText.text = points.ToString();

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

            float dragDistance = dragVector.magnitude;
            power = Mathf.Max(minPower, Mathf.Min(dragDistance * 5, maxPower));

            powerBarRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(launchDirection.z, launchDirection.x) * Mathf.Rad2Deg + 180);
            powerBarTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (power - 1) / (maxPower - minPower) * 240 + 96);
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

    // When spacebar is pressed
    public override void Ability()
    {
        if (canAbility && !UIManager.Instance.isPaused)
        {
            Launch(currentDirection, abilityPower, false);

            points = 0;
            canAbility = false;
        }
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

        RectTransform canvasRectTransform = canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRectTransform.rect.size;

        Rect camRect = uiCamera.rect;

        float halfObjectWidth = 200;
        float halfObjectHeight = 200;

        float totalXPaddingUnits = halfObjectWidth; // + uiMargin;
        float totalYPaddingUnits = halfObjectHeight; // + uiMargin;

        float xMarginNorm = totalXPaddingUnits / canvasSize.x;
        float yMarginNorm = totalYPaddingUnits / canvasSize.y;

        float minX = camRect.xMin + xMarginNorm;
        float maxX = camRect.xMax - xMarginNorm;
        float minY = camRect.yMin + yMarginNorm;
        float maxY = camRect.yMax - yMarginNorm;

        if (minX > maxX) minX = maxX = camRect.center.x;
        if (minY > maxY) minY = maxY = camRect.center.y;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        float monitorXNorm = mouseScreenPos.x / Screen.width;
        float monitorYNorm = mouseScreenPos.y / Screen.height;

        float clampedMonitorX = Mathf.Clamp(monitorXNorm, minX, maxX);
        float clampedMonitorY = Mathf.Clamp(monitorYNorm, minY, maxY);

        Vector2 canvasLocalPoint = new Vector2(
            (clampedMonitorX - canvasRectTransform.pivot.x) * canvasSize.x,
            (clampedMonitorY - canvasRectTransform.pivot.y) * canvasSize.y
        );

        Vector3 worldPosOnCanvas = canvasRectTransform.TransformPoint(canvasLocalPoint);
        Vector3 finalParentLocalPoint = parentRect.InverseTransformPoint(worldPosOnCanvas);

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