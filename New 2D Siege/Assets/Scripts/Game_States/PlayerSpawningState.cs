using System.Collections.Generic;
using PurrNet;
using PurrNet.Packing;
using PurrNet.StateMachine;
using UnityEngine;

public static class GameExtensions
{
    [RegisterPackers]
    static void RegisterPackers()
    {
        PackCollections.RegisterList<PlayerHealth>();
    }
}

public class PlayerSpawningState : StateNode
{
    [SerializeField] private PlayerHealth playerPrefab;
    [SerializeField] private List<PlayerHealth> attackerPrefabs;
    [SerializeField] private List<PlayerHealth> defenderPrefabs;
    //[SerializeField] private List<Transform> spawnPoints = new();

    [SerializeField] private List<Transform> spawnPointsRed = new();
    [SerializeField] private List<Transform> spawnPointsBlue = new();
    

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        
        if (!asServer)
            return;

        DespawnPlayers();
        DespawnGadgets();
        
        var spawnedPlayers = SpawnPlayersWithTeam();
        
        Debug.Log($"Sending {spawnedPlayers}");
        machine.Next(spawnedPlayers);
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
        var allGadgets = FindObjectsByType<PlaceableGadget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var gadget in allGadgets)
        {
            Destroy(gadget.gameObject);
        }
    }

    /*private List<PlayerHealth> SpawnPlayers()
    {
        var spawnedPlayers = new List<PlayerHealth>();

        int currentSpawnIndex = 0;
        foreach (var player in networkManager.players)
        {
            var spawnPoint = spawnPoints[currentSpawnIndex];
            var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            newPlayer.GiveOwnership(player);
            spawnedPlayers.Add(newPlayer);
            currentSpawnIndex++;
            
            if (currentSpawnIndex >= spawnPoints.Count)
                currentSpawnIndex = 0;
        }
        
        return spawnedPlayers;
    }*/

    private Dictionary<GameController.Team, List<PlayerHealth>> SpawnPlayersWithTeam()
    {
        var spawnedPlayers = new Dictionary<GameController.Team, List<PlayerHealth>>()
        {
            { GameController.Team.Red, new List<PlayerHealth>() },
            { GameController.Team.Blue, new List<PlayerHealth>() }
        };
        
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }
        
        playerPrefab = attackerPrefabs[0];
        
        // Spawn Red Team
        int currentSpawnIndex = 0;
        foreach (var player in gameController.GlobalTeams[GameController.Team.Red])
        {
            var spawnPoint = spawnPointsRed[currentSpawnIndex];
            var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            newPlayer.GetComponent<PlayerHealth>().SetColor(true);
            newPlayer.GiveOwnership(player);
            spawnedPlayers[GameController.Team.Red].Add(newPlayer);
            currentSpawnIndex++;
        }
        
        Debug.Log($"Spawned {currentSpawnIndex} Red team players");
        
        playerPrefab = defenderPrefabs[0];
        
        // Spawn Blue Team
        currentSpawnIndex = 0;
        foreach (var player in gameController.GlobalTeams[GameController.Team.Blue])
        {
            var spawnPoint = spawnPointsBlue[currentSpawnIndex];
            var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            newPlayer.GetComponent<PlayerHealth>().SetColor(false);
            newPlayer.GiveOwnership(player);
            spawnedPlayers[GameController.Team.Blue].Add(newPlayer);
            currentSpawnIndex++;
        }
        
        Debug.Log($"Spawned {currentSpawnIndex} Blue team players");
        
        return spawnedPlayers;
    }


    
    
    
    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}
