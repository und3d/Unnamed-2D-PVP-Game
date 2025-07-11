using System.Collections;
using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class RoundEndState : StateNode<bool>
{
    [SerializeField] private int amountOfRounds = 3;
    [SerializeField] private StateNode spawningState;

    private int _roundCount = 0;
    private WaitForSeconds _delay = new(3f);

    
    //basic round end enter
    public override void Enter(bool attackWon, bool asServer)
    {
        base.Enter(asServer);
        
        if (!asServer)
            return;
        
        CheckForGameEnd(attackWon);
    }

    private void CheckForGameEnd(bool attackWon)
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }
        
        if (attackWon)
        {
            var attackTeam = gameController.teamSides[GameController.Side.Attack];
            gameController.AddRoundWin(attackTeam);
        }
        else
        {
            var defenseTeam = gameController.teamSides[GameController.Side.Defense];
            gameController.AddRoundWin(defenseTeam);
        }

        int roundsToWin = (amountOfRounds / 2) + 1;
        _roundCount++;

        foreach (var team in gameController.roundScores)
        {
            if (team.Value >= roundsToWin)
            {
                var winningTeam = team.Key;
                machine.Next();
                return;
            }
            else if (_roundCount > amountOfRounds)
            {
                Debug.Log("Round count has reached max without winner. Game ends as Tie, please do not use even round count.");
                machine.Next();
                return;
            }
        }
        
        StartCoroutine(DelayNextState());
        
        /* Non-Team System
        
        if (_roundCount >= amountOfRounds)
        {
            machine.Next();
            return;        
        }
        StartCoroutine(DelayNextState());
        */
        
    }

    private IEnumerator DelayNextState()
    {
        yield return _delay;
        machine.SetState(spawningState);
    }
}
