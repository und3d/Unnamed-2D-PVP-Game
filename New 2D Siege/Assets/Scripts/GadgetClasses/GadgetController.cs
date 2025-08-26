using System;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class GadgetController : NetworkBehaviour
{
    public enum GadgetType
    {
        Placeable, Throwable, Drone, Tool, Toggle
    }
    
    private GameObject equippedGadget;
    private GameController gameController;
    [SerializeField] private playerController _playerController;
    
    [Header("Gadget Settings")]
    [SerializeField] private KeyCode gadgetKey = KeyCode.G;
    [SerializeField] private KeyCode useGadgetKey = KeyCode.F;
    [SerializeField] private int primaryGadgetCount = 3;
    [SerializeField] private bool hasDurability;
    [SerializeField] private bool hasDuration;
    [SerializeField] private bool canRecharge = true;
    [SerializeField] private int toolDurability = 15;
    [SerializeField] private float primaryGadgetDuration = 30f;     // How long can a tool or toggle be used (If timer needed)
    [SerializeField] private float primaryGadgetDelay = 2f;         // How long until tool or toggle can be enabled/pulled-out after being disabled/put-away
    [SerializeField] private int secondaryGadgetCount = 2;
    public bool gadgetBeingPickedUp;
    
    [Header("Gadget Type")]
    [SerializeField] private GadgetType primaryGadget;
    [SerializeField] private GadgetType secondaryGadget;
    
    [Header("Placeable")]
    [SerializeField] private GameObject gadgetPlaceablePreview;

    [Header("Throwable")]
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private float throwForce = 10f;
    public float timeBeforeActivation;
    public float timeAtPickup;
    public bool thrownGadgetIsBeingPickedUp;
    
    [Header("Drone Gadget")]
    
    [Header("Tool")]
    [SerializeField] private GameObject gadgetTool;
    
    [Header("Toggle")]
    [SerializeField] private GameObject gadgetToggle;
    
    [Header("Drones")]
    [SerializeField] private int dronesCount = 2;
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private Transform droneThrowOrigin;
    [SerializeField] private float droneThrowForce;
    
    [Header("General Gadget References")]
    [SerializeField] private GameObject gadgetVisual;
    [SerializeField] private GameObject gadgetPrefab;

    public bool isGadgetPulledOut;
    private RoundView roundView;
    private string gadgetName;
    private ToggleGadget toggleGadgetScript;
    private ToolGadget toolGadgetScript;
    private float _lastToggleTime;
    private InputAction throwDroneKeybind;
    
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
        
        Invoke(nameof(SetLateVariables), 0.01f);

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
        #region Regular Drones
            // Regular Drone Throwing
            if (dronePrefab && !_playerController.IsDefender() && dronesCount > 0)
            {
                if (throwDroneKeybind == null)
                    return;
                if (throwDroneKeybind.WasPressedThisFrame())
                    ThrowDrone();
            }
        #endregion

        #region Primary Gadgets
            // Primary Gadgets
            if (gameController.justThrewGadget)
            {
                gameController.justThrewGadget = false;
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
                            if (owner != null) placer.Initialize(this, transform, owner.Value);
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
                        ThrowDroneGadget();
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
        #endregion
        
    }
    
    // TOGGLE GADGET ACTIVATION

    public void ToggleGadgetActivationToggle()
    {
        if (toggleGadgetScript.gadgetIsEnabled)
            _lastToggleTime = Time.unscaledTime;
        
        toggleGadgetScript.gadgetIsEnabled = !toggleGadgetScript.gadgetIsEnabled;
        toggleGadgetScript.ToggleGadgetVisuals();
        isGadgetPulledOut = !isGadgetPulledOut;
        _playerController.isGadgetEquipped = isGadgetPulledOut;
    }
    
    // TOOL GADGET ACTIVATION
    private void ToolGadgetActivationToggle()
    {
        toolGadgetScript.gadgetIsEnabled = !toolGadgetScript.gadgetIsEnabled;
        toolGadgetScript.ToggleGadgetVisuals();
        isGadgetPulledOut = !isGadgetPulledOut;
        _playerController.isGadgetEquipped = isGadgetPulledOut;
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
        ToggleGadgetVisual();
        roundView.UpdateGadgetPrimaryCount(primaryGadgetCount);
    }

    public void OnGadgetPickup()
    {
        primaryGadgetCount++;
        roundView.UpdateGadgetPrimaryCount(primaryGadgetCount);
    }

    private void Throw()
    {
        if (primaryGadgetCount <= 0)
            return;

        if (Camera.main)
        {
            Vector2 throwDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - throwOrigin.position).normalized;
            GameObject gadget = Instantiate(gadgetPrefab, throwOrigin.position, transform.rotation);
        
            if (gadget.TryGetComponent<ThrowableGadget>(out var script))
            {
                if (owner != null) script.Initialize(gameObject, owner.Value);
                script.Throw(throwDir, throwForce);
            }
        }

        gameController.justThrewGadget = true;
        gameController.isGadgetEquipped = false;
        OnGadgetPlaced();
    }

    private void ThrowDroneGadget()
    {
        if (primaryGadgetCount <= 0)
            return;

        if (Camera.main)
        {
            Vector2 throwDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - throwOrigin.position).normalized;
            var gadget = Instantiate(gadgetPrefab, throwOrigin.position, transform.rotation * Quaternion.Euler(0f, 0f, 90f));
        
            if (gadget.TryGetComponent<DroneGadget>(out var script))
            {
                if (owner != null) script.Initialize(gameObject, owner.Value);
                script.Throw(throwDir, throwForce);
            }
        }

        gameController.isGadgetEquipped = false;
        OnGadgetPlaced();
    }
    
    private void ThrowDrone()
    {
        if (!Camera.main) return;
        
        Vector2 throwDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - droneThrowOrigin.position).normalized;
        var droneObject = Instantiate(dronePrefab, droneThrowOrigin.position, transform.rotation); //* Quaternion.Euler(0f, 0f, 90f));
        
        if (!droneObject.TryGetComponent<DroneController>(out var script))
            return;
        
        if (owner != null) script.Initialize(gameObject, owner.Value);
        script.Throw(throwDir, droneThrowForce);

        dronesCount--;
    }

    private void ToggleGadgetVisual()
    {
        isGadgetPulledOut = !isGadgetPulledOut;
        _playerController.isGadgetEquipped = isGadgetPulledOut;
        
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

    private void SetLateVariables()
    {
        throwDroneKeybind = _playerController.GetInputAction("Throw Drone");
    }
    
    [ObserversRpc(runLocally:false)]
    private void ToggleGadgetVisualRPC()
    {
        gadgetVisual.SetActive(!gadgetVisual.activeSelf);
    }
}
