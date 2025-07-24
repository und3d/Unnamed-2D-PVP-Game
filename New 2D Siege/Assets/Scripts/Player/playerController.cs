using PurrNet;
using PurrNet.StateMachine;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class playerController : NetworkIdentity
{
    [Header("Settings & Variables")]
    [SerializeField] private PlayerSettings playerSettings;
    public bool isGadgetEquipped;
    
    [Header("References")]
    public StateMachine stateMachine;
    public List<StateNode> weaponStates = new();

    private float moveSpeed;
    private GameController gameController;
    private GameObject currentPreview;
    private Rigidbody2D _rigidbody;
    private StateNode lastState;
    Vector2 moveDirection;
    Vector2 mousePosition;
    bool canMove = true;
    private bool isSprinting;
    private bool lastSprintState;
    private bool lastGadgetState;
    private bool lastWeaponHiddenState;

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
        if (!InstanceHandler.TryGetInstance(out GameController _gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
            return;
        }
        gameController = _gameController;
        lastState = weaponStates[0];
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
    }

    private void FixedUpdate()
    {
        if (!gameController.canMove)
        {
            _rigidbody.linearVelocity = new Vector2(0, 0);
            return;
        }

        if (!playerSettings.toggleSprint)
        {
            if (Input.GetKey(KeyCode.LeftShift) && !isGadgetEquipped)
            {
                moveSpeed = playerSettings.sprintSpeed;
                isSprinting = true;
            }
            else
            {
                moveSpeed = playerSettings.walkSpeed;
                isSprinting = false;
            }
        }
        else
        {
            if (!isGadgetEquipped)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    // Swaps between sprint and walk. If sprinting, set speed to sprint, opposite if walking
                    isSprinting = !isSprinting;
                    moveSpeed = isSprinting ? playerSettings.sprintSpeed : playerSettings.walkSpeed;
                }
            }
            else
            {
                moveSpeed = playerSettings.walkSpeed;
                isSprinting = false;
            }
        }
        
        ToggleWeaponEquipped();
        
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
        if (!gameController.canMove)
            return;
        
        HandleWeaponSwitching();
    }

    private void HandleWeaponSwitching()
    {
        if (isGadgetEquipped || isSprinting) return;
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            stateMachine.SetState(weaponStates[0]);
            lastState = weaponStates[0];
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            stateMachine.SetState(weaponStates[1]);
            lastState = weaponStates[1];
        }
    }

    private void ToggleWeaponEquipped()
    {
        var shouldHideWeapon = isSprinting || isGadgetEquipped;

        if (lastWeaponHiddenState == shouldHideWeapon) 
            return;
        //                                           True            false
        stateMachine.SetState(shouldHideWeapon ? weaponStates[2] : lastState);
        
        lastWeaponHiddenState = shouldHideWeapon;
    }

    private void RotateTowardsMouse()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorldPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _rigidbody.rotation = angle - 90f;
    }
}
