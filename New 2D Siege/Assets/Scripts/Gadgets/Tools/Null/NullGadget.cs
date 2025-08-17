using UnityEngine;

public class NullGadget : ToolGadget
{
    protected override void GadgetFunctionality(RaycastHit2D hit)
    {
        Debug.Log("Null's gadget functionality not implemented.");
    }
}
