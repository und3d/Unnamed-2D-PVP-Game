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
    
    protected ProgressBarController progressBar;
    protected bool isBeingPickedUp = false;
    protected PlayerID ownerID;
    protected GameObject playerObject;
    protected GadgetController gadgetController;
    protected GameController gameController;
    protected Rigidbody2D gadgetRigidbody;
    protected Collider2D gadgetCollider;
    protected float stopThreshold = 0.1f;
    protected float checkDelay = 0.2f;
    protected bool hasStopped;
    protected bool isFrozen;
    protected InputAction interactKey;
    protected Coroutine _reloadCoroutine;
    
    // Tool Types
    public enum ToolGadgetType
    {
        None, Gun, Durability, Infinite
    }
    [SerializeField] protected ToolGadgetType toolGadgetType;
    
    // Toggle Types
    public enum ToggleGadgetType
    {
        None, Timer, Infinite
    }
    [SerializeField] protected ToggleGadgetType toggleGadgetType;

    public static bool gadgetBeingPickedUp = false;
    public bool canBePickedUp;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        if (!isOwner)
            return;
        
        progressBar = gameController.progressBar;
    }

    protected virtual void Awake()
    {
        if (!InstanceHandler.TryGetInstance(out gameController))
        {
            Debug.LogError($"GadgetBase failed to get gameController!", this);
        }
        
        if (!TryGetComponent(out gadgetRigidbody))
        {
            Debug.Log("GadgetBase: gadgetRigidbody is null");
        }

        if (!TryGetComponent(out gadgetCollider))
        {
            Debug.Log("GadgetBase: gadgetCollider is null");
        }
        
        if (!isOwner)
            return;
        
        interactKey = InputManager.PlayerKeybinds.Get("Player/Interact");
    }

    public virtual void Initialize(GameObject player, PlayerID playerID)
    {
        playerObject = player;
        gadgetController = playerObject.GetComponent<GadgetController>();
        ownerID = playerID;
    }

    public virtual void Initialize(GadgetController _gadgetController)
    {
        this.gadgetController = _gadgetController;
    }

    protected virtual void Update()
    {
        if (isOwner)
            HandlePickup();
    }

    protected virtual void HandlePickup()
    {
        if (!CanStartPickup())
        {
            gadgetBeingPickedUp = false;
            return;
        }
        
        gadgetBeingPickedUp = true;

        if (!interactKey.IsPressed()) return;
        if (!progressBar)
            return;
        progressBar.BeginInteraction( new InteractionRequest
        {
            duration = pickupHoldTime,
            key = interactKey,
            canStart = () => true,
            onComplete = PickupGadget
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
        Destroy(gameObject);
    }

    public virtual void GadgetShot()
    {
        Destroy(gameObject);
    }

    protected void CheckIfStopped()
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
        return PlayerIsInRange() && IsCursorNearGadget() && playerObject;
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
