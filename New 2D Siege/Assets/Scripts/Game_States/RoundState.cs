using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

public class RoundState : StateNode<Dictionary<GameController.Team, List<PlayerHealth>>>
{
    private List<PlayerID> _playersRed = new();
    private List<PlayerID> _playersBlue = new();
    [SerializeField] private float roundTime = 180f;
    [SerializeField] private float objectiveTime = 45f;
    
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
        
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
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
        
        StartCoroutine(RoundTimer(roundTime));
    }

    private void Update()
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }

        if (!gameController.isPlanted.value)
            return;
        
    }

    private void OnPlayerDeathRed(PlayerID deadPlayer)
    {
        _playersRed.Remove(deadPlayer);

        if (_playersRed.Count <= 0)
        {
            if (!InstanceHandler.TryGetInstance(out GameController gameController))
            {
                Debug.LogError($"GameStartState failed to get gameController!", this);
            }
            
            // Boolean expression (Sets blueIsAttacking to true or false depending on if blue team is on Attack)
            // Red is the losing team here, meaning Blue has won the round. machine.Next() takes in a bool for if the WINNING team is on ATK or not.
            bool blueIsAttacking = gameController.teamSides[GameController.Side.Attack] == GameController.Team.Blue;
            
            machine.Next(data:blueIsAttacking);
        }
    }
    
    private void OnPlayerDeathBlue(PlayerID deadPlayer)
    {
        _playersBlue.Remove(deadPlayer);

        if (_playersBlue.Count <= 0)
        {
            if (!InstanceHandler.TryGetInstance(out GameController gameController))
            {
                Debug.LogError($"GameStartState failed to get gameController!", this);
            }
            
            // Boolean expression (Sets redIsAttacking to true or false depending on if red team is on Attack)
            // Blue is the losing team here, meaning Red has won the round. machine.Next() takes in a bool for if the WINNING team is on ATK or not.
            bool redIsAttacking = gameController.teamSides[GameController.Side.Attack] == GameController.Team.Red;
            
            machine.Next(data:redIsAttacking);
        }
    }

    private IEnumerator RoundTimer(float roundDuration)
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }
        
        float timeLeft = roundDuration;
        
        // Timer
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;

            yield return null;
        }
        
        // Ends round if time hits 0 and ATK is not planting or has not planted the package
        if (!gameController.isPlanting.value && !gameController.isPlanted.value)
        {
            machine.Next(data:false);
        }
    }

    private IEnumerator ObjectiveTimer(float objectiveDuration)
    {
        float timeLeft = objectiveDuration;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            
            yield return null;
        }
        
        machine.Next(data:true);
    }
}
