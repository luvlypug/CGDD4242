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

        Ball opponent = collision.gameObject.GetComponentInParent<Ball>();

        if (opponent == null || opponent == owner || opponent.rb == null)
        {
            return;
        }

        SpeedTracker tracker = opponent.GetComponent<SpeedTracker>();

        if (tracker == null)
        {
            tracker = opponent.gameObject.AddComponent<SpeedTracker>();
        }

        StartCoroutine(MeasureSlowdown(opponent, tracker.PreviousSpeed));
    }

    IEnumerator MeasureSlowdown(Ball opponent, float speedBefore)
    {
        yield return new WaitForSeconds(measureDelay);

        if (opponent == null || owner == null)
        {
            yield break;
        }

        float after = opponent.rb.linearVelocity.magnitude;
        owner.ReportHit(Mathf.Max(0f, speedBefore - after));
    }
}