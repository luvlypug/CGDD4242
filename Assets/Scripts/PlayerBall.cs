using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerBall : Ball
{
    public InputActionReference clickAction;

    private Vector3 startMousePos;
    private Vector3 currentMousePos;
    private bool wasClickIgnored = false;
    private bool isAiming = false;

    [SerializeField] private float aimTimeScale;

    // Update is called once per frame
    void Update()
    {
        if (isAiming && !UIManager.Instance.isPaused)
        {
            currentMousePos = GetMouseWorldPosition();
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

    // Once left click is released
    void OnClickReleased(InputAction.CallbackContext context)
    {
        if (!wasClickIgnored)
        {
            isAiming = false;

            Debug.Log("Mouse released at: " + currentMousePos.x + ", " + currentMousePos.y);

            Vector2 dragVector = startMousePos - currentMousePos;
            Vector3 normalizedDaunchDirection = dragVector.normalized;
            Vector3 launchDirection = new Vector3(normalizedDaunchDirection.x, 0, normalizedDaunchDirection.y);

            float dragDistance = dragVector.magnitude;
            float power = Mathf.Max(minPower, Mathf.Min(dragDistance * 2, maxPower));

            Debug.Log("X: " + launchDirection.x + " Z: " + launchDirection.z + " power: " + power);

            Launch(launchDirection, power);
            SpeedUp();
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
