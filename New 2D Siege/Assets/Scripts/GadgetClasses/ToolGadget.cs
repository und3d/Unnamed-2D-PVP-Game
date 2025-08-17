using System;
using System.Collections;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class ToolGadget : GadgetBase
{
    [Header("Gadget Settings")] 
    public bool gadgetIsEnabled;
    [SerializeField] private SpriteRenderer gadgetVisual;
    [SerializeField] private Vector3 gadgetPosition;

    [Header("Gun Tool Settings")] 
    [SerializeField] protected int toolGunGadgetCount = 3;
    [SerializeField] protected float range = 20f;
    [SerializeField] protected float reloadTime = 2.5f;
    [SerializeField] private float recoilStrength = 1f;
    [SerializeField] private float recoilDuration = 0.2f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private AnimationCurve recoilCurve;
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem enviroHitEffect, playerHitEffect;
    private int currentAmmo;
    private Vector2 _originalPosition;
    private Coroutine _recoilCoroutine;
    private InputAction _reloadKey;
    private bool isReloading;
    private Coroutine _reloadCoroutine;
    
    
    private GadgetController ownerGadgetController;
    private GameObject player;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
        
        _originalPosition = transform.localPosition;

        if (!isOwner)
            return;
        
        // Put bullet into chamber of gadget
        if (toolGadgetType == ToolGadgetType.Gun)
        {
            currentAmmo = 1;
            toolGunGadgetCount -= 1;
        }
        
        UpdateGadgetCounter(currentAmmo, toolGunGadgetCount);
    }

    protected override void Awake()
    {
        base.Awake();
        
        _reloadKey = InputManager.PlayerKeybinds.Get("Player/Reload");
    }

    protected override void Update()
    {
        //Gadget is not placed, no pickup needed
        if (!isOwner || !gadgetIsEnabled)
            return;
        
        switch (toolGadgetType)
        {
            case ToolGadgetType.None:
                // Default case
                break;
            case ToolGadgetType.Gun:
                ToolGadgetGun();
                break;
            case ToolGadgetType.Durability:
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Debug.Log("Used The Gadget");
                }
                break;
            case ToolGadgetType.Infinite:
                // Vision gadget functionality
                break;
            default:
                break;
        }
    }

    protected void UpdateGadgetCounter(int _currentAmmo, int _remainingAmmo)
    {
        if (!InstanceHandler.TryGetInstance(out RoundView roundView))
        {
            Debug.LogError($"Gun failed to get roundView!", this);
        }
        
        roundView.UpdateGadgetPrimaryCountToolGun(_currentAmmo, _remainingAmmo);
    }

    private void ToolGadgetGun()
    {
        if (_reloadKey.IsPressed() && toolGunGadgetCount > 0 && !isReloading && currentAmmo < 1)
        {
            _reloadCoroutine = StartCoroutine(StartReload());
            isReloading = true;
        }
        
        if (currentAmmo == 0 || isReloading)
            return;
        
        // SWAP TO NEW INPUT SYSTEM
        if (!Input.GetKeyDown(KeyCode.Mouse0))
            return;

        currentAmmo -= 1;
        UpdateGadgetCounter(currentAmmo, toolGunGadgetCount);
        PlayShotEffect();

        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        Vector2 direction = (mouseWorldPosition - (Vector2)playerTransform.position).normalized;

        Ray2D ray = new(shootOrigin.position, direction);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, range, hitLayer);
        
        if (hit)
        {
            if (!hit.transform.TryGetComponent(out PlayerHealth playerHealth))
            {
                if (enviroHitEffect)
                {
                    EnvironmentHit(hit.point, hit.normal);
                    return;
                }
            }
            PlayerHit(playerHealth, playerHealth.transform.InverseTransformPoint(hit.point), hit.normal);
        }

        GadgetFunctionality(hit);
    }

    protected virtual void GadgetFunctionality(RaycastHit2D hit)
    {
        Debug.Log("Tool gadget functionality not implemented.");
    }
    
    private IEnumerator StartReload()
    {
        var elapsed = 0f;

        while (elapsed < reloadTime)
        {
            elapsed += Time.deltaTime;
            
            yield return null;
        }
        
        if (!isReloading)
            yield break;
        
        toolGunGadgetCount -= 1;
        currentAmmo = 1;
        
        UpdateGadgetCounter(currentAmmo, toolGunGadgetCount);
        isReloading = false;
    }
    
    private IEnumerator PlayRecoil()
    {
        var elapsed = 0f;

        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            var curveTime = elapsed / recoilDuration;
            
            var recoilValue = recoilCurve.Evaluate(curveTime);
            var recoilOffset = Vector2.down * (recoilValue * recoilStrength);
            transform.localPosition = _originalPosition + recoilOffset;
            
            yield return null;
        }
        
        transform.localPosition = _originalPosition;
    }
    
    [ObserversRpc(runLocally:true)]
    private void PlayShotEffect()
    {
        if (muzzleFlash)
            muzzleFlash.Play();
        
        if (_recoilCoroutine != null)
            StopCoroutine(_recoilCoroutine);
        
        _recoilCoroutine = StartCoroutine(PlayRecoil());
    }
    
    [ObserversRpc(runLocally: true)]
    private void EnvironmentHit(Vector3 position, Vector3 normal)
    {
        if (enviroHitEffect)
        {
            Instantiate(enviroHitEffect, position, Quaternion.LookRotation(normal));
        }
    }
    
    [ObserversRpc(excludeOwner: true)]
    private void PlayerHitConfirmation(PlayerHealth _player, Vector3 localPosition, Vector3 normal)
    {
        PlayerHit(_player, localPosition, normal);
    }
    
    private void PlayerHit(PlayerHealth _player, Vector3 localPosition, Vector3 normal)
    {
        if (playerHitEffect && _player)
        {
            Instantiate(playerHitEffect, _player.transform.TransformPoint(localPosition), Quaternion.LookRotation(normal));
        }
    }

    public void CancelReload()
    {
        if (!isReloading) 
            return;
        
        isReloading = false;
        StopCoroutine(_reloadCoroutine);
    }
    
    [ObserversRpc]
    public void ToggleGadgetVisuals()
    {
        gadgetVisual.enabled = !gadgetVisual.enabled;
    }
}
