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
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private StateMachine stateMachine;
    [SerializeField] private List<StateNode> weaponStates = new();


    private Rigidbody2D _rigidbody;
    Vector2 moveDirection;
    Vector2 mousePosition;
    bool canMove = true;

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
    }

    private void Start()
    {
        if (playerCamera == null)
        {
            enabled = false;
            return;
        }
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
        playerCamera.gameObject.SetActive(isOwner);
    }

    private void FixedUpdate()
    {
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
}
