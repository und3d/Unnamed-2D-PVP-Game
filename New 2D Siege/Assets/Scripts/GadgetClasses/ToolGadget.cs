using System;
using UnityEngine;

public abstract class ToolGadget : GadgetBase
{
    [Header("Gadget Settings")]
    [SerializeField] private bool hasActivator;
    [SerializeField] private KeyCode useGadgetKey = KeyCode.F;
    [SerializeField] private Vector3 gadgetPosition;

    private GadgetController ownerGadgetController;
    private Transform playerTransform;
    private GameObject player;
    
    protected override void Update()
    {
        //Gadget is not placed, no pickup needed
        
        // If the tool is active by default, no use for keybind
        if (!hasActivator)
            return;
        
        if (Input.GetKeyDown(useGadgetKey))
        {
            Debug.Log("Used The Gadget");
        }
    }

    public override void GadgetShot()
    {
        // Gadget is a handheld, cannot be shot
    }
}
