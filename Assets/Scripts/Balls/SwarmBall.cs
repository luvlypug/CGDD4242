using System.Collections.Generic;
using UnityEngine;

// Austin's agent
public class SwarmBall : Ball
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

    public GameObject miniBallPrefab;
    public int miniBallCount = 12;
    public float miniBallBurstSpeed = 8f;

    public int TotalMiniBallHits;
    public float TotalSlowdown;
    private float abilityReadySince = -1f;

    void Update()
    {
        currentDirection = GetCurrentDirection();

        if (points >= maxPoints)
        {
            points = maxPoints;
            canAbility = true;
        }

        if (isEliminated)
        {
            return;
        }

        if (canAbility && abilityReadySince < 0f)
        {
            abilityReadySince = Time.time;
        }

        opportunityTimer += Time.deltaTime;

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
        if (!canAbility)
        {
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, opportunityRadius);
        List<Ball> targets = new List<Ball>();

        foreach (Collider hit in hits)
        {
            Ball other = hit.GetComponentInParent<Ball>();

            if (other == null || other == this || other.isEliminated || other.rb == null || targets.Contains(other))
            {
                continue;
            }

            targets.Add(other);
        }

        if (targets.Count == 0)
        {
            if (debugOpportunity)
            {
                Debug.Log("Swarm: ability ready but no opposing balls in radius");
            }

            return;
        }

        float bestUtility = 0f;
        float bestDistance = float.MaxValue;

        foreach (Ball target in targets)
        {
            float distance = FlatDistance(transform.position, target.transform.position);
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
            if (canAbility)
            {
                Ability();
            }
        }
    }

    float TargetUtility(Ball target, float distance, List<Ball> allTargets)
    {
        float speedScore = Mathf.Clamp01(Flat(target.rb.linearVelocity).magnitude / maxExpectedBallSpeed);
        float proximityScore = 1f - Mathf.Clamp01(distance / opportunityRadius);

        int neighbors = 0;

        foreach (Ball other in allTargets)
        {
            if (other == target)
            {
                continue;
            }

            if (FlatDistance(other.transform.position, target.transform.position) <= clusterRadius)
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
        bool hunting = canAbility;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            float reward = 0f;
            float risk = 0f;

            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, scanDist);

            foreach (RaycastHit hit in hits)
            {
                Collider col = hit.collider;

                if (col.GetComponentInParent<SwarmMiniBall>() != null)
                {
                    continue;
                }

                Ball hitBall = col.GetComponentInParent<Ball>();

                if (hitBall == this || (hitBall != null && hitBall.isEliminated))
                {
                    continue;
                }

                float dist = Mathf.Max(hit.distance, 1f);

                if (col.CompareTag("Point"))
                {
                    if (!hunting)
                    {
                        reward += pointReward / dist;
                    }
                }
                else if (hitBall != null)
                {
                    if (hunting)
                    {
                        float speedFactor = 1f;

                        if (hitBall.rb != null)
                        {
                            speedFactor += Mathf.Clamp01(Flat(hitBall.rb.linearVelocity).magnitude / maxExpectedBallSpeed);
                        }

                        reward += ballReward * speedFactor / dist;
                    }
                    else
                    {
                        risk += ballRiskWhenBuilding / dist;
                    }
                }
                else if (col.CompareTag("Deadzone") || col.gameObject.name == "End")
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
        launchDirection = bestDirection;
        power = Mathf.Clamp(launchPower, minPower, maxPower);
        Launch(launchDirection, power);
    }

    public override void Ability()
    {
        if (canAbility && !UIManager.Instance.isPaused)
        {
            if (miniBallPrefab == null)
            {
                return;
            }

            for (int i = 0; i < miniBallCount; i++)
            {
                float angle = (360f / miniBallCount) * i * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

                GameObject mini = Instantiate(miniBallPrefab, transform.position + direction * 0.6f, Quaternion.identity);

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

            points = 0;
            canAbility = false;
            abilityReadySince = -1f;
        }
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