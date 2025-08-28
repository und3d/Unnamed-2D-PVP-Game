using PurrNet;
using UnityEngine;

public abstract class ToggleGadget : GadgetBase
{
    [Header("Toggle Gadget Settings")]
    [SerializeField] private bool doesRecharge;
    [SerializeField] private float timerDuration;
    [SerializeField] private float rechargeDuration;
    [SerializeField] private float minimumActivationDuration;
    [SerializeField] private float timeBetweenActivations;
    [SerializeField] private SpriteRenderer gadgetVisual;

    public bool gadgetIsEnabled;

    private float durationLeft;
    private float _deactivationTime;
    protected playerController _playerController;
    protected PlayerHealth _playerHealth;
    protected bool gadgetActiveCheck;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;

        if (!isOwner)
            return;
        
        durationLeft = timerDuration;
        
        _playerController = GetComponentInParent<playerController>();
        _playerHealth = GetComponentInParent<PlayerHealth>();
        
        if (toggleGadgetType == ToggleGadgetType.Timer)
            UpdateGadgetDurationVisual(durationLeft);
        else if (toggleGadgetType == ToggleGadgetType.Infinite)
            roundView.SetGadgetPrimaryCountInfinite();
    }

    protected virtual void FixedUpdate()
    {
        if (!isOwner)
            return;
        
        if (gadgetIsEnabled)
        {
            switch (toggleGadgetType)
            {
                case ToggleGadgetType.None:
                    Debug.LogError("ToggleGadgetType is set to None.");
                    gadgetIsEnabled = false;
                    break;
                case ToggleGadgetType.Infinite:
                    if (gadgetActiveCheck)
                        break;
                    gadgetActiveCheck = true;
                    GadgetFunctionalityToggle(gadgetIsEnabled);
                    break;
                case ToggleGadgetType.Timer:
                    if (durationLeft > 0)
                    {
                        durationLeft -= Time.deltaTime;
                        UpdateGadgetDurationVisual(durationLeft);
                        
                        if (gadgetActiveCheck)
                            break;
                        gadgetActiveCheck = true;
                        GadgetFunctionalityToggle(gadgetIsEnabled);

                        break; // Exit case
                    }

                    gadgetController.ToggleGadgetActivationToggle();
                    break;
            }
        }
        else
        {
            switch (toggleGadgetType)
            {
                case ToggleGadgetType.None:
                    // Default case, do nothing.
                    break;
                case ToggleGadgetType.Infinite:
                    if (gadgetActiveCheck)
                    {
                        GadgetFunctionalityToggle(gadgetIsEnabled);
                        gadgetActiveCheck = false;
                    }
                    break;
                case ToggleGadgetType.Timer:
                    if (gadgetActiveCheck)
                    {
                        GadgetFunctionalityToggle(gadgetIsEnabled);
                        gadgetActiveCheck = false;
                    }
                    
                    if (!doesRecharge || durationLeft >= timerDuration)
                        break;
                    durationLeft = Mathf.MoveTowards(
                    durationLeft,
                    timerDuration,
                    (timerDuration / rechargeDuration) * Time.deltaTime
                        );
                    UpdateGadgetDurationVisual(durationLeft);
                    break;
            }
        }
    }

    private void UpdateGadgetDurationVisual(float duration)
    {
        if (!roundView)
            return;
        roundView.UpdateGadgetPrimaryCountToggleTimer(duration);
    }

    public bool CanActivateToggleGadget()
    {
        return durationLeft > minimumActivationDuration || toggleGadgetType == ToggleGadgetType.Infinite;
    }

    public float GetTimeBetweenActivations()
    {
        return timeBetweenActivations;
    }

    protected virtual void GadgetFunctionalityToggle(bool toggle)
    {
        // Override this method in the specific gadget script
    }

    [ObserversRpc]
    public void ToggleGadgetVisuals()
    {
        gadgetVisual.enabled = !gadgetVisual.enabled;
    }
}
