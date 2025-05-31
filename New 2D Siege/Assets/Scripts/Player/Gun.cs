using System;
using PurrNet;
using UnityEngine;

public class Gun : NetworkIdentity
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;
    
    [SerializeField] private GameController gameController;
    
    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        enabled = isOwner;
    }

    private void Update()
    {
        if (gameController.debugMode)
        {
            
        }
        
        if (!Input.GetKeyDown(KeyCode.Mouse0))
            return;

        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPosition - (Vector2)playerTransform.position).normalized;
        
        RaycastHit2D hit = Physics2D.Raycast((Vector2)playerTransform.position, direction, range, hitLayer);
        
        if (!hit)
            return;

        if (!hit.transform.TryGetComponent(out PlayerHealth playerHealth))
            return;
        
        playerHealth.ChangeHealth(-damage);

        if (gameController.debugMode)
        {
            Debug.Log($"Hit: {hit.transform.name}");
            Debug.DrawRay(playerTransform.position, direction * range, Color.red, 1f);
        }
    }
}
