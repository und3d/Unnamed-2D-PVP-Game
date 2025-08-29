using System;
using System.Collections.Generic;
using PurrNet;
using PurrNet.Packing;
using PurrNet.StateMachine;
using UnityEngine;

public static class GameExtensions
{
    [RegisterPackers]
    private static void RegisterPackers()
    {
        PackCollections.RegisterList<PlayerHealth>();
    }
}

public class PlayerSpawningState : StateNode
{
    [SerializeField] private PlayerHealth playerPrefab;
    [SerializeField] private GameObject dynamicObjectsParent;
    [SerializeField] private GameObject mapParent;
    [SerializeField] private List<PlayerHealth> attackerPrefabs;
    [SerializeField] private List<PlayerHealth> defenderPrefabs;
    //[SerializeField] private List<Transform> spawnPoints = new();

    [SerializeField] private List<Transform> spawnPointsAttack = new();
    [SerializeField] private List<Transform> spawnPointsDefense = new();

    private GameController gameController;
    private RoundView roundView;
    private Transform spawnPoint;
    private GameObject spawnedDynamicObjects;

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!InstanceHandler.TryGetInstance(out roundView))
        {
            Debug.LogError($"PlayerSpawningState failed to get roundView!", this);
        }
        
        roundView.ResetAllPlayerIcons();
        
        if (!asServer)
            return;
        
        if (spawnedDynamicObjects)
            Destroy(spawnedDynamicObjects);
        
        spawnedDynamicObjects = Instantiate(dynamicObjectsParent, mapParent.transform);
        
        //ResetDynamicObjects();
        
        if (!InstanceHandler.TryGetInstance(out gameController))
        {
            Debug.LogError($"PlayerSpawningState failed to get gameController!", this);
        }
        
        var spawnedPlayers = SpawnPlayersWithTeam();
        
        //Debug.Log($"Sending {spawnedPlayers}");
        machine.Next(spawnedPlayers);
    }

    private Dictionary<GameController.Team, List<PlayerHealth>> SpawnPlayersWithTeam()
    {
        var spawnedPlayers = new Dictionary<GameController.Team, List<PlayerHealth>>()
        {
            { GameController.Team.Red, new List<PlayerHealth>() },
            { GameController.Team.Blue, new List<PlayerHealth>() }
        };
        
        //Debug.Log("Red Players: " + string.Join(", ", gameController.redTeamSelections.Keys));
        //Debug.Log("Blue Players: " + string.Join(", ", gameController.blueTeamSelections.Keys));
        
        var redSide = GetSideForTeam(GameController.Team.Red);
        
        // Spawn Red Team
        var currentSpawnIndex = 0;
        foreach (var player in gameController.GlobalTeams[GameController.Team.Red])
        {
            switch (redSide)
            {
                case GameController.Side.Attack:
                    playerPrefab = attackerPrefabs[gameController.redTeamSelections[player]];
                    spawnPoint = spawnPointsAttack[currentSpawnIndex];
                    gameController.ToggleGlobalLight(player, GameController.Side.Attack);
                    break;
                case GameController.Side.Defense:
                    playerPrefab = defenderPrefabs[gameController.redTeamSelections[player]];
                    spawnPoint = spawnPointsDefense[currentSpawnIndex];
                    gameController.ToggleGlobalLight(player, GameController.Side.Defense);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            newPlayer.GetComponent<PlayerHealth>().SetColor(Color.red);
            newPlayer.GetComponent<PlayerHealth>().SetPlayerIconID(currentSpawnIndex);
            SetPlayerIconAlive(currentSpawnIndex, GameController.Team.Red);
            newPlayer.GiveOwnership(player);
            spawnedPlayers[GameController.Team.Red].Add(newPlayer);
            currentSpawnIndex++;
        }
        
        //Debug.Log($"Spawned {currentSpawnIndex} Red team players");
        
        var blueSide = GetSideForTeam(GameController.Team.Blue);
        
        // Spawn Defenders
        currentSpawnIndex = 0;
        foreach (var player in gameController.GlobalTeams[GameController.Team.Blue])
        {
            switch (blueSide)
            {
                case GameController.Side.Attack:
                    playerPrefab = attackerPrefabs[gameController.blueTeamSelections[player]];
                    spawnPoint = spawnPointsAttack[currentSpawnIndex];
                    gameController.ToggleGlobalLight(player, GameController.Side.Attack);
                    break;
                case GameController.Side.Defense:
                    playerPrefab = defenderPrefabs[gameController.blueTeamSelections[player]];
                    spawnPoint = spawnPointsDefense[currentSpawnIndex];
                    gameController.ToggleGlobalLight(player, GameController.Side.Defense);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            newPlayer.GetComponent<PlayerHealth>().SetColor(Color.blue);
            newPlayer.GetComponent<PlayerHealth>().SetPlayerIconID(currentSpawnIndex);
            SetPlayerIconAlive(currentSpawnIndex, GameController.Team.Blue);
            newPlayer.GiveOwnership(player);
            spawnedPlayers[GameController.Team.Blue].Add(newPlayer);
            currentSpawnIndex++;
        }
        
        //Debug.Log($"Spawned {currentSpawnIndex} Blue team players");
        //Debug.Log($"Spawned {currentSpawnIndex} Blue team players");
        
        return spawnedPlayers;
    }

    private GameController.Side GetSideForTeam(GameController.Team team)
    {
        foreach (var kvp in gameController.teamSides)
        {
            if (kvp.Value == team)
            {
                return kvp.Key;
            }
        }

        throw new System.Exception($"Team {team} not found in teamSides!");
    }

    [ObserversRpc]
    private void SetPlayerIconAlive(int playerIconID, GameController.Team team)
    {
        if (team == GameController.Team.Red)
            roundView.SetRedPlayerIconAlive(playerIconID);
        else
            roundView.SetBluePlayerIconAlive(playerIconID);
    }

    [ObserversRpc(runLocally:false)]
    private void ResetDynamicObjects()
    {
        if (spawnedDynamicObjects)
            Destroy(spawnedDynamicObjects);
        
        spawnedDynamicObjects = Instantiate(dynamicObjectsParent, mapParent.transform);
    }
    
    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}
