using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class RoundState : StateNode<List<PlayerHealth>>
{
    private List<PlayerID> _players = new();
    
    public override void Enter(List<PlayerHealth> data, bool asServer)
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

    private void OnPlayerDeath(PlayerID deadPlayer)
    {
        _players.Remove(deadPlayer);

        if (_players.Count <= 1)
        {
            machine.Next();
        }

    }
}
