using UnityEngine;
using System.Collections.Generic;

// jonah code hi (built off of rins code thank you credits to rin)

public class SpeedBall : Ball
{
    private Rigidbody rb;
    // private Ball ball;

    // launch directions 
    public int rayCount = 24;

    // launch info
    public float launchPower = 15f;
    private Vector3 bestDirection; 

    // timer for launchinf
    private float launchTimer;
    public float launchCooldown = 3f; 

    // how far sensors can look
    public float scanDist = 25f; 

    // decision variables
    public float riskTolerance = 0.99f; 
    public float collisionPrediction = 1f;
    public float threatTimer;
    public float threatCheckCooldown = 0.1f; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // ball = GetComponent<Ball>();
    }

    // Update is called once per frame
    void Update()
    {
        currentDirection = GetCurrentDirection();

        if (points >= maxPoints)
        {
            points = maxPoints;
            canAbility = true;
        }

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
                    Ability();
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

            float reward = 0f;
            float risk = 0f;

            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, scanDist);

            foreach (RaycastHit hit in hits){

                // point objects worthless to this guy he dont care about money at all 
                if (hit.collider.CompareTag("Point"))
                {
                    reward += 5f / Mathf.Max(hit.distance, 1f);
                }
                
                // balls = #needthat
                if (hit.collider.CompareTag("Ball") && hit.collider.gameObject != gameObject)
                {
                    reward += 25f / Mathf.Max(hit.distance, 1f);
                }
            }
            
            float utility = reward - (risk * (1f - riskTolerance));
            
            if (utility > bestUtility)
            {
                bestUtility = utility;
                bestDirection = direction;
            }
        }
    }

    void Launch()
    {
        Debug.Log(launchPower);

        if (launchPower >= 25f) {
            launchPower = 25f;
        }

        rb.linearVelocity = bestDirection * launchPower;

        Debug.Log("speed ball is launching!");

        if (launchPower >= 15f) {
            launchPower = 15f;
        }

    }

    public override void Ability()
    {
        launchPower += 10f;
        Debug.Log("speed ball is speeding up!");
    }


}
