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
        
        if (Input.GetKeyDown(KeyCode.F) && gameController.canActivateThrownGadget && !gadgetBeingPickedUp)
        {
            ActivateGadget();
        }
        
        base.Update();
    }

    protected override void ActivateGadget()
    {
        
        
        base.ActivateGadget();
    }
}
