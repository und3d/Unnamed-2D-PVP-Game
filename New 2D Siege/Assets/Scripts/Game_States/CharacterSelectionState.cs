using PurrNet.StateMachine;
using UnityEngine;

public class CharacterSelectionState : StateNode
{
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        
        if (!asServer)
            return;


        machine.Next();
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}
