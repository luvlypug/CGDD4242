using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Ball : MonoBehaviour
{
    // Assign your Click/Press action from your Input Actions asset
    public InputActionReference clickAction;

    [SerializeField] private float speed;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Launch(45, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Launch(float angleInDegrees, float power)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        float directionX = Mathf.Cos(angleInRadians);
        float directionZ = Mathf.Sin(angleInRadians);

        Vector3 launchDirection = new Vector3(directionX, 0f, directionZ);

        rb.linearVelocity = launchDirection * power * speed;
        //rb.AddForce(launchDirection * power * speed, ForceMode.Impulse);
    }

    private void OnEnable()
    {
        clickAction.action.Enable();
        clickAction.action.performed += OnClickPerformed;
    }

    private void OnDisable()
    {
        clickAction.action.performed -= OnClickPerformed;
        clickAction.action.Disable();
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        // Check if the pointer/mouse is over a UI element like a button
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; // Exit if clicking on UI
        }

        // For touch input, you can also check touch ID finger index if needed:
        // if (EventSystem.current.IsPointerOverGameObject(Touchscreen.current.primaryTouch.touchId.ReadValue())) return;

        SlowDown();
    }

    private void SlowDown()
    {
        Time.timeScale = 0.5f;
    }

}