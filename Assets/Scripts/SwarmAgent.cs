using System.Collections.Generic;
using UnityEngine;

public class SwarmAgent : Ball
{
    public int rayCount = 24;
    public float scanDist = 20f;

    public float launchPower = 5f;
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
    public float useSwarmThreshold = 0.35f;
    public float thresholdDecayPerSecond = 0.1f;
    public float minThreshold = 0.1f;
    public float swarmRange = 6f;
    public bool debugOpportunity = false;

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

    void Update()
    {
        opportunityTimer += Time.deltaTime;

        if (AbilityReady && abilityReadySince < 0f)
        {
            abilityReadySince = Time.time;
        }

        if (opportunityTimer >= opportunityCheckCooldown)
        {
            opportunityTimer = 0f;
            CheckOpportunity();
        }

        if (canLaunch)
        {
            ChooseBestDirection();
            LaunchAlongBest();
        }
    }

    void CheckOpportunity()
    {
        if (!AbilityReady)
        {
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, opportunityRadius);
        List<Rigidbody> targets = new List<Rigidbody>();

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Ball"))
            {
                continue;
            }

            Rigidbody targetRb = hit.attachedRigidbody;

            if (targetRb == null || targetRb == rb || targets.Contains(targetRb))
            {
                continue;
            }

            targets.Add(targetRb);
        }

        if (targets.Count == 0)
        {
            if (debugOpportunity)
            {
                Debug.Log("Swarm: ability ready but no tagged Ball targets in radius");
            }

            return;
        }

        float bestUtility = 0f;
        float bestDistance = float.MaxValue;

        foreach (Rigidbody target in targets)
        {
            float distance = FlatDistance(transform.position, target.position);
            float utility = TargetUtility(target, distance, targets);

            if (utility > bestUtility)
            {
                bestUtility = utility;
                bestDistance = distance;
            }
        }

        float heldFor = Time.time - abilityReadySince;
        float threshold = Mathf.Max(minThreshold, useSwarmThreshold - heldFor * thresholdDecayPerSecond);

        if (debugOpportunity)
        {
            Debug.Log("Swarm: utility " + bestUtility + " threshold " + threshold + " distance " + bestDistance);
        }

        if (bestUtility >= threshold && bestDistance <= swarmRange)
        {
            UseSwarm();
        }
    }

    float TargetUtility(Rigidbody target, float distance, List<Rigidbody> allTargets)
    {
        float speedScore = Mathf.Clamp01(Flat(target.linearVelocity).magnitude / maxExpectedBallSpeed);
        float proximityScore = 1f - Mathf.Clamp01(distance / opportunityRadius);

        int neighbors = 0;

        foreach (Rigidbody other in allTargets)
        {
            if (other == target)
            {
                continue;
            }

            if (FlatDistance(other.position, target.position) <= clusterRadius)
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

    void LaunchAlongBest()
    {
        float power = Mathf.Clamp(launchPower, minPower, maxPower);
        Launch(bestDirection, power);
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