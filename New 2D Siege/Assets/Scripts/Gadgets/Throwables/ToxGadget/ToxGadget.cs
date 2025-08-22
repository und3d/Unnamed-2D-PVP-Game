using System;
using UnityEngine;

public class ToxGadget : ThrowableGadget
{
    protected override void Update()
    {
        if (!isOwner)
            return;
        if (!playerObject)
            return;
        if (!gameController)
            return;
        
        if (isOwner && !(Time.unscaledTime < timeAtPlacement + timeBeforePickup) && canBePickedUp)
        {
            HandlePickup();
            if (gadgetController.thrownGadgetIsBeingPickedUp)
            {
                Debug.Log($"thrownGadgetIsBeingPickedUp: {gadgetController.thrownGadgetIsBeingPickedUp}", this);
                return;
            }
        }

        //Debug.Log($"thrownGadgetPickedUp: {thrownGadgetPickedUp}", this);

        if (thrownGadgetPickedUpLocal)
        {
            ActivationDelay();
        }
    }

    private void LateUpdate()
    {
        if (!gameController || !gadgetController)
            return;
        if (gadgetController.isGadgetPulledOut)
            return;
        if (gameController.justThrewGadget)
            return;
        
        if (roundView.IsGadgetPickupTextActive())
        {
            Debug.Log($"isGadgetInteractTextActive: {roundView.IsGadgetPickupTextActive()}", this);
            return;
        }
        
        Debug.Log($"isGadgetInteractTextActive: {roundView.IsGadgetPickupTextActive()}", this);
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            
            ActivateGadget();
        }
    }

    private void ActivationDelay()
    {
        if (!(Time.unscaledTime < gadgetController.timeAtPickup + gadgetController.timeBeforeActivation))
        {
            thrownGadgetPickedUpLocal = false;
        }
    }

    protected override void ActivateGadget()
    {
        
        
        base.ActivateGadget();
    }
}
