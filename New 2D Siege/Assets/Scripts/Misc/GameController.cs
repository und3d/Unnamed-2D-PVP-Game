using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    public bool debugMode = false;
    public bool canMove = true;
    public bool isGadgetEquipped;
    public bool justThrewGadget;
    public bool canActivateThrownGadget;
    
    public ProgressBarController progressBar;
    [SerializeField] private GameObject playerGlobalLightAttack;
    [SerializeField] private GameObject playerGlobalLightDefense;
    
    [SerializeField] private SyncDictionary<PlayerID, ScoreData> scores = new();
    
    public enum Team {
        Red, Blue
    }

    public enum Side
    {
        Attack, Defense
    }
    
    public SyncDictionary<Team, int> roundScores = new();
    public SyncDictionary<Side, Team> teamSides = new();
    public SyncDictionary<PlayerID, int> redTeamSelections = new();
    public SyncDictionary<PlayerID, int> blueTeamSelections = new();
    public SyncDictionary<Team, List<PlayerID>> GlobalTeams = new();
    public SyncVar<bool> isPlanting = new(false);
    public SyncVar<bool> isPlanted = new(false);

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this); //A safer singleton; needs a OnDestroy
        scores.onChanged += OnScoresChanged;
        
        GlobalTeams = new SyncDictionary<Team, List<PlayerID>>()
        {
            { Team.Red, new List<PlayerID>() },
            { Team.Blue, new List<PlayerID>() }
        };

        roundScores = new SyncDictionary<Team, int>()
        {
            { Team.Red, 0 },
            { Team.Blue, 0 }
        };

        teamSides = new SyncDictionary<Side, Team>()
        {
            { Side.Attack, Team.Red },
            { Side.Defense, Team.Blue }
        };
    }

    private void OnScoresChanged(SyncDictionaryChange<PlayerID, ScoreData> change)
    {
        if (InstanceHandler.TryGetInstance(out ScoreboardView scoreboardView))
        {
            scoreboardView.SetData(scores.ToDictionary());
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        InstanceHandler.UnregisterInstance<GameController>();
    }

    public void AddRoundWin(Team team)
    {
        var roundScoreData = roundScores[team];
        roundScoreData++;
        roundScores[team] = roundScoreData;
    }
    
    public void AddKill(PlayerID playerID)
    {
        CheckForDictionaryEntry(playerID);
        
        var scoreData = scores[playerID]; //Get score data for player
        scoreData.kills++; //Increase kills
        scores[playerID] = scoreData; //Update score data
    }

    public void AddDeath(PlayerID playerID)
    {
        CheckForDictionaryEntry(playerID);
        
        var scoreData = scores[playerID]; //Get score data for player
        scoreData.deaths++; //Increase deaths
        scores[playerID] = scoreData; //Update score data
    }
    
    public void AddAssist(PlayerID playerID)
    {
        CheckForDictionaryEntry(playerID);
        
        var scoreData = scores[playerID]; //Get score data for player
        scoreData.assists++; //Increase assists
        scores[playerID] = scoreData; //Update score data
    }

    public PlayerID GetWinner()
    {
        PlayerID winner = default;
        var highestKills = 0;

        foreach (var score in scores)
        {
            if (score.Value.kills > highestKills)
            {
                highestKills = score.Value.kills;
                winner = score.Key;
            }
        }

        return winner;
    }
    
    private void CheckForDictionaryEntry(PlayerID playerID)
    {
        if (!scores.ContainsKey(playerID))
        {
            scores.Add(playerID, new ScoreData());
        }
    }
    
    public struct ScoreData
    {
        public int kills;
        public int deaths;
        public int assists;

        public override string ToString()
        {
            return $"K: {kills} D: {deaths} A: {assists}";
        }
    }
    
    public Side GetPlayerSide(PlayerID playerId)
    {
        // First, find the player's team
        foreach (var (team, players) in GlobalTeams)
        {
            if (players.Contains(playerId))
            {
                // Now find the side that matches this team
                foreach (var sidePair in teamSides)
                {
                    if (sidePair.Value.Equals(team))
                    {
                        return sidePair.Key;
                    }
                }
            }
        }

        throw new Exception($"PlayerID {playerId} not found in any team.");
    }

    [TargetRpc]
    public void ToggleGlobalLight(PlayerID playerId, Side side)
    {
        switch (side)
        {
            case Side.Attack:
                playerGlobalLightAttack.SetActive(true);
                playerGlobalLightDefense.SetActive(false);
                break;
            case Side.Defense:
                playerGlobalLightDefense.SetActive(true);
                playerGlobalLightAttack.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(side), side, null);
        }
    }
}
