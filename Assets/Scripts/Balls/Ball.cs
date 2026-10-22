using System.Collections;
using UnityEngine;

public abstract class Ball : MonoBehaviour
{
    public float speed;
    public float minPower;
    public float maxPower;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool canLaunch = true;
    [HideInInspector] public bool canAbility = false;
    [HideInInspector] public int points = 0;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
    [HideInInspector] public static int maxPoints = 100;
    [HideInInspector] public Vector3 currentDirection;
    [HideInInspector] public Vector3 launchDirection;
    [HideInInspector] public float power;
    [HideInInspector] public bool isEliminated;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //StartCoroutine(LaunchCooldown(1));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Point")
        {
            points += collision.gameObject.GetComponent<Point>().amount;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Deadzone")
        {
            canLaunch = false;
        }

        if (collider.gameObject.name == "End")
        {
            isEliminated = true;
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "Deadzone")
        {
            canLaunch = true;
        }
    }

    public void Launch(Vector3 launchDirection, float power, bool useCooldown = true)
    {
        rb.linearVelocity = launchDirection * power * speed;
        //rb.AddForce(launchDirection * power * speed, ForceMode.Impulse);

        if (useCooldown)
        {
            StartCoroutine(LaunchCooldown(power / (maxPower - minPower) * 1.5f + 0.5f));
        }
    }

    public void Launch(float launchAngleInDegrees, float power, bool useCooldown = true)
    {
        float angleInRadians = launchAngleInDegrees * Mathf.Deg2Rad;

        float directionX = Mathf.Cos(angleInRadians);
        float directionZ = Mathf.Sin(angleInRadians);

        Vector3 launchDirection = new Vector3(directionX, 0, directionZ);

        rb.linearVelocity = launchDirection * power * speed;
        //rb.AddForce(launchDirection * power * speed, ForceMode.Impulse);

        if (useCooldown)
        {
            StartCoroutine(LaunchCooldown(power / (maxPower - minPower) * 1.5f + 0.5f));
        }
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