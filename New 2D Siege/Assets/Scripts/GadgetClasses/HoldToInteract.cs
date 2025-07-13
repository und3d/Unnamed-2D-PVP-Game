using UnityEngine;

public abstract class HoldToInteract : MonoBehaviour
{
    [SerializeField] protected ProgressBarController progressBar;
    [SerializeField] protected KeyCode interactionKey;
    [SerializeField] protected float duration = 2f;

    protected bool isInteracting;

    protected virtual void Update()
    {
        if (Input.GetKeyDown(interactionKey) && CanInteract())
        {
            progressBar.BeginInteraction(new InteractionRequest
            {
                
                duration = duration,
                key = interactionKey,
                canStart = CanInteract,
                onComplete = OnComplete
            });
        }
    }

    protected abstract bool CanInteract();
    protected abstract void OnComplete();
}
