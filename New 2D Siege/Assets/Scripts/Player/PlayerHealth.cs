using System;
using PurrNet;
using UnityEngine;

public class PlayerHealth : NetworkIdentity
{
    [SerializeField] private SyncVar<int> health = new(100);
    [SerializeField] public SyncVar<bool> isRedTeam = new(true);
    [SerializeField] private SyncVar<bool> canBeShot = new SyncVar<bool>(true, ownerAuth:true);
    [SerializeField] private int selfLayer, otherLayer;
    [SerializeField] private int playerIconID;
    
    public Action<PlayerID> OnDeath_Server;

    public int Health => health;
    
    private GameController gameController;
    //private bool isRedTeamBool = true;
    
    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        var actualLayer = isOwner ? selfLayer : otherLayer;
        SetLayerRecursive(gameObject, actualLayer);

        if (!isOwner) 
            return;
        
        if (!InstanceHandler.TryGetInstance(out gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }
        
        GetPlayerTeam();
        
        InstanceHandler.GetInstance<RoundView>().UpdateHealth(health.value);
        health.onChanged += OnHealthChanged;
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

  
    public void ChangeHealth(int amount, PlayerID shooter)
    {
        if (!isServer)
            return;
        
        health.value += amount;

        if (health <= 0)
        {
            if (InstanceHandler.TryGetInstance(out GameController _gameController))
            {
                _gameController.AddKill(shooter);
                if (owner.HasValue)
                    _gameController.AddDeath(owner.Value);
                else
                    Debug.LogError($"Owner has no value!");
                
                Debug.Log("Player has died, calling ChangePlayerIconOnDeath");
                ChangePlayerIconOnDeath(playerIconID, isRedTeam.value ? GameController.Team.Red : GameController.Team.Blue);
            }
            OnDeath_Server?.Invoke(owner.Value);
            Destroy(gameObject);
        }
    }

    private void GetPlayerTeam()
    {
        //Debug.Log($"Get Player {owner.Value}'s Team");
        gameController.GetTeam(owner.Value, this);
    }
    
    public void SetPlayerIconID(int iconID)
    {
        playerIconID = iconID;
    }
    
    public void CanBeShotToggle(bool toggle)
    {
        Debug.Log($"Setting canBeShot to: {toggle}");
        
        canBeShot.value = toggle;
    }

    public bool GetCanBeShotValue()
    {
        return canBeShot.value;
    }
    
    [ObserversRpc]
    public void SetColor(Color teamColor)
    {
        GetComponent<SpriteRenderer>().color = teamColor;
    }

    [ServerRpc]
    private void ChangePlayerIconOnDeath(int _playerIconID, GameController.Team _team)
    {
        Debug.Log($"Set (PlayerIconID: {_playerIconID}, Team: {_team}) to dead. (Server)");
        ChangePlayerIconOnDeathServer(_playerIconID, _team);
    }

    [ObserversRpc(runLocally:true)]
    private void ChangePlayerIconOnDeathServer(int _playerIconID, GameController.Team _team)
    {
        if (!InstanceHandler.TryGetInstance(out RoundView roundView))
        {
            Debug.LogError($"Player failed to get RoundView!", this);
        }
        
        //Debug.Log($"Set (PlayerIconID: {_playerIconID}, Team: {_team}) to dead.");
        
        if (_team == GameController.Team.Red)
            roundView.SetRedPlayerIconOnDeath(_playerIconID);
        else
            roundView.SetBluePlayerIconOnDeath(_playerIconID);
    }
}
