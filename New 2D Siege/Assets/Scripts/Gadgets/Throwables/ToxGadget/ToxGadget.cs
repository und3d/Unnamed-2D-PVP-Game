using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class ToxGadget : ThrowableGadget
{
    [SerializeField] private SpriteRenderer gadgetSprite;
    [SerializeField] private SpriteRenderer toxCloud;
    [SerializeField] private CircleCollider2D toxCloudCollider;
    [SerializeField] private float timeBetweenTicks = 0.5f;
    [SerializeField] private float toxCloudDuration = 15f;
    [SerializeField] private int damagePerTick = 10;
    
    public List<GameObject> playersInToxCloud;
    private float lastToxTick;
    private float timeOfActivation;
    
    protected override void Update()
    {
        if (!isOwner)
            return;
        if (!playerObject)
            return;
        if (!gameController)
            return;
        
        if (toxCloud.enabled)
        {
            if (Time.unscaledTime - timeOfActivation > toxCloudDuration)
                Destroy(gameObject);
            
            if (!(Time.unscaledTime - lastToxTick > timeBetweenTicks)) 
                return;
            
            DamagePlayers(playersInToxCloud);
            
            lastToxTick = Time.unscaledTime;

            return;
        }
        
        if (isOwner && !(Time.unscaledTime < timeAtPlacement + timeBeforePickup) && canBePickedUp)
        {
            HandlePickup();
            if (gadgetController.thrownGadgetIsBeingPickedUp)
            {
                Debug.Log($"thrownGadgetIsBeingPickedUp: {gadgetController.thrownGadgetIsBeingPickedUp}", this);
                return;
            }
        }

        //Debug.Log($"thrownGadgetPickedUp: {thrownGadgetPickedUp}", this);

        if (thrownGadgetPickedUpLocal)
        {
            ActivationDelay();
        }
    }

    private void LateUpdate()
    {
        if (!gameController || !gadgetController)
            return;
        if (gadgetController.isGadgetPulledOut)
            return;
        if (gameController.justThrewGadget)
            return;
        
        if (roundView.IsGadgetPickupTextActive())
        {
            //Debug.Log($"isGadgetInteractTextActive: {roundView.IsGadgetPickupTextActive()}", this);
            return;
        }
        
        //Debug.Log($"isGadgetInteractTextActive: {roundView.IsGadgetPickupTextActive()}", this);
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            
            ActivateGadget();
        }
    }

    private void ActivationDelay()
    {
        if (!(Time.unscaledTime < gadgetController.timeAtPickup + gadgetController.timeBeforeActivation))
        {
            thrownGadgetPickedUpLocal = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || other.gameObject == playerObject)
            return;
        
        playersInToxCloud.Add(other.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!playersInToxCloud.Contains(other.gameObject))
            return;
        playersInToxCloud.Remove(other.gameObject);
    }

    [ObserversRpc]
    protected override void ActivateGadget()
    {
        FreezeGadget(true);
        gadgetSprite.enabled = false;
        toxCloud.enabled = true;
        toxCloudCollider.enabled = true;
        
        lastToxTick = Time.unscaledTime;
        timeOfActivation = Time.unscaledTime;
    }

    [ServerRpc]
    private void DamagePlayers(List<GameObject> playerList)
    {
        foreach (var player in playerList)
        {
            if (!player.TryGetComponent(out PlayerHealth playerHealth))
            {
                Debug.LogError($"Player {player.name} does not have a PlayerHealth component!", this);
                continue;
            }

            if (owner != null) playerHealth.ChangeHealth(-damagePerTick, owner.Value);
            Debug.Log($"Player {player.name} has been hit by toxic cloud for {damagePerTick} damage.");
        }
    }
}
