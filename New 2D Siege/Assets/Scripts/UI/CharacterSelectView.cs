using System;
using System.Collections.Generic;
using PurrNet;
using TMPro;
using UnityEngine;

public class CharacterSelectView : View
{
    [SerializeField] private List<CanvasGroup> childViews;
    [SerializeField] private TMP_Text selectionTimer;
    
    private int minutes, seconds, centiSeconds;
    
    public override void OnShow()
    {
        
    }

    public override void OnHide()
    {
        foreach (var view in childViews)
            view.alpha = 0;
    }

    public void UpdateSelectionTimer(float timeLeft)
    {
        minutes = Mathf.FloorToInt(timeLeft / 60);
        seconds = Mathf.FloorToInt(timeLeft % 60);
        
        selectionTimer.text = $"{minutes:00}:{seconds:00}";
    }
    
}
