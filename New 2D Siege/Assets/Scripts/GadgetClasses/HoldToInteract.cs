using UnityEngine;
using UnityEngine.InputSystem;

public abstract class HoldToInteract : MonoBehaviour
{
    [SerializeField] protected ProgressBarController progressBar;
    [SerializeField] protected InputAction interactionKey;
    [SerializeField] protected float duration = 2f;

    protected bool isInteracting;

    protected virtual void Update()
    {
        if (interactionKey.WasPressedThisFrame() && CanInteract())
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
