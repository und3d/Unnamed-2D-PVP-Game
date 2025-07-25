using System;
using PurrNet;
using UnityEngine;

public class GadgetController : NetworkBehaviour
{
    public enum GadgetType
    {
        Placeable, Throwable, Drone, Tool, Toggle
    }
    
    private GameObject equippedGadget;
    private GameController gameController;
    [SerializeField] private playerController playerController;
    
    [Header("Gadget Settings")]
    [SerializeField] private KeyCode gadgetKey = KeyCode.G;
    [SerializeField] private KeyCode useGadgetKey = KeyCode.F;
    [SerializeField] private int primaryGadgetCount = 3;
    [SerializeField] private bool hasDurability = false;
    [SerializeField] private bool hasDuration = false;
    [SerializeField] private bool canRecharge = true;
    [SerializeField] private int toolDurability = 15;
    [SerializeField] private float primaryGadgetDuration = 30f;     // How long can a tool or toggle be used (If timer needed)
    [SerializeField] private float primaryGadgetDelay = 2f;         // How long until tool or toggle can be enabled/pulled-out after being disabled/put-away
    [SerializeField] private int secondaryGadgetCount = 2;
    
    [Header("Gadget Type")]
    [SerializeField] private GadgetType primaryGadget;
    [SerializeField] private GadgetType secondaryGadget;
    
    [Header("Placeable")]
    [SerializeField] private GameObject gadgetPlaceablePreview;

    [Header("Throwable")]
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private float throwForce = 10f;
    
    [Header("Drone")]
    
    [Header("Tool")]
    [SerializeField] private GameObject gadgetTool;
    
    [Header("Toggle")]
    [SerializeField] private GameObject gadgetToggle;

    [SerializeField] private GameObject gadgetVisual;
    [SerializeField] private GameObject gadgetPrefab;

    private bool isGadgetPulledOut = false;
    
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
        if (gameController.justThrewGadget)
        {
            gameController.justThrewGadget = false;
            gameController.canActivateThrownGadget = true;
        }
        
        switch (primaryGadget)
        {
            case GadgetType.Placeable:
                if (Input.GetKeyDown(gadgetKey) && primaryGadgetCount > 0)
                {
                    if (!isGadgetPulledOut)
                    {
                        equippedGadget = Instantiate(gadgetPlaceablePreview);
                        var placer = equippedGadget.GetComponent<PlaceGadgetPreview>();
                        placer.Initialize(this, transform, owner.Value);
                        ToggleGadgetVisual();
                    }
                    else
                    {
                        gameController.progressBar.GetComponent<ProgressBarController>().Cancel(gameController);
                        PutAwayGadget();
                    }
                }
                break;
            case GadgetType.Throwable:
                if (Input.GetKeyDown(gadgetKey) && primaryGadgetCount > 0)
                {
                    if (!isGadgetPulledOut)
                    {
                        gameController.isGadgetEquipped = true;
                        ToggleGadgetVisual();
                    }
                    else
                    {
                        gameController.isGadgetEquipped = false;
                        PutAwayGadget();
                    }
                }

                if (Input.GetKeyDown(useGadgetKey) && isGadgetPulledOut)
                {
                    Throw();
                }
                break;
            case GadgetType.Drone:
                if (Input.GetKeyDown(gadgetKey) && primaryGadgetCount > 0)
                {
                    if (!isGadgetPulledOut)
                    {
                        ToggleGadgetVisual();
                        
                    }
                    else
                    {
                        PutAwayGadget();
                    }
                }
                
                if (Input.GetKeyDown(useGadgetKey) && isGadgetPulledOut)
                {
                    ThrowDrone();
                }
                break;
            case GadgetType.Tool:
                if (Input.GetKeyDown(gadgetKey))
                {
                    if (!isGadgetPulledOut)
                    {
                        ToggleGadgetVisual();
                        
                    }
                    else
                    {
                        PutAwayGadget();
                    }
                }
                break;
            case GadgetType.Toggle:
                Debug.Log("Toggle method not implemented");
                break;
        }
    }

    private void PutAwayGadget()
    {
        if (gadgetVisual) ToggleGadgetVisual();
        if (equippedGadget) Destroy(equippedGadget);
        equippedGadget = null;
    }

    public void OnGadgetPlaced()
    {
        equippedGadget = null; // allow future placements
        primaryGadgetCount--;
    }

    public void OnGadgetPickup()
    {
        Debug.Log("Pickup Gadget");
        primaryGadgetCount++;
    }

    private void Throw()
    {
        Vector2 throwDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - throwOrigin.position).normalized;
        GameObject gadget = Instantiate(gadgetPrefab, throwOrigin.position, transform.rotation);
        
        if (gadget.TryGetComponent<ThrowableGadget>(out var script))
        {
            script.Initialize(gameObject, owner.Value);
            script.Throw(throwDir, throwForce);
        }
        
        gameController.canActivateThrownGadget = false;
        gameController.justThrewGadget = true;
        gameController.isGadgetEquipped = false;
        OnGadgetPlaced();
        ToggleGadgetVisual();
    }

    private void ThrowDrone()
    {
        Vector2 throwDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - throwOrigin.position).normalized;
        GameObject gadget = Instantiate(gadgetPrefab, throwOrigin.position, transform.rotation * Quaternion.Euler(0f, 0f, 90f));
        
        if (gadget.TryGetComponent<DroneGadget>(out var script))
        {
            script.Initialize(gameObject, owner.Value);
            script.Throw(throwDir, throwForce);
        }
        
        OnGadgetPlaced();
        ToggleGadgetVisual();
    }

    private void ToggleGadgetVisual()
    {
        isGadgetPulledOut = !isGadgetPulledOut;
        playerController.isGadgetEquipped = isGadgetPulledOut;
        ToggleGadgetVisualRPC();
    }

    public bool isGadgetEquipped()
    {
        return isGadgetPulledOut;
    }
    
    [ObserversRpc(runLocally:false)]
    private void ToggleGadgetVisualRPC()
    {
        gadgetVisual.SetActive(!gadgetVisual.activeSelf);
    }
}
