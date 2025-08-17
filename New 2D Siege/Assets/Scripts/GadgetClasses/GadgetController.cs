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
    private RoundView roundView;
    private string gadgetName;
    private ToggleGadget toggleGadgetScript;
    private ToolGadget toolGadgetScript;
    private float _lastToggleTime;
    
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
        
        if (!InstanceHandler.TryGetInstance(out roundView))
        {
            Debug.LogError($"GadgetController failed to get RoundView!", this);
        }

        if (!isOwner)
            return;

        gadgetName = primaryGadget switch
        {
            GadgetType.Placeable => gadgetPlaceablePreview.GetComponent<PlaceGadgetPreview>().GetGadgetName(),
            GadgetType.Throwable => gadgetPrefab.GetComponent<ThrowableGadget>().GetGadgetName(),
            GadgetType.Drone => gadgetPrefab.GetComponent<DroneGadget>().GetGadgetName(),
            GadgetType.Tool => gadgetTool.GetComponent<ToolGadget>().GetGadgetName(),
            GadgetType.Toggle => gadgetToggle.GetComponent<ToggleGadget>().GetGadgetName(),
            _ => "None"
        };

        switch (primaryGadget)
        {
            case GadgetType.Toggle:
                toggleGadgetScript = gadgetToggle.GetComponent<ToggleGadget>();
                toggleGadgetScript.Initialize(this);
                break;
            case GadgetType.Tool:
                toolGadgetScript = gadgetTool.GetComponent<ToolGadget>();
                break;
            case GadgetType.Placeable:
            case GadgetType.Throwable:
            case GadgetType.Drone:
                // Do nothing for other gadget types
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        roundView.UpdateGadgetPrimaryText(gadgetName);
        roundView.UpdateGadgetPrimaryCount(primaryGadgetCount);
        roundView.UpdateGadgetSecondaryCount(secondaryGadgetCount);
        roundView.UpdateGadgetSecondaryText("None");
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
                    ToolGadgetActivationToggle();
                }
                break;
            case GadgetType.Toggle:
                if (Input.GetKeyDown(gadgetKey))
                {
                    // If gadget is active when key is pressed, disable gadget
                    if (toggleGadgetScript.gadgetIsEnabled)
                    {
                        ToggleGadgetActivationToggle();
                    }
                    // If gadget is disabled and conditions are met, enable gadget
                    else if (_lastToggleTime + toggleGadgetScript.GetTimeBetweenActivations() < Time.unscaledTime && toggleGadgetScript.CanActivateToggleGadget())
                    {
                        ToggleGadgetActivationToggle();
                    }
                    else
                    {
                        Debug.LogWarning("Toggle Gadget is not active and cannot activate yet!");
                    }
                }
                break;
        }
    }
    
    // TOGGLE GADGET ACTIVATION

    public void ToggleGadgetActivationToggle()
    {
        if (toggleGadgetScript.gadgetIsEnabled)
            _lastToggleTime = Time.unscaledTime;
        
        toggleGadgetScript.gadgetIsEnabled = !toggleGadgetScript.gadgetIsEnabled;
        toggleGadgetScript.ToggleGadgetVisuals();
        isGadgetPulledOut = !isGadgetPulledOut;
        playerController.isGadgetEquipped = isGadgetPulledOut;
    }
    
    // TOOL GADGET ACTIVATION
    private void ToolGadgetActivationToggle()
    {
        toolGadgetScript.gadgetIsEnabled = !toolGadgetScript.gadgetIsEnabled;
        toolGadgetScript.ToggleGadgetVisuals();
        isGadgetPulledOut = !isGadgetPulledOut;
        playerController.isGadgetEquipped = isGadgetPulledOut;
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
        roundView.UpdateGadgetPrimaryCount(primaryGadgetCount);
    }

    public void OnGadgetPickup()
    {
        Debug.Log("Pickup Gadget");
        primaryGadgetCount++;
        roundView.UpdateGadgetPrimaryCount(primaryGadgetCount);
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
        
        if (primaryGadget == GadgetType.Tool)
            gadgetTool.GetComponent<ToolGadget>().CancelReload();
        
        ToggleGadgetVisualRPC();
    }

    public bool isGadgetEquipped()
    {
        return isGadgetPulledOut;
    }

    public int GetGadgetCountPrimary()
    {
        return primaryGadgetCount;
    }
    
    [ObserversRpc(runLocally:false)]
    private void ToggleGadgetVisualRPC()
    {
        gadgetVisual.SetActive(!gadgetVisual.activeSelf);
    }
}
