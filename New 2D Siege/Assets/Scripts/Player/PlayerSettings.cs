using System.Collections.Generic;
using PurrNet.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Player/Settings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Movement Settings")] 
    public bool toggleSprint = false;
    public float walkSpeed = 4f;
    public float sprintSpeed = 6f;
    public float interactRange = 2f;
    public float placementTime = 3f;
    public KeyCode interactKey = KeyCode.F;
    public LayerMask interactLayers;
}
