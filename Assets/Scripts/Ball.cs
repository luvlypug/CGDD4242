using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Ball : MonoBehaviour
{
    // Assign your Click/Press action from your Input Actions asset
    public InputActionReference clickAction;

    [SerializeField] private float speed;
    [SerializeField] private float aimTimeScale;

    private Rigidbody rb;
    private bool wasClickIgnored = false;

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
        clickAction.action.canceled += OnClickReleased;
    }

    private void OnDisable()
    {
        clickAction.action.performed -= OnClickPerformed;
        clickAction.action.canceled -= OnClickReleased;
        clickAction.action.Disable();
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        // Check if the initial click started over a UI button
        if (EventSystem.current != null && UIManager.Instance.isPaused)
        {
            wasClickIgnored = true;
        }
        else
        {
            wasClickIgnored = false;
            SlowDown();
        }
    }

    private void OnClickReleased(InputAction.CallbackContext context)
    {
        // If the click originally started on a UI button, ignore the release logic
        if (!wasClickIgnored)
        {
            SpeedUp();
        }
    }

    private void SlowDown()
    {
        UIManager.Instance.currentTimeScale = aimTimeScale;
        Time.timeScale = UIManager.Instance.currentTimeScale;
    }

    private void SpeedUp()
    {
        UIManager.Instance.currentTimeScale = 1;

        if (!wasClickIgnored && !UIManager.Instance.isPaused)
        {
            Time.timeScale = UIManager.Instance.currentTimeScale;
        }
    }
}