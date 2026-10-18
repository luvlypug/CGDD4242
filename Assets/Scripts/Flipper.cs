using System.Collections;
using UnityEngine;

public class Flipper : Point
{
    [SerializeField] private float flipSpeed;
    private HingeJoint hinge;

    void Start()
    {
        hinge = GetComponent<HingeJoint>();
        hinge.useMotor = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            Coroutine coroutine = StartCoroutine(FlipFlipper());
        }
    }

    // Call this to forcefully fling the lever forward
    public void FlipperUp()
    {
        JointMotor motor = hinge.motor;
        motor.targetVelocity = flipSpeed;
        motor.force = 10000f;
        hinge.motor = motor;
        hinge.useMotor = true;
    }

    // Call this to reset it
    public void FlipperDown()
    {
        JointMotor motor = hinge.motor;
        motor.targetVelocity = -flipSpeed;
        hinge.motor = motor;
        hinge.useMotor = true;
    }

    public IEnumerator FlipFlipper()
    {
        FlipperUp();

        yield return new WaitForSeconds(0.2f);

        FlipperDown();
    }
}
