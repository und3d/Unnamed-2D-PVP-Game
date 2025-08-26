using System;
using UnityEngine;

public abstract class DroneGadget : GadgetBase
{
    [SerializeField] private GameObject droneVision;
    [SerializeField] private GameObject droneVisionNoShadows;
    
    private GameObject visionObj;
    private GameObject visionObjNoShadows;
    private bool canMove;

    private void Awake()
    {
        visionObj = Instantiate(droneVision, transform, false);
        //visionObj.gameObject.transform.Rotate(new Vector3(0, 0, 270));
        visionObjNoShadows = Instantiate(droneVisionNoShadows, transform, false);
        //visionObjNoShadows.gameObject.transform.Rotate(new Vector3(0, 0, 270));
        ToggleActive(false, isOwner);
    }

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

    public virtual void ToggleActive(bool toggle, bool droneOwner)
    {
        if (droneOwner)
            canMove = toggle;
        visionObj.SetActive(toggle);
        visionObjNoShadows.SetActive(toggle);
    }
}
