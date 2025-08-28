using System;
using UnityEngine;

public abstract class DroneGadget : GadgetBase
{
    [SerializeField] private GameObject droneVision;
    [SerializeField] private GameObject droneVisionNoShadows;
    
    private GameObject visionObj;
    private GameObject visionObjNoShadows;
    protected bool canMove;
    protected bool isOnCameras;

    private void Awake()
    {
        visionObj = Instantiate(droneVision, transform, false);
        visionObjNoShadows = Instantiate(droneVisionNoShadows, transform, false);
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
        if (isOnCameras)
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

    protected override void CheckIfStopped()
    {
        if (hasStopped || !isOwner) return;

        Debug.Log("Checking if stopped. ");
        
        if (gadgetRigidbody.linearVelocity.magnitude < stopThreshold)
        {
            FreezeGadget(false);
            CancelInvoke(nameof(CheckIfStopped));
        }
    }

    public virtual void PlayerIsOnCamerasToggle(bool toggle)
    {
        isOnCameras = toggle;
    }
}
