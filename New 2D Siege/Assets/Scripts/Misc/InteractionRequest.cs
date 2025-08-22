using System;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionRequest
{
    //public PlayerID player;
    public float duration;
    public InputAction key;
    public Func<bool> canStart;
    public Action onStart;
    public Action onComplete;
    public Action onCancel;
}
