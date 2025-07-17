using System;
using NaughtyAttributes;
using PurrNet;
using UnityEngine;

public class GadgetController : NetworkBehaviour
{
    private enum GadgetType
    {
        Placeable, Throwable, Drone, Tool, Toggle
    }
    private GameObject currentPlacementPreview;
    private GameController gameController;
    
    [Header("Gadget Settings")]
    [SerializeField] private KeyCode gadgetKey = KeyCode.G;
    [SerializeField] private int primaryGadgetCount = 3;
    [SerializeField] private int secondaryGadgetCount = 2;
    
    [Header("Gadget Type")]
    [SerializeField] private GadgetType primaryGadget;
    [SerializeField] private GadgetType secondaryGadget;
    
    
    [Header("Placeable")]
    [SerializeField] private GameObject gadgetPlaceablePreview;
    
    [Header("Throwable")]
    [SerializeField] private GameObject gadgetThrowablePreview;
    
    private void Awake()
    {
        if (!InstanceHandler.TryGetInstance(out gameController))
        {
            Debug.LogError($"GadgetController failed to get gameController!", this);
        }
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
    }

    private void Update()
    {
        switch (primaryGadget)
        {
            case GadgetType.Placeable:
                if (Input.GetKeyDown(gadgetKey) && primaryGadgetCount > 0)
                {
                    if (!currentPlacementPreview)
                    {
                        currentPlacementPreview = Instantiate(gadgetPlaceablePreview);
                        var placer = currentPlacementPreview.GetComponent<PlaceGadgetPreview>();
                        placer.Initialize(this, transform, owner.Value);
                    }
                    else
                    {
                        gameController.progressBar.GetComponent<ProgressBarController>().Cancel(gameController);
                        CancelPlacement();
                    }
                }
                break;
            case GadgetType.Throwable:
                if (Input.GetKeyDown(gadgetKey) && primaryGadgetCount > 0)
                {
                    if (!currentPlacementPreview)
                    {
                        currentPlacementPreview = Instantiate(gadgetThrowablePreview);
                        var thrower = currentPlacementPreview.GetComponent<ThrowGadgetPreview>();
                        thrower.Initialize(this, transform, owner.Value);
                    }
                    else
                    {
                        CancelPlacement();
                    }
                }
                break;
            case GadgetType.Drone:
                Debug.Log("Drone method not implemented");
                break;
            case GadgetType.Tool:
                Debug.Log("Tool method not implemented");
                break;
            case GadgetType.Toggle:
                Debug.Log("Toggle method not implemented");
                break;
        }
    }
    
    public void CancelPlacement()
    {
        if (currentPlacementPreview)
        {
            Destroy(currentPlacementPreview);
            currentPlacementPreview = null;
        }
    }

    public void OnGadgetPlaced()
    {
        currentPlacementPreview = null; // allow future placements
        primaryGadgetCount--;
    }

    public void OnGadgetPickup()
    {
        Debug.Log("Pickup Gadget");
        primaryGadgetCount++;
    }
}
