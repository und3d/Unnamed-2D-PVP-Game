using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class RoundState : StateNode<Dictionary<GameController.Team, List<PlayerHealth>>>
{
    private List<PlayerID> _playersRed = new();
    private List<PlayerID> _playersBlue = new();
    
    /*public override void Enter(List<PlayerHealth> data, bool asServer)
    {
        base.Enter(data, asServer);

        if (!asServer)
            return;
        
        _players.Clear();
        foreach (var player in data)
        {
            if (player.owner.HasValue)
                _players.Add(player.owner.Value);
            player.OnDeath_Server += OnPlayerDeath;
        }
    }
    */
    
    public override void Enter(Dictionary<GameController.Team, List<PlayerHealth>> spawnedPlayers, bool asServer)
    {
        Debug.Log($"Entering RoundState. Is this server? {asServer}");
        base.Enter(spawnedPlayers, asServer);

        if (!asServer)
        {
            Debug.Log("Exiting RoundState as a client.");
            return;
        }
        
        _playersRed.Clear();
        _playersBlue.Clear();
        
        // Red Team
        foreach (var player in spawnedPlayers[GameController.Team.Red])
        {
            if (player.owner.HasValue)
                _playersRed.Add(player.owner.Value);
            player.OnDeath_Server += OnPlayerDeathRed;
        }
        
        Debug.Log($"Red team players: {_playersRed.Count}");
        
        //Blue Team
        foreach (var player in spawnedPlayers[GameController.Team.Blue])
        {
            if (player.owner.HasValue)
                _playersBlue.Add(player.owner.Value);
            player.OnDeath_Server += OnPlayerDeathBlue;
        }
        
        Debug.Log($"Blue team players: {_playersBlue.Count}");
    }

    private void OnPlayerDeathRed(PlayerID deadPlayer)
    {
        _playersRed.Remove(deadPlayer);

        if (_playersRed.Count <= 0)
        {
            machine.Next();
        }
    }
    
    private void OnPlayerDeathBlue(PlayerID deadPlayer)
    {
        _playersBlue.Remove(deadPlayer);

        if (_playersBlue.Count <= 0)
        {
            machine.Next();
        }
    }
}
