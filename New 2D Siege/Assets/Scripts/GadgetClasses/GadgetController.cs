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
    public int primaryGadgetCount = 3;
    public int secondaryGadgetCount = 2;
    
    [Header("Gadget Type")]
    [SerializeField] private GadgetType primaryGadget;
    [SerializeField] private GadgetType secondaryGadget;
    
    
    [Header("Placeable")]
    [SerializeField] private GameObject gadgetPlaceablePreview;
    
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
                if (Input.GetKeyDown(gadgetKey))
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
                Debug.Log("Throwable method not implemented");
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
    }
}
