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
    //private Coroutine roundTimer;
    private float preparationPhaseTimeLeft;
    private float timeLeft;
    private float objectiveTimeLeft;
    [SerializeField] private float preparationPhaseTime = 45f;
    [SerializeField] private float roundTime = 180f;
    [SerializeField] private float objectiveTime = 45f;
    
    public override void Enter(Dictionary<GameController.Team, List<PlayerHealth>> spawnedPlayers, bool asServer)
    {
        base.Enter(spawnedPlayers, asServer);
        
        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            Debug.LogError($"CharacterSelectState: Failed to find GameViewManager", this);
            return;
        }
        
        gameViewManager.ShowView<RoundView>();
        
        if (!asServer)
        {
            return;
        }
        
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
            return;
        }
        
        _playersRed.Clear();
        _playersBlue.Clear();

        gameController.isPrepPhase.value = true;
        preparationPhaseTimeLeft = preparationPhaseTime;
        timeLeft = roundTime;
        objectiveTimeLeft = objectiveTime;
        
        // Red Team
        foreach (var player in spawnedPlayers[GameController.Team.Red])
        {
            if (player.owner.HasValue)
                _playersRed.Add(player.owner.Value);
            player.OnDeath_Server += OnPlayerDeathRed;
        }
        
        //Blue Team
        foreach (var player in spawnedPlayers[GameController.Team.Blue])
        {
            if (player.owner.HasValue)
                _playersBlue.Add(player.owner.Value);
            player.OnDeath_Server += OnPlayerDeathBlue;
        }
    }

    private void FixedUpdate()
    {
        if (!isServer || !isCurrentState)
            return;
        
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }

        if (gameController.isPrepPhase.value)
        {
            preparationPhaseTimeLeft -= Time.deltaTime;
            UpdateClientRoundTimer(preparationPhaseTimeLeft);

            if (preparationPhaseTimeLeft > 0) 
                return;
            
            preparationPhaseTimeLeft = 0;
            gameController.isPrepPhase.value = false;
            UpdateClientRoundTimer(preparationPhaseTimeLeft);

            return;
        }
        
        
        if (!gameController.isPlanted.value)
        {
            // Ends round if time hits 0 and ATK is not planting or has not planted the package
            if (timeLeft <= 0 && !gameController.isPlanted.value && !gameController.isPlanted.value)
            {
                timeLeft = 0;
                UpdateClientRoundTimer(timeLeft);
                
                machine.Next(data: false);
            }

            // Timer
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;

                UpdateClientRoundTimer(timeLeft);
            }
        }
        else if (gameController.isPlanted.value)
        {
            if (objectiveTimeLeft <= 0)
            {
                machine.Next(data: true);
            }

            if (objectiveTimeLeft > 0)
            {
                objectiveTimeLeft -= Time.deltaTime;
                
                UpdateClientRoundTimer(objectiveTimeLeft);
            }
        }
        else
        {
            Debug.LogError("Something went wrong with round timer.");
        }
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
    
    /*
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
            
            UpdateClientRoundTimer(timeLeft);

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
    */
    
    [ObserversRpc]
    private void UpdateClientRoundTimer(float _roundTimer)
    {
        if (!InstanceHandler.TryGetInstance(out RoundView roundView))
        {
            Debug.LogError($"GameStartState failed to get roundView!", this);
        }
        
        roundView.UpdateRoundTimer(_roundTimer);
    }
}
