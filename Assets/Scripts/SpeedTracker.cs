using UnityEngine;

public class SpeedTracker : MonoBehaviour
{
    public float PreviousSpeed { get; private set; }

    private Rigidbody rb;
    private float lastSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        PreviousSpeed = lastSpeed;
        lastSpeed = rb.linearVelocity.magnitude;
    }

}