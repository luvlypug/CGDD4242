using UnityEngine;

public class Bumper : Point
{
    [SerializeField] private float blastPower;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            Vector3 blastPos = transform.position;
            Rigidbody collisionRB = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 direction = collision.gameObject.transform.position - blastPos;
            direction.y = 0;
            collisionRB.AddForce(direction.normalized * blastPower, ForceMode.Impulse);
        }
    }
}