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

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;

        if (!isOwner)
            return;
        
        if (!InstanceHandler.TryGetInstance(out RoundView roundView))
        {
            Debug.LogError($"Gun failed to get roundView!", this);
        }
        
        durationLeft = timerDuration;
        
        if (toggleGadgetType == ToggleGadgetType.Timer)
            UpdateGadgetDurationVisual(durationLeft);
        else if (toggleGadgetType == ToggleGadgetType.Infinite)
            roundView.SetGadgetPrimaryCountInfinite();
    }

    protected override void Update()
    {
        
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
                    // Gadget Functionality
                    break;
                case ToggleGadgetType.Timer:
                    if (durationLeft > 0)
                    {
                        durationLeft -= Time.deltaTime;
                        UpdateGadgetDurationVisual(durationLeft);

                        // Gadget Functionality

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
                    // Do nothing when disabled
                    break;
                case ToggleGadgetType.Timer:
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
        if (!InstanceHandler.TryGetInstance(out RoundView roundView))
        {
            Debug.LogError($"Gun failed to get roundView!", this);
        }
        
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

    [ObserversRpc]
    public void ToggleGadgetVisuals()
    {
        gadgetVisual.enabled = !gadgetVisual.enabled;
    }
}
