using UnityEngine;

public abstract class DroneGadget : GadgetBase
{
    [SerializeField] protected Rigidbody2D rb;
    public bool canBePickedUp = false;
    
    protected bool isFrozen = false;

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
        if (isFrozen)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;
        
        GetComponent<Collider2D>().isTrigger = true;
        
        isFrozen = true;
        canBePickedUp = true;
    }
}
