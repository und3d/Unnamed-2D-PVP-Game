using PurrNet;
using PurrNet.StateMachine;
using System;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class playerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;

    [Header("References")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private StateMachine stateMachine;


    private Rigidbody2D _rigidbody;
    Vector2 moveDirection;
    Vector2 mousePosition;
    bool canMove = true;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
        playerCamera.gameObject.SetActive(isOwner); //Enabled camera if the user is controlling this player object
    }

    private void OnDisable()
    {
        return;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        if (playerCamera == null)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        HandleRotation(mousePosition);
        HandleMovement();
    }

    private void HandleRotation(Vector2 mousePos)
    {
        Vector2 aimDirection = mousePosition - _rigidbody.position;
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;

        if (canMove)
            _rigidbody.rotation = aimAngle;
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(horizontal, vertical).normalized;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        _rigidbody.linearVelocity = new Vector2(moveDirection.x * currentSpeed, moveDirection.y * currentSpeed) * Time.deltaTime;
    }
}
