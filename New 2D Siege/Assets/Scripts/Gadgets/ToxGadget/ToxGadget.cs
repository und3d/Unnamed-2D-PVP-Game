using System;
using Unity.VisualScripting;
using UnityEngine;

public class ToxGadget : ThrowableGadget
{
    private bool isFrozen = false;

    protected override void OnCollisionEnter2D(Collision2D collision)
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
