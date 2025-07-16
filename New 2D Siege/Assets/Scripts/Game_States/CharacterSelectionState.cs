using System.Collections;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class CharacterSelectionState : StateNode
{
    [SerializeField] private CharacterSelectController characterSelectController;
    [SerializeField] private float _selectionDuration = 30f;
    
    private Coroutine selectionTimer;
    
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        
        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            Debug.LogError($"CharacterSelectState: Failed to find GameViewManager", this);
            return;
        }
        
        gameViewManager.ShowView<CharacterSelectView>();
        
        if (!asServer)
            return;

        ClearValues();
        selectionTimer = StartCoroutine(SelectionTimer(_selectionDuration));
        CheckTeam();
    }

    private void ClearValues()
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError("CharacterSelectionState : No GameController found");
            return;
        }
        
        gameController.redTeamSelections.Clear();
        gameController.blueTeamSelections.Clear();
        characterSelectController.ClearTeams();
    }

    private void CheckTeam()
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError("CharacterSelectionState : No GameController found");
            return;
        }

        foreach (var player in gameController.GlobalTeams[GameController.Team.Red])
        {
            characterSelectController.ShowCharacterSelect(player, GameController.Team.Red);
        }

        foreach (var player in gameController.GlobalTeams[GameController.Team.Blue])
        {
            characterSelectController.ShowCharacterSelect(player, GameController.Team.Blue);
        }
    }

    private IEnumerator SelectionTimer(float selectionDuration)
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError("CharacterSelectionState : No GameController found");
        }
        
        float timeLeft = selectionDuration;
        
        // Timer
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;

            if (gameController.redTeamSelections.Count + gameController.blueTeamSelections.Count ==
                networkManager.players.Count)
            {
                machine.Next();
                StopCoroutine(selectionTimer);
            }
            
            yield return null;
        }
        
        
        machine.Next();
    }
    
    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}
