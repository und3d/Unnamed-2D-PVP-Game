using System;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceGadgetPreview : MonoBehaviour
{
    [Header("Placeable Settings")]
    [SerializeField] private float placementTime = 2f;
    [SerializeField] private float maxPlacementDistance = 3f;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;
    
    [Header("References")]
    [SerializeField] private GameObject gadgetPrefab;
    private ProgressBarController progressBar;
    [SerializeField] private Transform spawnOffset;
    
    [Header("Optional")]
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private GameController.Team ownerTeam;
    
    private GadgetController owner;
    private Transform playerTransform;
    private PlayerID ownerID;
    private SpriteRenderer sr;
    private bool isPlacing = false;
    private bool canPlace = false;
    private InputAction interactKey;
    
    public void Initialize(GadgetController manager, Transform player, PlayerID id)
    {
        owner = manager;
        playerTransform = player;
        ownerID = id;
        sr = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }

        interactKey = InputManager.PlayerKeybinds.Get("Player/Interact");
        
        progressBar = gameController.progressBar;
    }

    private void Update()
    {
        if (!playerTransform)
            return;
        
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerToMouse = mouseWorld - playerTransform.position;
        if (playerToMouse.magnitude > maxPlacementDistance)
            playerToMouse = playerToMouse.normalized * maxPlacementDistance;

        Vector2 targetPos = (Vector2)playerTransform.position + playerToMouse;
        
        if (!isPlacing)
        {
            transform.position = targetPos;
            canPlace = IsValidPlacement(targetPos);
        }

        sr.color = canPlace ? validColor : invalidColor;

        if (interactKey.IsPressed() && canPlace)
        {
            isPlacing = true;
            progressBar.BeginInteraction(new InteractionRequest
            {
                duration = placementTime,
                key = interactKey,
                canStart = () => IsValidPlacement(transform.position),
                onComplete = PlaceGadget
            });
        }
        else if (!interactKey.IsPressed())
            isPlacing = false;
    }

    private bool IsValidPlacement(Vector2 pos)
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        
        Vector2 center = pos + col.offset;
        float radius = col.radius * transform.lossyScale.x;
        
        Collider2D hit = Physics2D.OverlapCircle(center, radius, LayerMask.GetMask("Map Objects", "PlayerSelf", "PlayerOther", "Gadgets"));

        return !hit;
    }

    private void PlaceGadget()
    {
        var gadget = Instantiate(gadgetPrefab, transform.position, Quaternion.identity);
        
        if (gadget.TryGetComponent<PlaceableGadget>(out var gadgetScript))
            gadgetScript.Initialize(owner.gameObject, ownerID);
        
        owner.OnGadgetPlaced();
        Destroy(gameObject);
    }

    private void PlayPlacementFeedback()
    {
        if (placeSound)
        {
            AudioSource.PlayClipAtPoint(placeSound, transform.position);
        }
    }

    public string GetGadgetName()
    {
        var gadgetName = gadgetPrefab.GetComponent<PlaceableGadget>().GetGadgetName();
        return gadgetName;
    }
}