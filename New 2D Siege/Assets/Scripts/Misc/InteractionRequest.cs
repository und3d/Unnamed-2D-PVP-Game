using System;
using PurrNet;
using UnityEngine;

public class InteractionRequest
{
    //public PlayerID player;
    public float duration;
    public KeyCode key;
    public Func<bool> canStart;
    public Action onComplete;
}
