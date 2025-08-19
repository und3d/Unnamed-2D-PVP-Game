using PurrNet;
using UnityEngine;

public class Barricade : NetworkIdentity
{
    [SerializeField] private GameObject barricadeChild;
    [SerializeField] private SyncVar<int> barricadeHealth = new(100);
    [SerializeField] private bool enabledOnStart = false;
    [SerializeField] private float playerPlaceDistance = 2f;

    private GameObject playerObject;
    private bool isBeingPlaced = false;
    private GameController gameController;
    
    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        barricadeHealth.onChanged += OnHealthChanged;
        
        if (enabledOnStart)
        {
            barricadeChild.SetActive(true);
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
            ToggleBarricade();
        }
    }

    public void Hit(int damage)
    {
        barricadeHealth.value += damage;
    }

    [ObserversRpc]
    public void SetVisibility()
    {
        barricadeChild.SetActive(!barricadeChild.activeSelf);
    }
    
    [ServerRpc]
    public void ToggleBarricade()
    {
        barricadeHealth.value = 100;
        SetVisibility();
    }
}
