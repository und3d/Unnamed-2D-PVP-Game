using System;
using PurrNet;
using UnityEngine;

public abstract class ThrowableGadget : GadgetBase
{
    protected override void Awake()
    {
        base.Awake();
        
        InvokeRepeating(nameof(CheckIfStopped), checkDelay, checkDelay);
    }

    protected override void Update()
    {
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

        FreezeGadget(true);
    }
    
    protected virtual void ActivateGadget()
    {
        Destroy(gameObject);
    }
}
