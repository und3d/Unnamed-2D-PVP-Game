using PurrNet;
using UnityEngine;

public abstract class ThrowableGadget : GadgetBase
{
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected bool canBePickedUp = false;

    protected override void Update()
    {
        if (!canBePickedUp)
            return;
        base.Update();
    }

    public virtual void Throw(Vector2 direction, float force)
    {
        rb.simulated = true;
        rb.linearVelocity = direction * force;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle bounce, explode, stick, etc.
    }
}
