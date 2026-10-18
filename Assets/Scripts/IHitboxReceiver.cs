using UnityEngine;

public interface IHitboxReceiver
{
    void OnHitboxEnter(Collider collider);
}