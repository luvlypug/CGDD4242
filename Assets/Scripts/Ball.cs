using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Launch(90, 1);
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

}
