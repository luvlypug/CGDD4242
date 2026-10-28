using UnityEngine;

// rin code hi 
// redid and organized some stuff to make less messy and inherit from ball :D

public class HopBall : Ball 
{ 
    // movement stuff 
    [SerializeField] private int rayCount = 24; 
    [SerializeField] private float scanDistance = 20f; 
    [SerializeField] private float launchInterval = 3f; 

    // detect the threats
    [SerializeField] private float threatRadius = 10f; 
    [SerializeField] private float collisionPrediction = 1f; 

    // abilitiy
    // (might have to update this later,
    // but for testing puroses I kept my previous ability code p much the same)
    [SerializeField] private float hopForce = 2f; 
    
    private float launchTimer; 
    private Vector3 bestDirection; 
    private Vector3 threatPosition; 
    
    void Update() 
    {
        currentDirection = GetCurrentDirection();

        if (points >= maxPoints)
        {
            points = maxPoints;
            canAbility = true;
        }

        // should use launch from ball script but still uses the original checkforthreats() method
        currentDirection = GetCurrentDirection(); 
        launchTimer += Time.deltaTime; 
        CheckForThreats(); 
        
        if (launchTimer >= launchInterval && canLaunch) 
        { 
            launchTimer = 0f; ChooseBestDirection(); 
            power = maxPower; 
            Launch(bestDirection, power); 
        }
    } 
    
    void CheckForThreats() 
    {
        // updated method for organization 
        // find things tagged ball so we can avoid them 
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball"); 

        foreach (GameObject ball in balls) 
        { 
            // ignore self #yurp
            if (ball == gameObject) 
                continue; 

            // grab the enemies rb then start calculating threat 
            Rigidbody enemyRB = ball.GetComponent<Rigidbody>(); 

            if (enemyRB == null) 
                continue; 

            float distance = Vector3.Distance(transform.position, ball.transform.position); 

            if (distance > threatRadius) 
                continue; 

            Vector3 directionToSelf = (transform.position - ball.transform.position).normalized; 
            float alignment = Vector3.Dot(enemyRB.linearVelocity.normalized, directionToSelf); 
            
            if (alignment < 0.8f) 
                continue; 
            
            float speed = enemyRB.linearVelocity.magnitude; 
            
            if (speed <= 0f) 
                continue; 
            
            float timeToImpact = distance / speed; 
            
            // logic for when to use ability
            // may update logic since this is the only time it uses the ability
            if (timeToImpact <= collisionPrediction) 
            { 
                threatPosition = ball.transform.position; 
                
                if (canAbility) 
                { 
                    Ability(); 
                } 

                return; 
            } 
        } 
    } 
    
    void ChooseBestDirection() 
    { 
        float bestUtility = float.MinValue;

        // find things tagged ball so we can avoid them 
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");

        for (int i = 0; i < rayCount; i++) 
        { 
            float angle = i * (360f/ rayCount); 

            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward; 

            float reward = 0f; 
            float risk = 0f; 

            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, scanDistance); 

            foreach (RaycastHit hit in hits) 
            { 
                // points = good
                if (hit.collider.CompareTag("Point")) 
                { 
                    reward += 15f / Mathf.Max(hit.distance, 1f); 
                } 
                
                // danger = bad 
                if (hit.collider.CompareTag("Danger"))
                { 
                    risk += 20f / Mathf.Max(hit.distance, 1f); 
                }

                // enemies also = bad 
                if (hit.collider.CompareTag("Ball"))
                {
                    risk += 30f / Mathf.Max(hit.distance, 1f);
                }
            } 

            float utility = reward - risk; 

            if (utility > bestUtility) 
            {
                bestUtility = utility; 
                bestDirection = direction; 
            } 
        }
    } 

    public override void Ability() 
    {
        if (canAbility && !UIManager.Instance.isPaused)
        {
            rb.linearVelocity = Vector3.up * hopForce;

            Debug.Log("Hop ball used hop");

            points = 0;
            canAbility = false;
        }
    } 
}