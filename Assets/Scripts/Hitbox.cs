using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private IHitboxReceiver receiver;

    void Start()
    {
        receiver = GetComponentInParent<IHitboxReceiver>();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (receiver != null)
        {
            receiver.OnHitboxEnter(collider);
        }
    }
}