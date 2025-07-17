using PurrNet;
using UnityEngine;

public class ThrowGadgetPreview : MonoBehaviour
{
    [Header("Throwing")]
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private KeyCode throwKey = KeyCode.F;

    [Header("References")]
    [SerializeField] private GameObject gadgetPrefab;
    [SerializeField] private Transform throwOrigin;
    
    private GadgetController owner;
    private Transform playerTransform;
    private GameObject player;
    private PlayerID ownerID;
    private bool isAiming = true;

    public void Initialize(GadgetController controller, Transform _playerTransform, PlayerID id)
    {
        owner = controller;
        playerTransform = _playerTransform;
        player = playerTransform.gameObject;
        ownerID = id;
        
        transform.SetParent(playerTransform);
        throwOrigin = player.GetComponentInChildren<ThrowOrigin>().throwOrigin;
        transform.localPosition = new Vector3(-0.5105f,0.255f,0);
        transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (!isAiming) return;

        if (Input.GetKeyDown(throwKey))
        {
            Throw();
        }
    }

    private void Throw()
    {
        isAiming = false;

        Vector2 throwDir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - throwOrigin.position).normalized;

        GameObject gadget = Instantiate(gadgetPrefab, throwOrigin.position, Quaternion.identity);

        if (gadget.TryGetComponent<ThrowableGadget>(out var script))
        {
            script.Initialize(owner.gameObject, ownerID);
            script.Throw(throwDir, throwForce);
        }

        owner.OnGadgetPlaced(); // use same handler for consistency
        Destroy(gameObject); // destroy preview
    }
}
