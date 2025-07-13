using UnityEngine;
using UnityEngine.Accessibility;

public abstract class Placeable : HoldToInteract
{
    [Header("Placeable Settings")]
    [SerializeField] protected GameObject prefabToSpawn;
    [SerializeField] protected float placementTime = 2f;
    [SerializeField] protected Transform spawnOffset;
    
    [Header("Optional")]
    [SerializeField] protected AudioClip placeSound;
    [SerializeField] protected GameController.Team ownerTeam;

    protected override bool CanInteract()
    {
        return CanPlaceHere();
    }

    protected override void OnComplete()
    {
        Spawn();
        PlayPlacementFeedback();
    }

    protected virtual bool CanPlaceHere()
    {
        // Add placement checks
        
        return true;
    }

    protected virtual void Spawn()
    {
        Vector2 position = spawnOffset ? spawnOffset.position : transform.position;
        Quaternion rotation = transform.rotation;
        
        GameObject placed = Instantiate(prefabToSpawn, position, rotation);
        
        // Write ownership code
    }

    protected virtual void PlayPlacementFeedback()
    {
        if (placeSound)
        {
            AudioSource.PlayClipAtPoint(placeSound, transform.position);
        }
    }

}
