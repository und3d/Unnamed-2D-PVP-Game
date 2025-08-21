using System.Collections.Generic;
using PurrNet.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player/Settings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Movement Settings")] 
    public InputActionAsset playerInput;
    public bool toggleSprint = false;
    public float walkSpeed = 4f;
    public float sprintSpeed = 6f;
    
    [Header("Barricade Settings")]
    public float barricadeInteractRange = 1f;
    public float barricadePlacementTime = 1.5f;
    public float timeBetweenBarricadeInteractions = 1f;
    
    
    [Header("Wall Settings")]
    public float wallInteractRange = 1f;
    public float reinforcementTime = 3.5f;
    public float timeBetweenReinforcements = 1f;
    
    [Header("Interaction Layers")]
    public LayerMask interactLayers;
}
