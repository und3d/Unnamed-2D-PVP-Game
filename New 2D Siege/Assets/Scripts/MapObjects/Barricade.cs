using PurrNet;
using UnityEngine;

public class Barricade : NetworkIdentity
{
    [SerializeField] private GameObject barricadeChild;
    [SerializeField] private GameObject lockdownBarricade;
    [SerializeField] private SyncVar<int> barricadeHealth = new(100);
    [SerializeField] private bool enabledOnStart = false;
    [SerializeField] private float playerPlaceDistance = 2f;

    private GameObject playerObject;
    private bool isBeingPlaced = false;
    private bool regularActive;
    private bool lockdownActive;
    private GameController gameController;
    
    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        barricadeHealth.onChanged += OnHealthChanged;
        
        if (enabledOnStart)
        {
            barricadeChild.SetActive(true);
            regularActive = true;
        }
        
        if (!InstanceHandler.TryGetInstance(out gameController))
        {
            Debug.LogError($"GameStartState failed to get gameController!", this);
        }
    }

    private void OnHealthChanged(int newHealth)
    {
        //Debug.Log($"New Barricade Health: {newHealth}");
        if (newHealth <= 0)
        {
            ToggleBarricade(true);
        }
    }

    public void Hit(int damage)
    {
        barricadeHealth.value += damage;
    }

    public bool GetRegularActive()
    {
        return regularActive;
    }

    public bool getLockdownActive()
    {
        return lockdownActive;
    }

    [ObserversRpc]
    public void SetVisibility(bool regular)
    {
        if (regular)
        {
            barricadeChild.SetActive(!barricadeChild.activeSelf);
            regularActive = !regularActive;
        }
        else
        {
            lockdownBarricade.SetActive(!lockdownBarricade.activeSelf);
            lockdownActive = !lockdownActive;
        }
    }
    
    [ServerRpc]
    public void ToggleBarricade(bool regular)
    {
        barricadeHealth.value = 100;
        SetVisibility(regular);
    }
}
