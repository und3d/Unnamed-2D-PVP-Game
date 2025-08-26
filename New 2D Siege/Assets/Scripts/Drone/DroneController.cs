using UnityEngine;

public class DroneController : DroneGadget
{
    private playerController playerController;
    
    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        if (!isOwner)
            return;
        
        playerController = playerObject.GetComponent<playerController>();
    }

    protected override void PickupGadget()
    {
        DronePickup();
        roundView.HideGadgetPickupText(this);
        thrownGadgetPickedUpLocal = true;
        gadgetController.timeAtPickup = Time.unscaledTime;
        gadgetController.thrownGadgetIsBeingPickedUp = false;
        gadgetController.gadgetBeingPickedUp = false;
        Destroy(gameObject);
    }

    private void DronePickup()
    {
        
    }
}
