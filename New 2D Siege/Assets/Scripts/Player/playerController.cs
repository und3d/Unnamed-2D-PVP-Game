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
    private InputManager inputManager;
    private GameObject currentPreview;
    private GameObject barricadeObj;
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
    private GameController.Side side;
    private float timeAtBarricadeInteraction;
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
        
        if (!InstanceHandler.TryGetInstance(out inputManager))
        {
            Debug.LogError($"PlayerController failed to get inputManager!", this);
            return;
        }
        
        SetKeybindReferences();
        
        TryGetComponent(out _rigidbody);
        if (!InstanceHandler.TryGetInstance(out GameController _gameController))
        {
            Debug.LogError($"PlayerController failed to get gameController!", this);
            return;
        }
        
        gameController = _gameController;
        progressBar = gameController.progressBar;
        lastState = weaponStates[0];
        
        GetPlayerSide();
        
        var visionObj = Instantiate(playerVision, transform, false);
        
        //Debug.Log($"Checking if {owner.Value}'s side is Defense");
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
        if (Time.unscaledTime < timeAtBarricadeInteraction + playerSettings.timeBetweenBarricadeInteractions)
            return;
        
        barricadeObj = GetObjectUnderCursor();

        if (!barricadeObj || !barricadeObj.GetComponent<Barricade>())
        {
            barricadeDetected = false;
            return;
        }
        
        barricadeDetected = true;
        var sqrDistance = (barricadeObj.transform.position - transform.position).sqrMagnitude;
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
                    onComplete = ToggleBarricade
                });
            }
        }
        else
        {
            barricadeDetected = false;
        }
    }

    private void ToggleBarricade()
    {
        barricadeObj.GetComponent<Barricade>().ToggleBarricade();
        timeAtBarricadeInteraction = Time.unscaledTime;
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
    
    private void GetPlayerSide()
    {
        //Debug.Log($"Get Player {owner.Value}'s Side.");
        gameController.GetPlayerSide(owner.Value, this);
    }

    public void SetPlayerSide(GameController.Side _side)
    {
        //Debug.Log($"Player {owner.Value}'s Side is {_side}.");
        side = _side;
        
        if (side == GameController.Side.Defense)
        {
            isDefender = true;
        }
    }

    private void SetKeybindReferences()
    {
        _aim = inputManager.Get("Player/Aim");
        _shoot = inputManager.Get("Player/Shoot");
        _sprint = inputManager.Get("Player/Sprint");
        _reload = inputManager.Get("Player/Reload");
        _interact = inputManager.Get("Player/Interact");
        _primaryWeapon = inputManager.Get("Player/Primary Weapon");
        _secondaryWeapon = inputManager.Get("Player/Secondary Weapon");
        _primaryGadget = inputManager.Get("Player/Primary Gadget");
        _secondaryGadget = inputManager.Get("Player/Secondary Gadget");

    }
}
