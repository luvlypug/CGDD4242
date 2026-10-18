using System.Collections;
using UnityEngine;

public abstract class Ball : MonoBehaviour
{
    [SerializeField] public float speed;
    [SerializeField] public float minPower;
    [SerializeField] public float maxPower;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool canLaunch = true;
    [HideInInspector] public bool canAbility = false;
    [HideInInspector] public int points = 0;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
    [HideInInspector] public static int maxPoints = 100;
    [HideInInspector] public Vector3 currentDirection;
    [HideInInspector] public Vector3 launchDirection;
    [HideInInspector] public float power;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        canAbility = true;
    }

    public void Launch(Vector3 launchDirection, float power)
    {
        rb.linearVelocity = launchDirection * power * speed;
        //rb.AddForce(launchDirection * power * speed, ForceMode.Impulse);

        StartCoroutine(LaunchCooldown(power / (maxPower - minPower) + 0.1f));
    }

    public void Launch(float launchAngleInDegrees, float power)
    {
        float angleInRadians = launchAngleInDegrees * Mathf.Deg2Rad;

        float directionX = Mathf.Cos(angleInRadians);
        float directionZ = Mathf.Sin(angleInRadians);

        Vector3 launchDirection = new Vector3(directionX, 0, directionZ);

        rb.linearVelocity = launchDirection * power * speed;
        //rb.AddForce(launchDirection * power * speed, ForceMode.Impulse);

        StartCoroutine(LaunchCooldown(power / (maxPower - minPower) * 1.5f + 0.5f));
    }

    public abstract void Ability();

    public Vector3 GetCurrentDirection()
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 rollingDirection = velocity.normalized;
        Vector3 flatDirection = new Vector3(velocity.x, 0f, velocity.z).normalized;

        // Debug visualization (draws a green line showing the direction)
        if (rollingDirection.magnitude > 0.1f)
        {
            Debug.DrawRay(transform.position, rollingDirection * 2f, Color.green);
        }

        return flatDirection;
    }

    public IEnumerator LaunchCooldown(float seconds) 
    {
        canLaunch = false;
        yield return new WaitForSeconds(seconds);
        canLaunch = true;
    }
}