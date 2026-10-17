using System.Collections;
using UnityEngine;

public class SwarmMiniBall : MonoBehaviour
{
    public float lifetime = 6f;
    public float measureDelay = 0.25f;

    private SwarmAgent owner;

    public void Init(SwarmAgent agent)
    {
        owner = agent;
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (owner == null)
        {
            return;
        }

        if (!collision.gameObject.CompareTag("Ball"))
        {
            return;
        }

        if (collision.gameObject == owner.gameObject)
        {
            return;
        }

        Rigidbody opp = collision.rigidbody;

        if (opp == null)
        {
            return;
        }

        SpeedTracker tracker = opp.GetComponent<SpeedTracker>();

        if (tracker == null)
        {
            tracker = opp.gameObject.AddComponent<SpeedTracker>();
        }

        StartCoroutine(MeasureSlowdown(opp, tracker.PreviousSpeed));
    }

    IEnumerator MeasureSlowdown(Rigidbody opp, float speedBefore)
    {
        yield return new WaitForSeconds(measureDelay);

        if (opp == null || owner == null)
        {
            yield break;
        }

        float after = opp.linearVelocity.magnitude;
        owner.ReportHit(Mathf.Max(0f, speedBefore - after));
    }
}
