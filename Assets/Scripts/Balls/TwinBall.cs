using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Midia zone
public class TwinBall : Ball
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 15f;

    [Header("Movement")]
    [SerializeField] private float launchPower = 10f;
    [SerializeField] private float thinkInterval = 1f;

    [Header("Waypoints")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [SerializeField] private float waypointReachDistance = 2f; //how far the dude is willing to go for the waypoints. likely wont come up but eh. canmt hurt

    [Header("Ability: Split")]
    [SerializeField] private GameObject twinballPrefab;
    [SerializeField] private float cloneLifetime = 8f;
    [SerializeField] private float cloneforce = 1f;
    [SerializeField] private float abilityRange = 5f;
    [SerializeField] private float coolDown = 10f;

    //Shared between parent and clone
    private int familyID;

    //clone-specific flag
    private bool isClone = false;

    private int currentWaypointIndex = 0;

    //we mentioned that the player might have acess to the abilities so imma just do this quick and dirty way to make sure twinballs can fight other twinball
    private void Start()
    {
        if (familyID == 0)
        {
            familyID = Random.Range(100000, 1000000);
        }

        InvokeRepeating(nameof(UpdateMovement), 0f, thinkInterval);
    }

    private void Update()
    {
        currentDirection = GetCurrentDirection();

        if (points >= maxPoints)
        {
            points = maxPoints;
            canAbility = true;
        }
    }

    private void UpdateMovement()
    {
        if (this.gameObject.activeSelf)
        {
            Vector3 moveDirection = CalculateSteering();

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                currentDirection = moveDirection.normalized;
                Launch(currentDirection, launchPower);
            }
        }
    }
    //NOTE: allwaypoint stuff should not cause errors if there is no waypoints
    //moving to own method for tidyness
    //TODO: Random waypoint selection?
    //no biggy if not but that would make the AI less
    //artifitial 
    private Vector3 GetWaypointDirection()
    {
        if (waypoints == null || waypoints.Count == 0)
            return currentDirection;

        Transform currentWaypoint = waypoints[currentWaypointIndex];

        if (Vector3.Distance(transform.position, currentWaypoint.position)
            <= waypointReachDistance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Count)
            {
                currentWaypointIndex = 0;
            }

            currentWaypoint = waypoints[currentWaypointIndex];
        }

        return (currentWaypoint.position - transform.position).normalized;
    }

    //I, fool that i am, made the prev verson of calc steering normalize diredtion,
    //so if an enemy was near an obstacle, it wouldnt actually attack
    //now it beefs better
    //REWRITE NUMBRE THREE WOOOOO
    //ok nvm not so bad
    private Vector3 CalculateSteering()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        Transform closestEnemy = null;
        Transform closestPoint = null;

        float closestEnemyDistance = float.MaxValue;
        float closestPointDistance = float.MaxValue;

        Vector3 avoidance = Vector3.zero;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            Vector3 toTarget = hit.transform.position - transform.position;
            float distance = toTarget.magnitude;

            //Priority 1 nutcheck
            if (hit.CompareTag("Ball"))
            {
                TwinBall otherTwin = hit.GetComponent<TwinBall>();

                //ignore parent/clone
                if (otherTwin != null && otherTwin.familyID == familyID)
                    continue;

                if (distance < closestEnemyDistance)
                {
                    closestEnemyDistance = distance;
                    closestEnemy = hit.transform;
                }

                continue;
            }

            //Priority 2: Avoid Danger
            if (hit.CompareTag("Danger"))
            {
                avoidance +=
                    (transform.position - hit.transform.position).normalized /
                    Mathf.Max(distance, 0.1f);

                continue;
            }

            //Priority 3: Collect Points
            if (hit.CompareTag("Point"))
            {
                if (distance < closestPointDistance)
                {
                    closestPointDistance = distance;
                    closestPoint = hit.transform;
                }
            }
        }

        Vector3 desiredDirection = currentDirection;

        //Highest priority: enemy balls
        if (closestEnemy != null)
        {
            desiredDirection =
                (closestEnemy.position - transform.position).normalized;

            //Use ability when close enough
            if (closestEnemyDistance <= abilityRange)
            {
                if (!isClone)
                {
                    // Can use ability check
                    if (canAbility)
                    {
                        Ability();
                    }
                }
            }
        }
        // Second priority: points
        else if (closestPoint != null)
        {
            desiredDirection =
                (closestPoint.position - transform.position).normalized;
        }
        // Lowest priority: waypoints
        else
        {
            desiredDirection = GetWaypointDirection();
        }

        //avoid obstacles but should still attack
        if (avoidance.sqrMagnitude > 0.01f)
        {
            desiredDirection =
                (desiredDirection + avoidance.normalized * 0.75f).normalized;
        }

        return desiredDirection;
    }

    public override void Ability()
    {
        if (canAbility && !UIManager.Instance.isPaused)
        {
            if (isClone || twinballPrefab == null)
                return;

            GameObject cloneObj =
                Instantiate(twinballPrefab, transform.position, transform.rotation);

            TwinBall clone = cloneObj.GetComponent<TwinBall>();

            if (clone != null)
            {
                clone.InitializeClone(familyID);
                StartCoroutine(abilityCooldown(coolDown));

                clone.isClone = true;
                clone.canAbility = false;

                // Perpendicular direction
                Vector3 perpendicular =
                    Vector3.Cross(currentDirection, Vector3.up);

                // Fallback if moving vertically
                if (perpendicular.sqrMagnitude < 0.01f)
                {
                    perpendicular =
                        Vector3.Cross(currentDirection, Vector3.right);
                }

                perpendicular.Normalize();

                clone.currentDirection = perpendicular;
                clone.Launch(perpendicular, cloneforce);
            }

            points = 0;
            canAbility = false;
        }
    }

    private void InitializeClone(int inheritedFamilyID)
    {
        familyID = inheritedFamilyID;
        isClone = true;

        StartCoroutine(DestroyAfterTime());
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(cloneLifetime);

        Destroy(gameObject);
    }

    public IEnumerator abilityCooldown(float seconds)
    {
        canAbility = false;

        yield return new WaitForSeconds(seconds);

        canAbility = true;
    }

    //OK to be a lil transparent. i mentioned this to another programmer friend & they gave some pointers on the waypoint stuff, but the biggest thing is this cool gizmo code ngl
    //and yes im lkeaving this down hear and not at the top deliobrately i dont wanna give the wrong impression that i yoinked the code from them
    //which to be fair i did try and yoinbk the code from MYSELF since i had another project use waypoints & i hoped i could just swap out the navmesh stuff with the shmovement of the ball
    //i coulkd not but the structure ended up the same. kinda
    private void OnDrawGizmosSelected()
    {
        //sight area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        //ability activation area
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, abilityRange);

        //  cool af waypoint visuals
        if (waypoints != null && waypoints.Count > 0)
        {
            Gizmos.color = Color.yellow;

            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null)
                    continue;

                Gizmos.DrawSphere(waypoints[i].position, 0.3f);

                Transform next =
                    waypoints[(i + 1) % waypoints.Count];

                if (next != null)
                {
                    Gizmos.DrawLine(
                        waypoints[i].position,
                        next.position);
                }
            }
        }
    }
}
