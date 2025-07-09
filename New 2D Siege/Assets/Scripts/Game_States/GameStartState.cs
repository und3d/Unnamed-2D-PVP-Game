using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class GameStartState : StateNode
{
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        
        if (!asServer)
            return;
        
        SetTeams();
        
        machine.Next();
    }

    private void SetTeams()
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
            return;
        }
        
        // Clear the teams on new game start
        foreach (var playerList in gameController.GlobalTeams.Values)
        {
            playerList.Clear();
        }
        
        int redCount = 0;
        int blueCount = 0;
        int half = networkManager.players.Count / 2;
        
        foreach (var player in networkManager.players)
        {
            // If Red Team is full
            if (redCount >= half)
            {
                gameController.GlobalTeams[GameController.Team.Blue].Add(player);
                blueCount++;
            }
            // If Blue Team is full
            else if (blueCount >= half)
            {
                gameController.GlobalTeams[GameController.Team.Red].Add(player);
                redCount++;
            }
            else
            {
                // If both teams have open slots, assign a player to a team
                if (Random.value < 0.5f)
                {
                    gameController.GlobalTeams[GameController.Team.Red].Add(player);
                    redCount++;
                }
                else
                {
                    gameController.GlobalTeams[GameController.Team.Blue].Add(player);
                    blueCount++;
                }
            }
        }
        
        Debug.Log("Completed Setting Teams");
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}
