using UnityEngine;
using System.Collections;
//Midia zone
public class Twinball : Ball
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 15f;

    [Header("Movement")]
    [SerializeField] private float launchPower = 10f;
    [SerializeField] private float thinkInterval = 1f;

    [Header("Ability: Split")]
    [SerializeField] private GameObject twinballPrefab;
    [SerializeField] private float cloneLifetime = 8f;
    [SerializeField] private float cloneforce = 1f;
    [SerializeField] private float abilityRange = 5f;
    [SerializeField] private float coolDown = 10f;

    // Shared between parent and clone

    private int familyID;

    // Clone-specific flag
    private bool isClone = false;



    //we mentioned that the player might have acess to the abilities so imma just do this quick and dirty way to make sure twinballs can fight other twinball
    private void Start()
    {
        if (familyID == 0)
        {
            familyID = Random.Range(100000, 1000000);
        }
        InvokeRepeating(nameof(UpdateMovement), 0f, thinkInterval);

        //temp just testing
        canAbility = true;

    }


    private void UpdateMovement()
    {
        
        Vector3 moveDirection = CalculateSteering();

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            currentDirection = moveDirection.normalized;
            Launch(currentDirection, launchPower);
        }
    }
    //I, fool that i am, made the prev verson of calc steering normalize diredtion, so if an enemy was near an obstacle, it wouldnt actually attack
    //now it beefs better
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

            // Priority 1: Attack Balls
            if (hit.CompareTag("Ball"))
            {
                Twinball otherTwin = hit.GetComponent<Twinball>();

                // Ignore parent/clone
                if (otherTwin != null && otherTwin.familyID == familyID)
                    continue;

                if (distance < closestEnemyDistance)
                {
                    closestEnemyDistance = distance;
                    closestEnemy = hit.transform;
                }

                continue;
            }

            // Priority 2: Avoid Danger
            if (hit.CompareTag("Danger"))
            {
                avoidance +=
                    (transform.position - hit.transform.position).normalized /
                    Mathf.Max(distance, 0.1f);

                continue;
            }

            // Priority 3: Collect Points
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

        // Highest priority: enemy balls
        if (closestEnemy != null)
        {
            desiredDirection =
                (closestEnemy.position - transform.position).normalized;

            // Use ability when close enough
            if (closestEnemyDistance <= abilityRange)
            {
                if(!isClone)
                {
                    Ability();
                }
            }
        }
        else if (closestPoint != null)
        {
            desiredDirection =
                (closestPoint.position - transform.position).normalized;
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
        Debug.Log(canAbility);
        if (isClone || twinballPrefab == null ||!canAbility)
            return;

        GameObject cloneObj =
            Instantiate(twinballPrefab, transform.position, transform.rotation);

        Twinball clone = cloneObj.GetComponent<Twinball>();

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

    private void OnDrawGizmosSelected()
    {
        //sight area

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        //ability activation range

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, abilityRange);

    }

}
