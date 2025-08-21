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
    private int previousSelection = -1;
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
        {
            hasSelected = true;
            previousSelection = characterID;
            SelectedCharacterRPC(characterID, GameController.Side.Attack);
        }
        else
        {
            SelectedCharacterRPC(characterID, previousSelection, GameController.Side.Attack);
            previousSelection = characterID;
        }
    }

    public void SelectedDefender(int characterID)
    {
        if (!hasSelected)
        {
            hasSelected = true;
            previousSelection = characterID;
            SelectedCharacterRPC(characterID, GameController.Side.Defense);
        }
        else
        {
            SelectedCharacterRPC(characterID, previousSelection, GameController.Side.Defense);
            previousSelection = characterID;
        }
    }
    
    // Tells server who picked what character
    // FIRST SELECTION
    [ServerRpc(requireOwnership:false)]
    private void SelectedCharacterRPC(int selectedCharacterID, GameController.Side side, RPCInfo info = default)
    {
        ToggleCharacterButton(selectedCharacterID, side);
        
        switch (gameController.teamSides[side])
        {
            case GameController.Team.Red:
                gameController.redTeamSelections[info.sender] = selectedCharacterID;
                gameController.redTeamRemainingIDs.Remove(selectedCharacterID);
                //Debug.Log($"{info.sender} has been added to Red Players dictionary: {gameController.redTeamSelections.ContainsKey(info.sender)}");
                break;
            case GameController.Team.Blue:
                
                gameController.blueTeamSelections[info.sender] = selectedCharacterID;
                gameController.blueTeamRemainingIDs.Remove(selectedCharacterID);
                //Debug.Log($"{info.sender} has been added to Blue Players dictionary: {gameController.blueTeamSelections.ContainsKey(info.sender)}");
                break;
        }
    }
    
    // Tells server who picked what character and their PREVIOUS selection
    // HAS SELECTED PREVIOUSLY
    [ServerRpc(requireOwnership:false)]
    private void SelectedCharacterRPC(int selectedCharacterID, int previousCharacterID, GameController.Side side, RPCInfo info = default)
    {
        ToggleCharacterButton(selectedCharacterID, side);
        ToggleCharacterButton(previousCharacterID, side);
        
        switch (gameController.teamSides[side])
        {
            case GameController.Team.Red:
                gameController.redTeamSelections[info.sender] = selectedCharacterID;
                gameController.redTeamRemainingIDs.Remove(selectedCharacterID);
                gameController.redTeamRemainingIDs.Add(previousCharacterID);
                //Debug.Log($"{info.sender} has been added to Red Players dictionary: {gameController.redTeamSelections.ContainsKey(info.sender)}");
                break;
            case GameController.Team.Blue:
                gameController.blueTeamSelections[info.sender] = selectedCharacterID;
                gameController.blueTeamRemainingIDs.Remove(selectedCharacterID);
                gameController.blueTeamRemainingIDs.Add(previousCharacterID);
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
    
    // Toggles the button for the selected character
    [ObserversRpc]
    private void ToggleCharacterButton(int selectedCharacterID, GameController.Side side)
    {
        switch (side)
        {
            case GameController.Side.Attack:
                attackerButtons[selectedCharacterID].interactable = !attackerButtons[selectedCharacterID].interactable;
                break;
            case GameController.Side.Defense:
                defenderButtons[selectedCharacterID].interactable = !defenderButtons[selectedCharacterID].interactable;
                break;
        }
    }
    
    // Toggles the button for the current and previous selections
    [ObserversRpc]
    private void ToggleCharacterButton(int selectedCharacterID, int previousCharacterID, GameController.Side side)
    {
        switch (side)
        {
            case GameController.Side.Attack:
                attackerButtons[selectedCharacterID].interactable = !attackerButtons[selectedCharacterID].interactable;
                attackerButtons[previousCharacterID].interactable = !attackerButtons[previousCharacterID].interactable;
                break;
            case GameController.Side.Defense:
                defenderButtons[selectedCharacterID].interactable = !defenderButtons[selectedCharacterID].interactable;
                defenderButtons[previousCharacterID].interactable = !defenderButtons[previousCharacterID].interactable;
                break;
        }
    }
}
