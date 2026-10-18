using UnityEngine;
using System.Collections;
//Midia zone
public class Twinball : Ball
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 15f;

    [Header("Movement")]
    [SerializeField] private float launchPower = 100f;
    [SerializeField] private float thinkInterval = 0.25f;

    [Header("Splitting")]
    [SerializeField] private GameObject twinballPrefab;
    [SerializeField] private float cloneLifetime = 8f;

    // Shared between parent and clone
  
    private int familyID;

    // Clone-specific flag
    private bool isClone = false;

    private Vector3 currentDirection = Vector3.forward;

    private static int nextFamilyID = 1;

    //we mentioned that the player might have acess to the abilities so imma just do this quick and dirty way to make sure twinballs can fight other twinball
    private void Awake()
    {
        if (familyID == 0)
        {
            familyID = Random.Range(100000, 1000000);
        }
        InvokeRepeating(nameof(UpdateMovement), 0f, thinkInterval);

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

    private Vector3 CalculateSteering()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        Vector3 attraction = Vector3.zero;
        Vector3 avoidance = Vector3.zero;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            Vector3 toTarget = hit.transform.position - transform.position;
            float distance = Mathf.Max(toTarget.magnitude, 0.1f);

            // attacks balls, avoids dangers
            if (hit.CompareTag("Ball"))
            {
                Twinball otherTwin = hit.GetComponent<Twinball>();

                if (otherTwin != null && otherTwin.familyID == familyID)
                    continue; //does not attack parent

                attraction += toTarget.normalized / distance;
            }
            else if (hit.CompareTag("Danger"))
            {
                avoidance -= (toTarget.normalized / distance) * 1f;
            }
        }

        Vector3 finalDirection = attraction + avoidance;

        if (finalDirection.sqrMagnitude < 0.01f)
        {
            return currentDirection;
        }

        return finalDirection.normalized;
    }

    public void Split()
    {
        if (isClone || twinballPrefab == null)
            return;

        GameObject cloneObj =
            Instantiate(twinballPrefab, transform.position, transform.rotation);

        Twinball clone = cloneObj.GetComponent<Twinball>();

        if (clone != null)
        {
            clone.InitializeClone(familyID);

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
            clone.Launch(perpendicular, launchPower);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
