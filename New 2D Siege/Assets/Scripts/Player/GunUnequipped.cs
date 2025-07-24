using PurrNet.StateMachine;
using UnityEngine;

public class GunUnequipped : StateNode
{
    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        enabled = isOwner;
    }
}
