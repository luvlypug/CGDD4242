using UnityEngine;

public class ScaredBall : Ball, IHitboxReceiver
{
    void Start()
    {
        //StartCoroutine(LaunchCooldown(1));
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

            points = 0;
            canAbility = false;
        }
    }

}
