using PurrNet;
using PurrNet.StateMachine;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class playerController : NetworkIdentity
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintSpeed = 8f;

    [Header("References")]
    [SerializeField] private StateMachine stateMachine;
    [SerializeField] private List<StateNode> weaponStates = new();
    //[SerializeField] private GameObject gadgetPreviewPrefab;
    
    [Header("Keybinds")]
    //[SerializeField] private KeyCode gadgetKey = KeyCode.G;

    private GameObject currentPreview;
    private Rigidbody2D _rigidbody;
    Vector2 moveDirection;
    Vector2 mousePosition;
    bool canMove = true;

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
    }

    private void FixedUpdate()
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }

        if (!gameController.canMove)
        {
            _rigidbody.linearVelocity = new Vector2(0, 0);
            return;
        }
        
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        if (input.magnitude > 1)
            input.Normalize();
        
        var movement = input * moveSpeed;
        _rigidbody.linearVelocity = movement;
        
        if (Application.isFocused)
            RotateTowardsMouse();
    }

    private void Update()
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }
        /*
        if (Input.GetKeyDown(gadgetKey))
        {
            if (!currentPreview)
            {
                currentPreview = Instantiate(gadgetPreviewPrefab);
                var placer = currentPreview.GetComponent<PlaceGadgetPreview>();
                placer.Initialize(this, transform, owner.Value);
            }
            else
            {
                gameController.progressBar.GetComponent<ProgressBarController>().Cancel(gameController);
                CancelPlacement();
            }
        }
        */
        if (!gameController.canMove)
            return;
        
        HandleWeaponSwitching();
    }

    private void HandleWeaponSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            stateMachine.SetState(weaponStates[0]);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            stateMachine.SetState(weaponStates[1]);
    }

    private void RotateTowardsMouse()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorldPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _rigidbody.rotation = angle - 90f;
    }
    /*
    public void CancelPlacement()
    {
        if (currentPreview)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    public void OnGadgetPlaced()
    {
        currentPreview = null; // allow future placements
    }
    */
}
