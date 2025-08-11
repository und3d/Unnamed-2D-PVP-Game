using PurrNet;
using UnityEngine;

public class DestructibleWall : NetworkIdentity
{
    [SerializeField] private SpriteRenderer wallRenderer;
    
    private GameController gameController;
    private SyncVar<bool> isReinforced = new(false);
    public bool IsReinforced => isReinforced.value;
    
    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        if (!InstanceHandler.TryGetInstance(out gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }
    }

    [ObserversRpc]
    private void ReinforceWallObservers()
    {
        wallRenderer.color = Color.red;
    }
    
    [ServerRpc]
    public void ReinforceWall()
    {
        isReinforced.value = true;
        ReinforceWallObservers();
    }
}
