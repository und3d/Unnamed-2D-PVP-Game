using System.Collections;
using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class RoundEndState : StateNode<bool>
{
    [SerializeField] private int amountOfRounds = 5;
    [SerializeField] private StateNode characterSelectionState;

    private int _roundCount = 0;
    private int roundCountToSwap;
    private WaitForSeconds _delay = new(3f);
    private GameController gameController;

    
    //basic round end enter
    public override void Enter(bool attackWon, bool asServer)
    {
        base.Enter(asServer);
        
        if (!asServer)
            return;
        
        if (!InstanceHandler.TryGetInstance(out gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
            return;
        }
        
        CheckForGameEnd(attackWon);
    }

    private void CheckForGameEnd(bool attackWon)
    {
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
        
        var roundsToWin = (amountOfRounds / 2) + 1;
        _roundCount++;
        roundCountToSwap++;
        
        if (roundCountToSwap == amountOfRounds / 2)
            swapSides();
        
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
    }

    private void swapSides()
    {
        roundCountToSwap = 0;
        
        // Swapped via deconstruction
        (gameController.teamSides[GameController.Side.Attack], gameController.teamSides[GameController.Side.Defense]) 
            = (gameController.teamSides[GameController.Side.Defense], gameController.teamSides[GameController.Side.Attack]);
    }

    private IEnumerator DelayNextState()
    {
        yield return _delay;
        machine.SetState(characterSelectionState);
    }
}
