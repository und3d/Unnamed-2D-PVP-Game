using UnityEngine;

public class AegisGadget : ToggleGadget
{
    protected override void GadgetFunctionalityToggle(bool toggle)
    {
        _playerHealth.CanBeShotToggle(!toggle);
    }
}
