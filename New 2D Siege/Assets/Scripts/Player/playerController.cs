using PurrNet;
using PurrNet.StateMachine;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class playerController : NetworkIdentity
{
    [Header("Settings & Variables")]
    [SerializeField] private PlayerSettings playerSettings;
    public bool isGadgetEquipped;
    
    [Header("References")]
    public StateMachine stateMachine;
    public List<StateNode> weaponStates = new();
    [SerializeField] private GameObject playerVision;

    #region Private Variables
    private float moveSpeed;
    private GameController gameController;
    private GameObject currentPreview;
    private ProgressBarController progressBar;
    private Rigidbody2D _rigidbody;
    private StateNode lastState;
    Vector2 moveDirection;
    Vector2 mousePosition;
    private bool isSprinting;
    private bool lastSprintState;
    private bool lastGadgetState;
    private bool lastWeaponHiddenState;
    private bool isPlacing;
    private bool isDefender;
    #endregion
    
    #region Keybinds
    private InputAction _shoot;
    private InputAction _aim;
    private InputAction _sprint;
    private InputAction _reload;
    private InputAction _interact;
    private InputAction _primaryWeapon;
    private InputAction _secondaryWeapon;
    private InputAction _primaryGadget;
    private InputAction _secondaryGadget;
    
    #endregion

    public bool barricadeDetected;
    public bool barricadeInRange;
    public bool destructibleWallDetected;
    public bool destructibleWallInRange;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;

        if (!isOwner) 
            return;
        
        SetKeybindReferences();
        
        TryGetComponent(out _rigidbody);
        if (!InstanceHandler.TryGetInstance(out GameController _gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
            return;
        }
        gameController = _gameController;
        progressBar = gameController.progressBar;
        lastState = weaponStates[0];
        
        var visionObj = Instantiate(playerVision, transform, false);
        if (GetPlayerSide() == GameController.Side.Defense)
        {
            isDefender = true;
        }
    }

    private void FixedUpdate()
    {
        if (!_rigidbody)
            return;
        
        if (gameController && !gameController.canMove)
        {
            _rigidbody.linearVelocity = new Vector2(0, 0);
            return;
        }

        if (!playerSettings.toggleSprint)
        {
            if (_sprint != null && _sprint.IsPressed() && !isGadgetEquipped)
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
                if (_sprint != null && _sprint.WasPressedThisFrame() && !isGadgetEquipped)
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
        if (gameController && !gameController.canMove)
            return;
        
        if (isDefender)
        {
            BarricadeInteraction();
            DestructibleWallInteraction();
        }
        
        HandleWeaponSwitching();
    }

    private void HandleWeaponSwitching()
    {
        if (isGadgetEquipped || isSprinting) return;
        
        if (_primaryWeapon != null && _primaryWeapon.WasPressedThisFrame())
        {
            stateMachine.SetState(weaponStates[0]);
            lastState = weaponStates[0];
        }
        if (_secondaryWeapon != null && _secondaryWeapon.WasPressedThisFrame())
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

    private void BarricadeInteraction()
    {
        var obj = GetObjectUnderCursor();

        if (!obj || !obj.GetComponent<Barricade>())
        {
            barricadeDetected = false;
            return;
        }
        
        barricadeDetected = true;
        var sqrDistance = (obj.transform.position - transform.position).sqrMagnitude;
        if (sqrDistance <= playerSettings.interactRange * playerSettings.interactRange)
        {
            barricadeInRange = true;
            if (_interact != null && _interact.IsPressed() && !isGadgetEquipped)
            {
                progressBar.BeginInteraction(new InteractionRequest
                {
                    duration = playerSettings.placementTime,
                    key = _interact,
                    canStart = () => true,
                    onComplete = obj.GetComponent<Barricade>().ToggleBarricade
                });
            }
        }
        else
        {
            barricadeDetected = false;
        }
    }

    private void DestructibleWallInteraction()
    {
        var obj = GetObjectUnderCursor();

        if (!obj || !obj.GetComponent<DestructibleWall>() || obj.GetComponent<DestructibleWall>().IsReinforced)
        {
            destructibleWallDetected = false;
            return;
        }
        
        destructibleWallDetected = true;
        var sqrDistance = (obj.transform.position - transform.position).sqrMagnitude;
        if (sqrDistance <= playerSettings.interactRange * playerSettings.interactRange)
        {
            destructibleWallInRange = true;
            if (_interact != null && _interact.IsPressed() && !isGadgetEquipped)
            {
                progressBar.BeginInteraction(new InteractionRequest
                {
                    duration = playerSettings.placementTime,
                    key = _interact,
                    canStart = () => true,
                    onComplete = obj.GetComponent<DestructibleWall>().ReinforceWall
                });
            }
        }
        else
        {
            destructibleWallDetected = false;
        }
    }

    private GameObject GetObjectUnderCursor()
    {
        Vector2 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(cursorWorldPosition, Vector2.zero, 0f, playerSettings.interactLayers);
        
        if (!hit)
        {
            return null;
        }
        if (hit.collider.gameObject.GetComponent<Barricade>() || hit.collider.gameObject.GetComponent<DestructibleWall>())
        {
            return hit.collider.gameObject;
        }

        return null;
    }

    private void RotateTowardsMouse()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorldPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _rigidbody.rotation = angle - 90f;
    }

    private GameController.Side GetPlayerSide()
    {
        Debug.Log($"Side: {gameController.GetPlayerSide(owner.Value)}");
        return gameController.GetPlayerSide(owner.Value);
    }

    private void SetKeybindReferences()
    {
        var keybinds = playerSettings.playerInput;

        _aim = InputManager.PlayerKeybinds.Get("Player/Aim");
        _shoot = InputManager.PlayerKeybinds.Get("Player/Shoot");
        _sprint = InputManager.PlayerKeybinds.Get("Player/Sprint");
        _reload = InputManager.PlayerKeybinds.Get("Player/Reload");
        _interact = InputManager.PlayerKeybinds.Get("Player/Interact");
        _primaryWeapon = InputManager.PlayerKeybinds.Get("Player/Primary Weapon");
        _secondaryWeapon = InputManager.PlayerKeybinds.Get("Player/Secondary Weapon");
        _primaryGadget = InputManager.PlayerKeybinds.Get("Player/Primary Gadget");
        _secondaryGadget = InputManager.PlayerKeybinds.Get("Player/Secondary Gadget");

    }
}
