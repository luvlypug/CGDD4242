using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerBall : Ball
{
    [SerializeField] private InputActionReference clickAction;
    [SerializeField] private GameObject powerBar;
    [SerializeField] private RectTransform powerBarTail;
    [SerializeField] private float aimTimeScale;

    private Vector3 startMousePos;
    private Vector3 currentMousePos;
    private bool wasClickIgnored = false;
    private bool isAiming = false;
    private Vector3 launchDirection;
    private float dragDistance;
    private float power;


    // Update is called once per frame
    void Update()
    {
        if (isAiming && !UIManager.Instance.isPaused)
        {
            currentMousePos = GetMouseWorldPosition();

            Vector2 dragVector = startMousePos - currentMousePos;
            Vector3 normalizedDaunchDirection = dragVector.normalized;
            launchDirection = new Vector3(normalizedDaunchDirection.x, 0, normalizedDaunchDirection.y);

            dragDistance = dragVector.magnitude;
            float power = Mathf.Max(minPower, Mathf.Min(dragDistance * 2, maxPower));

            powerBarTail.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (power - 1) / (maxPower - minPower) * 398 + 96);
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
                Debug.Log("Mouse clicked at: " + startMousePos.x + ", " + startMousePos.y);
                SlowDown();
            }
        }
    }

    // Once left click is released
    void OnClickReleased(InputAction.CallbackContext context)
    {
        if (canLaunch)
        {
            if (!wasClickIgnored)
            {
                isAiming = false;

                Debug.Log("Mouse released at: " + currentMousePos.x + ", " + currentMousePos.y);

                

                Debug.Log("X: " + launchDirection.x + " Z: " + launchDirection.z + " power: " + power);

                Launch(launchDirection, power);
                SpeedUp();
            }
        }
    }

    // When right click is clicked
    public void OnCancelLaunch()
    {
        wasClickIgnored = true;
        isAiming = false;
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

    Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 screenPosWithDepth = new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPosWithDepth);
        worldPos.z = 0f;

        return worldPos;
    }
}
