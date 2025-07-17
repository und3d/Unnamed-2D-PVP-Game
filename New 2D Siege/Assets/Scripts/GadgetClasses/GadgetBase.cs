using PurrNet;
using UnityEngine;

public class GadgetBase : NetworkIdentity
{
    [Header("Settings")]
    [SerializeField] protected float pickupHoldTime = 2f;
    [SerializeField] protected KeyCode pickUpKey = KeyCode.F;
    [SerializeField] protected float playerPickupDistance = 1f;
    [SerializeField] protected float cursorPickupDistance = 1f;
    
    protected ProgressBarController progressBar;
    protected bool isBeingPickedUp = false;
    protected PlayerID ownerID;
    protected GameObject playerObject;
    protected GadgetController gadgetController;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }
        
        progressBar = gameController.progressBar;
    }

    public virtual void Initialize(GameObject player, PlayerID playerID)
    {
        playerObject = player;
        gadgetController = playerObject.GetComponent<GadgetController>();
        ownerID = playerID;
    }

    protected virtual void Update()
    {
        if (isOwner)
            HandlePickup();
    }

    protected virtual void HandlePickup()
    {
        if (!playerObject)
            return;
        if (!PlayerIsInRange())
            return;
        if (!IsCursorNearGadget())
            return;
        if (Input.GetKey(pickUpKey))
        {
            if (!progressBar)
                return;
            progressBar.BeginInteraction( new InteractionRequest
            {
                duration = pickupHoldTime,
                key = pickUpKey,
                canStart = () => true,
                onComplete = PickupGadget
            });
        }
    }

    protected virtual bool IsCursorNearGadget()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector2.Distance(transform.position, mouseWorldPos);

        return distance <= cursorPickupDistance;
    }

    protected virtual bool PlayerIsInRange()
    {
        float distance = Vector2.Distance(transform.position, playerObject.transform.position);
        
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
