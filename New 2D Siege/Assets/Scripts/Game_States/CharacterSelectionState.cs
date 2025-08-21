using System;
using System.Collections;
using NUnit.Framework.Interfaces;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class CharacterSelectionState : StateNode
{
    [SerializeField] private CharacterSelectController characterSelectController;
    [SerializeField] private CharacterSelectView characterSelectView;
    [SerializeField] private float _selectionDuration = 30f;
    
    private float timeLeft;
    private GameController gameController;
    private GameViewManager gameViewManager;
    
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        
        GetReferences();
        
        gameViewManager.ShowView<CharacterSelectView>();
        
        if (!asServer)
            return;
        
        DespawnPlayers();
        DespawnGadgets();
        
        ClearValues();
        timeLeft = _selectionDuration;
        CheckTeam();
    }

    private void ClearValues()
    {
        gameController.redTeamSelections.Clear();
        gameController.blueTeamSelections.Clear();
        
        gameController.redTeamRemainingIDs = new SyncList<int>()
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        };

        gameController.blueTeamRemainingIDs = new SyncList<int>()
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        };
    }

    private void CheckTeam()
    {
        foreach (var player in gameController.GlobalTeams[gameController.teamSides[GameController.Side.Attack]])
        {
            characterSelectController.ShowCharacterSelect(player, GameController.Side.Attack);
        }

        foreach (var player in gameController.GlobalTeams[gameController.teamSides[GameController.Side.Defense]])
        {
            characterSelectController.ShowCharacterSelect(player, GameController.Side.Defense);
        }
    }

    private void FixedUpdate()
    {
        if (!isServer || !isCurrentState)
            return;
        
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            UpdateClientSelectionTimer(timeLeft);
            RandomlySelectCharacters(GameController.Side.Attack);
            RandomlySelectCharacters(GameController.Side.Defense);

            machine.Next();
        }
        else if (timeLeft > 0)
        {
            if (timeLeft > 3f && gameController.redTeamSelections.Count + gameController.blueTeamSelections.Count == networkManager.players.Count)
            {
                Debug.Log($"Red Team Selection Count: {networkManager.players.Count} + Blue Team Selection Count: {gameController.blueTeamSelections.Count} == Total Players: {networkManager.players.Count}");
                timeLeft = 3f;
            }
            
            timeLeft -= Time.deltaTime;
            UpdateClientSelectionTimer(timeLeft);
        }
    }

    private void GetReferences()
    {
        if (!InstanceHandler.TryGetInstance(out gameViewManager))
        {
            Debug.LogError($"CharacterSelectState: Failed to find GameViewManager", this);
            return;
        }
        
        if (!InstanceHandler.TryGetInstance(out gameController))
        {
            Debug.LogError("CharacterSelectionState : No GameController found");
            return;
        }
    }
    
    private void DespawnPlayers()
    {
        var allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            Destroy(player.gameObject);
        }
    }

    private void DespawnGadgets()
    {
        var allGadgets = FindObjectsByType<GadgetBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var gadget in allGadgets)
        {
            Destroy(gadget.gameObject);
        }
    }

    private void RandomlySelectCharacters(GameController.Side side)
    {
        var randomCharacterID = - 1;
        switch (gameController.teamSides[side])
        {
            case GameController.Team.Red:
                foreach (var player in gameController.GlobalTeams[GameController.Team.Red])
                {
                    if (gameController.redTeamSelections.ContainsKey(player))
                        continue;
                        
                    randomCharacterID = gameController.redTeamRemainingIDs[Random.Range(0, gameController.redTeamRemainingIDs.Count)];
                    
                    gameController.redTeamRemainingIDs.Remove(randomCharacterID);
                    gameController.redTeamSelections.Add(player, randomCharacterID);
                }
                break;
            case GameController.Team.Blue:
                foreach (var player in gameController.GlobalTeams[GameController.Team.Blue])
                {
                    if (gameController.blueTeamSelections.ContainsKey(player))
                        continue;
                        
                    randomCharacterID = gameController.blueTeamRemainingIDs[Random.Range(0, gameController.blueTeamRemainingIDs.Count)];
                    
                    gameController.blueTeamRemainingIDs.Remove(randomCharacterID);
                    gameController.blueTeamSelections.Add(player, randomCharacterID);
                }
                break;
        }
    }

    [ObserversRpc]
    private void UpdateClientSelectionTimer(float selectionTimeLeft)
    {
        characterSelectView.UpdateSelectionTimer(selectionTimeLeft);
    }
    
    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}
