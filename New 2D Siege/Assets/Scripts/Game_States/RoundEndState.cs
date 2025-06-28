using System.Collections;
using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class RoundEndState : StateNode
{
    [SerializeField] private int amountOfRounds = 3;
    [SerializeField] private StateNode spawningState;

    private int _roundCount = 0;
    private WaitForSeconds _delay = new(3f);

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        
        if (!asServer)
            return;
        
        CheckForGameEnd();
    }

    private void CheckForGameEnd()
    {
        _roundCount++;
        if (_roundCount >= amountOfRounds)
        {
            machine.Next();
            return;        
        }
        
        StartCoroutine(DelayNextState());
    }

    private IEnumerator DelayNextState()
    {
        yield return _delay;
        machine.SetState(spawningState);
    }
}
