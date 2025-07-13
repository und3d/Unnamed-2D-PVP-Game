using UnityEngine;

public class TestInteraction : HoldToInteract
{
    protected override bool CanInteract()
    {
        return true;
    }

    protected override void OnComplete()
    {
        Debug.Log("TestInteraction Complete!");
    }
}
