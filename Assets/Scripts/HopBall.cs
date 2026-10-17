using UnityEngine;
using System.Collections.Generic;

// rin code hi 

public class HopBall : MonoBehaviour
{
    private Rigidbody rb;
    // private Ball ball;

    // launch directions 
    public int rayCount = 24;

    // launch info
    public float launchPower = 10f;
    private Vector3 bestDirection; 

    // timer for launchinf
    private float launchTimer;
    public float launchCooldown = 3f; 

    // how far sensors can look
    public float scanDist = 20f; 

    // decision variables
    public float riskTolerance = 0.1f; 
    public float threatRadius = 10f;
    public float collisionPrediction = 1f;
    public float threatTimer;
    public float threatCheckCooldown = 0.1f;

    // ability / skill 
    public int abilityCost = 10; 
    public int points = 20; // starting with some points for testing purposes :3 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // ball = GetComponent<Ball>();
    }

    // Update is called once per frame
    void Update()
    {
        threatTimer += Time.deltaTime;
        launchTimer += Time.deltaTime;

        // dont want threats to be called each frame probably maybe
        if (threatTimer >= threatCheckCooldown)
        {
            threatTimer = 0f;
            CheckIncomming(); 
        }

        // launch based on timer yayyy
        if (launchTimer > launchCooldown)
        {
            launchTimer = 0f;

            ChooseBestDirection();
            Launch();
        }
    }

    void CheckIncomming()
    {
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");

        foreach (GameObject ball in balls)
        { 
            // we dont care about ourselves so skip #yes
            if (ball ==  gameObject)
            {
                continue; // skip rest and move on 
            }

            Rigidbody enemyRb = ball.GetComponent<Rigidbody>();

            if (enemyRb == null)
            {
                continue; // skip rest and move on 
            }

            float distance = Vector3.Distance(transform.position, ball.transform.position);

            // also if its not a threat then ignore too dghiufdhujrfoujihfffffhhhhhhhhhhh
            if (distance >  threatRadius)
            {
                continue; // skip rest and move on 
            }

            Vector3 directionToSelf = (transform.position - ball.transform.position).normalized;

            // is ball coming towards us?? 1 = directly ggulpp, 0 = yes but sidewways i think, -1 = no!!!!!!!! i thinl
            float alignment = Vector3.Dot(enemyRb.linearVelocity.normalized, directionToSelf);

            if (alignment > 0.8f)
            {
                float speed = enemyRb.linearVelocity.magnitude; 

                if (speed <= 0) {
                    continue; // skip rest and move on 
                }

                float timeToImpact = distance / speed; 

                if (timeToImpact < collisionPrediction) { 
                    useHop(ball.transform.position);
                    return; 
                } 
            }


        }
    }

    void ChooseBestDirection()
    {
        float bestUtility = float.MinValue;

        // specified rayCount (24 rn to start) and 15 degree angles ..
        // though i maybe shouldve made the angles a varibale now uuhh oh well 
        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * 15f;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            // draw every ray cuz this ball is stupid and i cant tell what its doing ever bruh 
            Debug.DrawRay(
                transform.position,
                direction * scanDist,
                Color.red,
                3f
            );

            float reward = 0f;
            float risk = 0f;

            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, scanDist);

            foreach (RaycastHit hit in hits){

                // point objects = good 
                if (hit.collider.CompareTag("Point"))
                {
                    Debug.Log("FOUND POINT");

                    reward +=15f / Mathf.Max(hit.distance, 1f);
                }
                
                // balls = risky 
                if (hit.collider.CompareTag("Ball") && hit.collider.gameObject != gameObject)
                {
                    risk +=20f / Mathf.Max(hit.distance, 1f);
                }
                
                // ++ risk for danger 
                if (hit.collider.CompareTag("Danger"))
                {
                    Debug.Log("FOUND DANGER");

                    risk +=20f / Mathf.Max(hit.distance, 1f);
                }
            }

            Debug.Log($"Angle {angle}: Reward={reward:F2} Risk={risk:F2}");

            float utility =reward - (risk * (1f - riskTolerance));

            if (utility > bestUtility)
            {
                bestUtility = utility;
                bestDirection = direction;
            }
        }

        Debug.DrawRay(
            transform.position,
            bestDirection * scanDist,
            Color.green,
            3f
        );
    }

    void Launch()
    {
        //Debug.Log($"Launching with direction {bestDirection}");

        rb.linearVelocity = bestDirection * launchPower;

        //Debug.Log($"New Velocity {rb.linearVelocity}"); 
    }

    void useHop(Vector3 threatPosition)
    {
        if (points < abilityCost)
        {
            Debug.Log("Hop ball did not have enough points to use hop");
            return; 
        }

        points -= abilityCost;

        // make sure direction is away from the treat 
        Vector3 awayDirect = (transform.position - threatPosition).normalized;

        // hop 
        Vector3 hopDirect = awayDirect + Vector3.up;
        rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);

        Debug.Log("Hop ball used hop!!");
    }


}
