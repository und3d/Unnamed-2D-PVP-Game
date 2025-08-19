using UnityEngine;

public abstract class DroneGadget : GadgetBase
{
    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        InvokeRepeating(nameof(CheckIfStopped), checkDelay, checkDelay);
    }

    protected override void Update()
    {
        if (!playerObject)
            return;
        if (!canBePickedUp)
            return;
        base.Update();
    }

    public virtual void Throw(Vector2 direction, float force)
    {
        gadgetRigidbody.simulated = true;
        gadgetRigidbody.linearVelocity = direction * force;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFrozen)
            return;

        FreezeGadget(false);
    }
}
