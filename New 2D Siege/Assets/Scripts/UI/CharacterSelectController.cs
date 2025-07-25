using System.Collections.Generic;
using NUnit.Framework;
using PurrNet;
using UnityEngine;

public class CharacterSelectController : NetworkBehaviour
{
    private static CanvasGroup attackerViewStatic;
    private static CanvasGroup defenderViewStatic;
    private static GameObject attackerViewObjectStatic;
    private static GameObject defenderViewObjectStatic;
    private PlayerID selectingPlayer;
    private List<PlayerID> attackersList = new List<PlayerID>();
    private List<PlayerID> defendersList = new List<PlayerID>();
    private GameController gameController;
    
    [SerializeField] private CanvasGroup attackerView;
    [SerializeField] private CanvasGroup defenderView;
    [SerializeField] private GameObject attackerViewObject;
    [SerializeField] private GameObject defenderViewObject;
    
    private void Start()
    {
        if (!InstanceHandler.TryGetInstance(out GameController _gameController))
        {
            Debug.LogError("CharacterSelectController: GameController not found!");
            return;
        }
        gameController = _gameController;
        
        attackerViewStatic = attackerView;
        defenderViewStatic = defenderView;
        attackerViewObjectStatic = attackerViewObject;
        defenderViewObjectStatic = defenderViewObject;
    }

    public void SelectedAttacker(int characterID)
    {
        Debug.Log("Volt Selected");
        SelectedAttackCharacterRPC(characterID);
    }

    public void SelectedDefender(int characterID)
    {
        Debug.Log("Scramble Selected");
        SelectedDefenderCharacterRPC(characterID);
    }

    public void ClearTeams()
    {
        attackersList.Clear();
        defendersList.Clear();
    }
    
    [ServerRpc(requireOwnership:false)]
    private void SelectedAttackCharacterRPC(int characterID, RPCInfo info = default)
    {
        switch (gameController.teamSides[GameController.Side.Attack])
        {
            case GameController.Team.Red:
                gameController.redTeamSelections[info.sender] = characterID;
                Debug.Log($"{info.sender} has been added to Red Players dictionary: {gameController.redTeamSelections.ContainsKey(info.sender)}");
                break;
            case GameController.Team.Blue:
                gameController.blueTeamSelections[info.sender] = characterID;
                Debug.Log($"{info.sender} has been added to Blue Players dictionary: {gameController.blueTeamSelections.ContainsKey(info.sender)}");
                break;
        }
        
    }
    
    [ServerRpc(requireOwnership:false)]
    private void SelectedDefenderCharacterRPC(int characterID, RPCInfo info = default)
    {
        switch (gameController.teamSides[GameController.Side.Defense])
        {
            case GameController.Team.Red:
                gameController.redTeamSelections[info.sender] = characterID;
                Debug.Log($"{info.sender} has been added to Red Players dictionary: {gameController.redTeamSelections.ContainsKey(info.sender)}");
                break;
            case GameController.Team.Blue:
                gameController.blueTeamSelections[info.sender] = characterID;
                Debug.Log($"{info.sender} has been added to Blue Players dictionary: {gameController.blueTeamSelections.ContainsKey(info.sender)}");
                break;
        }
    }
    
    public void ShowCharacterSelect(PlayerID player, GameController.Side side)
    {
        switch (side)
        {
            case GameController.Side.Attack:
                attackersList.Add(player);
                break;
            case GameController.Side.Defense:
                defendersList.Add(player);
                break;
        }
        ShowCharacters(player, side);
    }
    
    [TargetRpc]
    public static void ShowCharacters(PlayerID target, GameController.Side side)
    {
        switch (side)
        {
            case GameController.Side.Attack:
                attackerViewObjectStatic.SetActive(true);
                attackerViewStatic.alpha = 1;
                attackerViewStatic.interactable = true;
                defenderViewStatic.alpha = 0;
                defenderViewStatic.interactable = false;
                defenderViewObjectStatic.SetActive(false);
                break;
            case GameController.Side.Defense:
                defenderViewObjectStatic.SetActive(true);
                attackerViewStatic.alpha = 0;
                attackerViewStatic.interactable = false;
                defenderViewStatic.alpha = 1;
                defenderViewStatic.interactable = true;
                attackerViewObjectStatic.SetActive(false);
                break;
        }
    }
}
