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
    public float interactRange = 2f;
    public float placementTime = 3f;
    public LayerMask interactLayers;
}
