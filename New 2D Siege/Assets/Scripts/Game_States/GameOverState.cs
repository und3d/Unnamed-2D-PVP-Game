using System.Collections.Generic;
using System.Linq;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class GameOverState : StateNode
{
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameEndState failed to get gameController!", this);
            return;
        }
        
        var winner = gameController.GetWinner();
        if (winner == default)
        {
            Debug.LogError($"GameEndState failed to get winner!", this);
            return;
        }

        if (!InstanceHandler.TryGetInstance(out EndGameView endGameView))
        {
            Debug.LogError($"GameEndState failed to get endGameView!", this);
            return;
        }
        
        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            Debug.LogError($"GameEndState failed to get gameViewManager!", this);
            return;
        }
        
        endGameView.SetWinner(winner);
        gameViewManager.ShowView<EndGameView>();
        Debug.Log($"Game has ended! Winner: {winner}");
    }
}
