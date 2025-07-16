using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class CharacterSelectView : View
{
    [SerializeField] private List<CanvasGroup> childViews;
    
    public override void OnShow()
    {
        
    }

    public override void OnHide()
    {
        foreach (var view in childViews)
            view.alpha = 0;
    }
    
}
