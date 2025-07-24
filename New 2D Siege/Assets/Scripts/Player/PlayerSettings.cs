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
}
