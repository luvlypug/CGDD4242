using UnityEngine;

public class ScaredBall : Ball, IHitboxReceiver
{
    [SerializeField] private float blastRadius;
    [SerializeField] private float blastPower;

    void Start()
    {
        //StartCoroutine(LaunchCooldown(1));
        canAbility = true;
    }

    void Update()
    {
        if (points >= maxPoints)
        {
            points = maxPoints;
            canAbility = true;
        }
    }


    public void OnHitboxEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Ball")
        {
            Debug.Log("hi");
            // Can use ability check
            if (canAbility)
            {
                Ability();
                //StartCoroutine(LaunchCooldown(1));
            }
            // Can launch check
            else if (canLaunch)
            {
                Vector3 direction = collider.gameObject.transform.position - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90 + (Random.Range(0, 2) * 2 - 1); // Randomly chooses 1 or -1 for left or right

                float radians = angle * Mathf.Deg2Rad;

                float directionX = Mathf.Cos(radians);
                float directionZ = Mathf.Sin(radians);

                Vector3 launchDirection = new Vector3(directionX, 0, directionZ);

                power = maxPower;

                Launch(launchDirection, power);
            }
        }
    }

    public override void Ability()
    {
        if (canAbility && !UIManager.Instance.isPaused)
        {
            Vector3 blastPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(blastPos, blastRadius);

            foreach (Collider hit in colliders)
            {
                if (hit.gameObject == gameObject) continue;

                if (hit.gameObject.tag == "Ball")
                {
                    Rigidbody hitRB = hit.GetComponent<Rigidbody>();
                    Vector3 direction = hit.transform.position - blastPos;
                    direction.y = 0;
                    hitRB.AddForce(direction.normalized * blastPower, ForceMode.Impulse);
                }
            }

            points = 0;
            canAbility = false;
        }
    }
}