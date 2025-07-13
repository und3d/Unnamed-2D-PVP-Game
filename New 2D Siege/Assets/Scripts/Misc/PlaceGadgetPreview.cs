using System;
using PurrNet;
using UnityEngine;

public class PlaceGadgetPreview : MonoBehaviour
{
    [SerializeField] private GameObject finalGadgetPrefab;
    [SerializeField] private ProgressBarController progressBar;
    [SerializeField] private KeyCode placeKey = KeyCode.G;
    [SerializeField] private float placementDuration = 1.5f;
    [SerializeField] private float maxPlacementDistance = 3f;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    private playerController owner;
    private Transform playerTransform;
    private PlayerID ownerID;
    private SpriteRenderer sr;
    private bool isPlacing = false;
    private bool canPlace = false;
    
    public void Initialize(playerController manager, Transform player, PlayerID id)
    {
        owner = manager;
        playerTransform = player;
        sr = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }
        
        progressBar = gameController.progressBar;
    }

    void Update()
    {
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

        if (Input.GetKey(placeKey) && canPlace)
        {
            isPlacing = true;
            progressBar.BeginInteraction(new InteractionRequest
            {
                duration = placementDuration,
                key = placeKey,
                canStart = () => IsValidPlacement(transform.position),
                onComplete = PlaceGadget
            });
        }
        else if (!Input.GetKey(placeKey))
            isPlacing = false;
    }

    bool IsValidPlacement(Vector2 pos)
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        
        Vector2 center = pos + col.offset;
        float radius = col.radius * transform.lossyScale.x;
        
        Collider2D hit = Physics2D.OverlapCircle(center, radius, LayerMask.GetMask("Map Objects", "PlayerSelf", "PlayerOther"));

        return !hit;
    }

    void PlaceGadget()
    {
        var gadget = Instantiate(finalGadgetPrefab, transform.position, Quaternion.identity);
        owner.OnGadgetPlaced();
        Destroy(gameObject);
    }
}