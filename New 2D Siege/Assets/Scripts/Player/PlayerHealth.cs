using System;
using PurrNet;
using UnityEngine;

public class PlayerHealth : NetworkIdentity
{
    [SerializeField] private SyncVar<int> health = new(100);
    [SerializeField] private int selfLayer, otherLayer;
    
    public Action<PlayerID> OnDeath_Server;

    public int Health => health;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        var actualLayer = isOwner ? selfLayer : otherLayer;
        SetLayerRecursive(gameObject, actualLayer);

        if (isOwner)
        {
            InstanceHandler.GetInstance<RoundView>().UpdateHealth(health.value);
            health.onChanged += OnHealthChanged;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        health.onChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int newHealth)
    {
        InstanceHandler.GetInstance<RoundView>().UpdateHealth(newHealth);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    [ServerRpc(requireOwnership:false)]
    public void ChangeHealth(int amount, RPCInfo info = default)
    {
        health.value += amount;

        if (health <= 0)
        {
            if (InstanceHandler.TryGetInstance(out GameController gameController))
            {
                gameController.AddKill(info.sender);
                if (owner.HasValue)
                    gameController.AddDeath(owner.Value);
                else
                    Debug.LogError($"Owner has no value!");
            }
            OnDeath_Server?.Invoke(owner.Value);
            Destroy(gameObject);
        }
    }
}
