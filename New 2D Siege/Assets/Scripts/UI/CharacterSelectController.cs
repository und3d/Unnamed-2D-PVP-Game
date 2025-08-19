using System.Collections.Generic;
using NUnit.Framework;
using PurrNet;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectController : NetworkBehaviour
{
    private static CanvasGroup attackerViewStatic;
    private static CanvasGroup defenderViewStatic;
    private static GameObject attackerViewObjectStatic;
    private static GameObject defenderViewObjectStatic;
    private PlayerID selectingPlayer;
    private int selectedCharacter = -1;
    private GameController gameController;
    private static bool hasSelected = false;
    
    [SerializeField] private List<Button> attackerButtons = new List<Button>();
    [SerializeField] private List<Button> defenderButtons = new List<Button>();
    
    private static List<Button> attackerButtonsStatic = new List<Button>();
    private static List<Button> defenderButtonsStatic = new List<Button>();
    
    [SerializeField] private CanvasGroup attackerView;
    [SerializeField] private CanvasGroup defenderView;
    [SerializeField] private GameObject attackerViewObject;
    [SerializeField] private GameObject defenderViewObject;
    
    // Client clicks op button -> Selected Attacker/Defender (ServerRPC) -> Server Checks which team, adds {id, selectionID) to team dictionary -> Server uses these dictionaries to spawn players
    // Server needs to track which character IDs have been taken. If character ID gets taken, gray out and disable character's button.
    // Once timer runs out, server needs to compare selection dictionaries to team dictionaries. Players not yet in selection dictionaries get assigned random remaining character ID.
    
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
        attackerButtonsStatic = attackerButtons;
        defenderButtonsStatic = defenderButtons;
    }

    private static void ResetButtons()
    {
        foreach (var button in attackerButtonsStatic)
        {
            button.interactable = true;
        }

        foreach (var button in defenderButtonsStatic)
        {
            button.interactable = true;
        }
    }

    public void SelectedAttacker(int characterID)
    {
        if (!hasSelected)
            hasSelected = true;
        else
            ToggleCharacterButton(selectedCharacter, GameController.Side.Attack);
        
        selectedCharacter = characterID;
        SelectedAttackCharacterRPC(characterID);
    }

    public void SelectedDefender(int characterID)
    {
        if (!hasSelected)
            hasSelected = true;
        else
            ToggleCharacterButton(selectedCharacter, GameController.Side.Defense);
        
        selectedCharacter = characterID;
        SelectedDefenderCharacterRPC(characterID);
    }
    
    [ServerRpc(requireOwnership:false)]
    private void SelectedAttackCharacterRPC(int characterID, RPCInfo info = default)
    {
        ToggleCharacterButton(characterID, GameController.Side.Attack);
        switch (gameController.teamSides[GameController.Side.Attack])
        {
            case GameController.Team.Red:
                gameController.redTeamSelections[info.sender] = characterID;
                //Debug.Log($"{info.sender} has been added to Red Players dictionary: {gameController.redTeamSelections.ContainsKey(info.sender)}");
                break;
            case GameController.Team.Blue:
                
                gameController.blueTeamSelections[info.sender] = characterID;
                //Debug.Log($"{info.sender} has been added to Blue Players dictionary: {gameController.blueTeamSelections.ContainsKey(info.sender)}");
                break;
        }
        
    }
    
    [ServerRpc(requireOwnership:false)]
    private void SelectedDefenderCharacterRPC(int characterID, RPCInfo info = default)
    {
        ToggleCharacterButton(characterID, GameController.Side.Defense);
        switch (gameController.teamSides[GameController.Side.Defense])
        {
            case GameController.Team.Red:
                gameController.redTeamSelections[info.sender] = characterID;
                //Debug.Log($"{info.sender} has been added to Red Players dictionary: {gameController.redTeamSelections.ContainsKey(info.sender)}");
                break;
            case GameController.Team.Blue:
                gameController.blueTeamSelections[info.sender] = characterID;
                //Debug.Log($"{info.sender} has been added to Blue Players dictionary: {gameController.blueTeamSelections.ContainsKey(info.sender)}");
                break;
        }
    }
    
    public void ShowCharacterSelect(PlayerID player, GameController.Side side)
    {
        ShowCharacters(player, side);
    }
    
    [TargetRpc]
    public static void ShowCharacters(PlayerID target, GameController.Side side)
    {
        hasSelected = false;
        ResetButtons();
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

    [ObserversRpc]
    private void ToggleCharacterButton(int characterID, GameController.Side side)
    {
        switch (side)
        {
            case GameController.Side.Attack:
                attackerButtons[characterID].interactable = !attackerButtons[characterID].interactable;
                break;
            case GameController.Side.Defense:
                defenderButtons[characterID].interactable = !defenderButtons[characterID].interactable;
                break;
        }
    }
}
