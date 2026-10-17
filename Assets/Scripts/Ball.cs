using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Ball : MonoBehaviour
{
    [SerializeField] private float speed;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public float minPower = 1;
    [HideInInspector] public float maxPower = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        rb = GetComponent<Rigidbody>();

        Launch(45, 1);
    }

    public void Launch(Vector3 launchDirection, float power)
    {
        rb.linearVelocity = launchDirection * power * speed;
        //rb.AddForce(launchDirection * power * speed, ForceMode.Impulse);
    }

    public void Launch(float launchAngleInDegrees, float power)
    {
        float angleInRadians = launchAngleInDegrees * Mathf.Deg2Rad;

        float directionX = Mathf.Cos(angleInRadians);
        float directionZ = Mathf.Sin(angleInRadians);

        Vector3 launchDirection = new Vector3(directionX, 0f, directionZ);

        rb.linearVelocity = launchDirection * power * speed;
        //rb.AddForce(launchDirection * power * speed, ForceMode.Impulse);
    }
}