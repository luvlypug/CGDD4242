using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwarmAgent : MonoBehaviour
{
    private Rigidbody rb;

    public int rayCount = 24;
    public float scanDist = 20f;

    public float launchPower = 5f;
    public float cooldownPerPower = 0.6f;
    private float launchTimer;
    private float currentCooldown;
    private Vector3 bestDirection = Vector3.forward;

    public float riskTolerance = 0.5f;
    public float pointReward = 10f;
    public float ballReward = 10f;
    public float ballRiskWhenBuilding = 2f;
    public float dangerRisk = 5f;
    public float maxExpectedBallSpeed = 20f;

    public float opportunityRadius = 12f;
    public float opportunityCheckCooldown = 0.1f;
    private float opportunityTimer;

    public float speedWeight = 0.4f;
    public float proximityWeight = 0.3f;
    public float clusterWeight = 0.3f;
    public float clusterRadius = 4f;
    public float useSwarmThreshold = 0.55f;
    public float thresholdDecayPerSecond = 0.03f;
    public float minThreshold = 0.2f;
    public float swarmRange = 5f;

    public int abilityCost = 50;
    public GameObject miniBallPrefab;
    public int miniBallCount = 12;
    public float miniBallBurstSpeed = 8f;

    public int Points;
    public int TotalMiniBallHits;
    public float TotalSlowdown;
    private float abilityReadySince = -1f;

    public bool AbilityReady
    {
        get { return Points >= abilityCost; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentCooldown = launchPower * cooldownPerPower;
    }

    void Update()
    {
        opportunityTimer += Time.deltaTime;
        launchTimer += Time.deltaTime;

        if (AbilityReady && abilityReadySince < 0f)
        {
            abilityReadySince = Time.time;
        }

        if (opportunityTimer >= opportunityCheckCooldown)
        {
            opportunityTimer = 0f;
            CheckOpportunity();
        }

        if (launchTimer > currentCooldown)
        {
            launchTimer = 0f;
            ChooseBestDirection();
            Launch();
        }
    }

    void CheckOpportunity()
    {
        if (!AbilityReady)
        {
            return;
        }

        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");

        float bestUtility = 0f;
        float bestDistance = float.MaxValue;
        bool found = false;

        foreach (GameObject ball in balls)
        {
            if (ball == gameObject)
            {
                continue;
            }

            Rigidbody enemyRb = ball.GetComponent<Rigidbody>();

            if (enemyRb == null)
            {
                continue;
            }

            float distance = FlatDistance(transform.position, ball.transform.position);

            if (distance > opportunityRadius)
            {
                continue;
            }

            float utility = TargetUtility(enemyRb, distance, balls);

            if (utility > bestUtility)
            {
                bestUtility = utility;
                bestDistance = distance;
                found = true;
            }
        }

        if (!found)
        {
            return;
        }

        float heldFor = Time.time - abilityReadySince;
        float threshold = Mathf.Max(minThreshold, useSwarmThreshold - heldFor * thresholdDecayPerSecond);

        if (bestUtility >= threshold && bestDistance <= swarmRange)
        {
            UseSwarm();
        }
    }

    float TargetUtility(Rigidbody target, float distance, GameObject[] allBalls)
    {
        float speedScore = Mathf.Clamp01(Flat(target.linearVelocity).magnitude / maxExpectedBallSpeed);
        float proximityScore = 1f - Mathf.Clamp01(distance / opportunityRadius);

        int neighbors = 0;

        foreach (GameObject other in allBalls)
        {
            if (other == gameObject || other == target.gameObject)
            {
                continue;
            }

            if (FlatDistance(other.transform.position, target.position) <= clusterRadius)
            {
                neighbors++;
            }
        }

        float clusterScore = Mathf.Clamp01(neighbors / 3f);

        return speedWeight * speedScore + proximityWeight * proximityScore + clusterWeight * clusterScore;
    }

    void ChooseBestDirection()
    {
        float bestUtility = float.MinValue;
        float angleStep = 360f / rayCount;
        bool hunting = AbilityReady;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            float reward = 0f;
            float risk = 0f;

            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, scanDist);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    continue;
                }

                if (hit.collider.CompareTag("MiniBall"))
                {
                    continue;
                }

                float dist = Mathf.Max(hit.distance, 1f);

                if (hit.collider.CompareTag("Point"))
                {
                    if (!hunting)
                    {
                        reward += pointReward / dist;
                    }
                }

                if (hit.collider.CompareTag("Ball"))
                {
                    if (hunting)
                    {
                        float speedFactor = 1f;
                        Rigidbody hitRb = hit.collider.attachedRigidbody;

                        if (hitRb != null)
                        {
                            speedFactor += Mathf.Clamp01(Flat(hitRb.linearVelocity).magnitude / maxExpectedBallSpeed);
                        }

                        reward += ballReward * speedFactor / dist;
                    }
                    else
                    {
                        risk += ballRiskWhenBuilding / dist;
                    }
                }

                if (hit.collider.CompareTag("Danger"))
                {
                    risk += dangerRisk / dist;
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
        rb.linearVelocity = bestDirection * launchPower;
        currentCooldown = launchPower * cooldownPerPower;
    }

    void UseSwarm()
    {
        Points -= abilityCost;
        abilityReadySince = -1f;

        if (miniBallPrefab == null)
        {
            return;
        }

        for (int i = 0; i < miniBallCount; i++)
        {
            float angle = (360f / miniBallCount) * i * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            GameObject mini = Instantiate(miniBallPrefab, transform.position + direction * 0.6f, Quaternion.identity);
            mini.tag = "MiniBall";

            SwarmMiniBall miniScript = mini.GetComponent<SwarmMiniBall>();

            if (miniScript != null)
            {
                miniScript.Init(this);
            }

            Rigidbody miniRb = mini.GetComponent<Rigidbody>();

            if (miniRb != null)
            {
                miniRb.linearVelocity = direction * miniBallBurstSpeed;
            }
        }

        Debug.Log("Swarm ball used swarm!");
    }

    public void AddPoints(int amount)
    {
        Points += amount;
    }

    public void ReportHit(float slowdownAmount)
    {
        TotalMiniBallHits++;
        TotalSlowdown += slowdownAmount;
    }

    static Vector3 Flat(Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        return Flat(a - b).magnitude;
    }
}


