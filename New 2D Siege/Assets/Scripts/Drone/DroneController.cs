using System;
using UnityEngine;

public class DroneController : DroneGadget
{
    [SerializeField] private float moveSpeed = 3f;
    
    private playerController _playerController;
    private Vector2 _move;
    
    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        if (!isOwner)
            return;
        
        _playerController = playerObject.GetComponent<playerController>();
    }

    private void FixedUpdate()
    {
        if (!canMove)
            return;
        
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        if (input.magnitude > 1)
            input.Normalize();
        
        var movement = input * moveSpeed;
        gadgetRigidbody.linearVelocity = movement;
        
        if (Application.isFocused)
            RotateTowardsMouse();
    }
    
    private void RotateTowardsMouse()
    {
        if (!mainCamera) return;
        
        var mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var direction = mouseWorldPosition - transform.position;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        gadgetRigidbody.rotation = angle - 90f;
    }

    protected override void PickupGadget()
    {
        DronePickup();
        roundView.HideGadgetPickupText(this);
        thrownGadgetPickedUpLocal = true;
        gadgetController.timeAtPickup = Time.unscaledTime;
        gadgetController.thrownGadgetIsBeingPickedUp = false;
        gadgetController.gadgetBeingPickedUp = false;
        Destroy(gameObject);
    }

    private void DronePickup()
    {
        gadgetController.ChangeDroneCount(1);
    }
}
