using System.Collections;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class CharacterSelectionState : StateNode
{
    [SerializeField] private CharacterSelectController characterSelectController;
    [SerializeField] private float _selectionDuration = 30f;
    
    private Coroutine selectionTimer;
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
        selectionTimer = StartCoroutine(SelectionTimer(_selectionDuration));
        CheckTeam();
    }

    private void ClearValues()
    {
        gameController.redTeamSelections.Clear();
        gameController.blueTeamSelections.Clear();
        characterSelectController.ClearTeams();
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

    private IEnumerator SelectionTimer(float selectionDuration)
    {
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
    
    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
    }
}
