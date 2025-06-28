using System;
using PurrNet;
using UnityEngine;

public class GameController : NetworkBehaviour
{
    public bool debugMode = false;
    
    [SerializeField] private SyncDictionary<PlayerID, ScoreData> scores = new();

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this); //A safer singleton; needs a OnDestroy
        scores.onChanged += OnScoresChanged;
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
}
