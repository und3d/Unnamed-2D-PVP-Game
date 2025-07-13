using System;
using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundView : View
{
    [SerializeField] private TMP_Text healthText;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this); //A safer singleton; needs a OnDestroy
    }
    
    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<RoundView>();
    }
    
    public override void OnShow()
    {
        
    }

    public override void OnHide()
    {
        
    }

    public void UpdateHealth(int health)
    {
        healthText.text = health.ToString();
    }
}
