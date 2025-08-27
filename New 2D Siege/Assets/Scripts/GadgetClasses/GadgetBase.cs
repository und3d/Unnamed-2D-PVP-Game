using System;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class GadgetBase : NetworkIdentity
{
    [Header("Settings")]
    [SerializeField] protected string gadgetName;
    [SerializeField] protected float pickupHoldTime = 2f;
    [SerializeField] protected float playerPickupDistance = 2f;
    [SerializeField] protected float cursorPickupDistance = 2f;
    [SerializeField] protected float timeBeforePickup = 1f;
    
    [Header("References")]
    [SerializeField] protected Rigidbody2D gadgetRigidbody;
    [SerializeField] protected Collider2D gadgetCollider;
    
    protected ProgressBarController progressBar;
    protected PlayerID ownerID;
    protected GameObject playerObject;
    protected GadgetController gadgetController;
    protected GameController gameController;
    protected InputManager inputManager;
    protected RoundView roundView;
    
    protected float stopThreshold = 0.1f;
    protected float checkDelay = 0.2f;
    protected bool hasStopped;
    protected bool isFrozen;
    protected bool thrownGadgetPickedUpLocal;
    protected InputAction interactKey;
    protected Coroutine _reloadCoroutine;

    protected float timeAtPlacement = 1000000000000f;
    
    // Tool Types
    public enum ToolGadgetType
    {
        None, Gun, Durability, Infinite
    }
    [Header("Tool Types")]
    [SerializeField] protected ToolGadgetType toolGadgetType;
    
    // Toggle Types
    public enum ToggleGadgetType
    {
        None, Timer, Infinite
    }
    [Header("Toggle Types")]
    [SerializeField] protected ToggleGadgetType toggleGadgetType;

    public bool canBePickedUp;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        if (!isOwner)
            return;
        
        if (!InstanceHandler.TryGetInstance(out gameController))
        {
            Debug.LogError($"GadgetBase failed to get gameController!", this);
            return;
        }
        
        progressBar = gameController.progressBar;

        if (!InstanceHandler.TryGetInstance(out inputManager))
        {
            Debug.LogError($"GadgetBase failed to get inputManager!", this);
            return;
        }

        if (!InstanceHandler.TryGetInstance(out roundView))
        {
            Debug.LogError($"GadgetBase failed to get roundView!", this);
            return;
        }
        
        interactKey = inputManager.Get("Player/Interact");

        timeAtPlacement = Time.unscaledTime;
    }

    public virtual void Initialize(GameObject player, PlayerID playerID)
    {
        playerObject = player;
        gadgetController = playerObject.GetComponent<GadgetController>();
        ownerID = playerID;
    }

    public virtual void Initialize(GadgetController _gadgetController)
    {
        gadgetController = _gadgetController;
    }

    protected virtual void Update()
    {
        if (isOwner && !(Time.unscaledTime < timeAtPlacement + timeBeforePickup))
            HandlePickup();
    }

    protected virtual void HandlePickup()
    {
        if (!CanStartPickup())
        {
            gadgetController.gadgetBeingPickedUp = false;
            return;
        }
        
        gadgetController.gadgetBeingPickedUp = true;

        if (!interactKey.IsPressed()) 
            return;
        if (!progressBar)
            return;
        gadgetController.thrownGadgetIsBeingPickedUp = true;
        progressBar.BeginInteraction( new InteractionRequest
        {
            duration = pickupHoldTime,
            key = interactKey,
            canStart = () => true,
            onComplete = PickupGadget,
            onCancel = CancelPickup
        });
    }

    protected virtual bool IsCursorNearGadget()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var distance = Vector2.Distance(transform.position, mouseWorldPos);

        return distance <= cursorPickupDistance;
    }

    protected virtual bool PlayerIsInRange()
    {
        if (!playerObject)
            return false;
        
        var distance = Vector2.Distance(transform.position, playerObject.transform.position);
        
        return distance <= playerPickupDistance;
    }

    protected virtual void PickupGadget()
    {
        gadgetController.OnGadgetPickup();
        roundView.HideGadgetPickupText(this);
        thrownGadgetPickedUpLocal = true;
        gadgetController.timeAtPickup = Time.unscaledTime;
        gadgetController.thrownGadgetIsBeingPickedUp = false;
        gadgetController.gadgetBeingPickedUp = false;
        Destroy(gameObject);
    }

    protected virtual void CancelPickup()
    {
        gadgetController.thrownGadgetIsBeingPickedUp = false;
        gadgetController.gadgetBeingPickedUp = false;
    }

    public virtual void GadgetShot()
    {
        Destroy(gameObject);
    }

    protected virtual void CheckIfStopped()
    {
        if (hasStopped || !isOwner) return;

        if (gadgetRigidbody.linearVelocity.magnitude < stopThreshold)
        {
            FreezeGadget(true);
        }
    }

    protected void FreezeGadget(bool setTrigger)
    {
        gadgetRigidbody.linearVelocity = Vector2.zero;
        gadgetRigidbody.angularVelocity = 0f;
        
        if (setTrigger)
            gadgetCollider.isTrigger = true;
        
        isFrozen = true;
        canBePickedUp = true;
    }

    protected bool CanStartPickup()
    {
        if (PlayerIsInRange() && IsCursorNearGadget() && playerObject)
        {
            roundView.ShowGadgetPickupText(interactKey, this);
            
            return true;
        }

        if (!gadgetController.gadgetBeingPickedUp)
        {
            roundView.HideGadgetPickupText(this);
        }

        return false;
    }

    public string GetGadgetName()
    {
        return gadgetName;
    }

    public ToolGadgetType GetToolGadgetType()
    {
        return toolGadgetType;
    }

    public ToggleGadgetType GetToggleGadgetType()
    {
        return toggleGadgetType;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
