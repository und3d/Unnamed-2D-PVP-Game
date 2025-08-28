using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : StateNode
{
    [Header("Stats")] 
    [SerializeField] private string gunName = "placeHolderGunName";
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private bool automatic;
    [SerializeField] private int magSize = 30;
    [SerializeField] private int maxReserveAmmo = 150;
    private int currentReserveAmmo;
    private int currentAmmo;
    private int magAmmoBeforeReload;
    [SerializeField] private float reloadTime = 2.5f;

    [Header("Recoil")] 
    [SerializeField] private float recoilStrength = 1f;
    [SerializeField] private float recoilDuration = 0.2f;
    [SerializeField] private AnimationCurve recoilCurve;
    
    [Header("References")]
    private playerController _player;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameController gameController;
    [SerializeField] private List<Renderer> renderers = new();
    [SerializeField] private ParticleSystem enviroHitEffect, playerHitEffect;

    private InputManager inputManager;
    private float _lastFireTime;
    private Vector2 _originalPosition;
    private Coroutine _recoilCoroutine;
    private InputAction _reloadKey;
    private bool isReloading;
    private Coroutine _reloadCoroutine;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        enabled = isOwner;
        
        _originalPosition = transform.localPosition;

        if (!InstanceHandler.TryGetInstance(out inputManager))
        {
            Debug.LogError("Gun failed to get input manager!", this);
            return;
        }
        
        _player = playerTransform.gameObject.GetComponent<playerController>();
        
        _reloadKey = inputManager.Get("Player/Reload");
    }

    private void Awake()
    {
        ToggleVisuals(false);
        
        currentReserveAmmo = maxReserveAmmo;
        currentAmmo = magSize + 1;
        
        UpdateAmmoCounter(currentAmmo, currentReserveAmmo);
        UpdateWeaponText();
    }

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        ToggleVisuals(true);
        
        UpdateAmmoCounter(currentAmmo, currentReserveAmmo);
        
        if (isOwner)
        {
            UpdateWeaponText();
        }
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
        ToggleVisuals(false);
        
        if (_reloadCoroutine == null)
            return;
        
        isReloading = false;
        StopCoroutine(_reloadCoroutine);
    }

    private void ToggleVisuals(bool toggle)
    {
        foreach (var renderer in renderers)
        {
            renderer.enabled = toggle;
        }
    }

    // This update function only updates when the STATE is active
    public override void StateUpdate(bool asServer)
    {
        base.StateUpdate(asServer);

        if (!isOwner)
            return;

        if (_player.isOnCameras)
            return;
        
        if (_reloadKey.IsPressed() && currentReserveAmmo > 0 && !isReloading && currentAmmo < magSize + 1)
        {
            magAmmoBeforeReload = currentAmmo;
            if (currentAmmo > 1)
            {
                currentReserveAmmo += magAmmoBeforeReload - 1;
                currentAmmo = 1;
                
                UpdateAmmoCounter(currentAmmo, currentReserveAmmo);
                _reloadCoroutine = StartCoroutine(StartReload());
            }
            else if (currentAmmo <= 1)
            {
                _reloadCoroutine = StartCoroutine(StartReload());
            }
            isReloading = true;
        }
        
        if (currentAmmo == 0 || isReloading)
            return;
        
        // SWAP TO NEW INPUT SYSTEM
        if (automatic && !Input.GetKey(KeyCode.Mouse0) || !automatic && !Input.GetKeyDown(KeyCode.Mouse0))
            return;

        if (_lastFireTime + fireRate > Time.unscaledTime)
            return;

        currentAmmo -= 1;
        UpdateAmmoCounter(currentAmmo, currentReserveAmmo);
        PlayShotEffect();
        _lastFireTime = Time.unscaledTime;

        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPosition - (Vector2)playerTransform.position).normalized;

        Ray2D ray = new(shootOrigin.position, direction);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, range, hitLayer);
        
        if (hit)
        {
            if (!hit.transform.TryGetComponent(out PlayerHealth playerHealth))
            {
                if (!hit.transform.TryGetComponent(out GadgetBase gadget))
                {
                    if (!hit.transform.parent.parent.TryGetComponent(out Barricade barricade))
                    {
                        if (enviroHitEffect)
                        {
                            EnvironmentHit(hit.point, hit.normal);
                            return;
                        }
                    }
                    BarricadeHit(barricade);
                    EnvironmentHit(hit.point, hit.normal);
                    return;
                }
                GadgetHit(gadget);
                return;
            }
            PlayerHit(playerHealth, playerHealth.transform.InverseTransformPoint(hit.point), hit.normal);
            
        }
        HandleHit(ray, networkManager.tickModule.rollbackTick);
    }

    private void UpdateAmmoCounter(int _currentAmmo, int _currentReserveAmmo)
    {
        if (!InstanceHandler.TryGetInstance(out RoundView roundView))
        {
            Debug.LogError($"Gun failed to get roundView!", this);
        }
        
        roundView.UpdateAmmoCounter(_currentAmmo, _currentReserveAmmo);
    }

    private void UpdateWeaponText()
    {
        if (!InstanceHandler.TryGetInstance(out RoundView roundView))
        {
            Debug.LogError($"Gun failed to get roundView!", this);
        }

        roundView.UpdateWeaponText(gunName);
    }

    private void GadgetHit(GadgetBase gadget)
    {
        gadget.GadgetShot();
    }

    [ServerRpc]
    private void BarricadeHit(Barricade barricade)
    {
        if (barricade.getLockdownActive())
        {
            return;
        }
        
        Debug.Log("Barricade Hit");
        barricade.Hit(-damage);
    }

    [ServerRpc]
    private void HandleHit(Ray2D ray, Double preciseTick, RPCInfo info = default)
    {
        var hitLayers = new ContactFilter2D();
        hitLayers.SetLayerMask(hitLayer);
        hitLayers.useLayerMask = true;
        
        if (rollbackModule.Raycast(preciseTick, ray, out var hit, range, hitLayers))
        {
            if (!hit.transform.TryGetComponent(out PlayerHealth playerHealth))
            {
                return;
            }

            //Debug.Log($"Player should be losing health.");
            if (playerHealth.GetCanBeShotValue())
            {
                playerHealth.ChangeHealth(-damage, info.sender);
            }
            PlayerHitConfirmation(playerHealth, playerHealth.transform.InverseTransformPoint(hit.point), hit.normal);
        }
    }

    [ObserversRpc(excludeOwner: true)]
    private void PlayerHitConfirmation(PlayerHealth player, Vector3 localPosition, Vector3 normal)
    {
        PlayerHit(player, localPosition, normal);
    }
    
    private void PlayerHit(PlayerHealth player, Vector3 localPosition, Vector3 normal)
    {
        if (playerHitEffect && player && player.GetCanBeShotValue())
        {
            Instantiate(playerHitEffect, player.transform.TransformPoint(localPosition), Quaternion.LookRotation(normal));
        }
        else if (enviroHitEffect && player && !player.GetCanBeShotValue())
        {
            Instantiate(enviroHitEffect, player.transform.TransformPoint(localPosition), Quaternion.LookRotation(normal));
        }
    }
    
    [ObserversRpc(runLocally: true)]
    private void EnvironmentHit(Vector3 position, Vector3 normal)
    {
        if (enviroHitEffect)
        {
            Instantiate(enviroHitEffect, position, Quaternion.LookRotation(normal));
        }
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

    private IEnumerator StartReload()
    {
        var elapsed = 0f;

        while (elapsed < reloadTime)
        {
            elapsed += Time.deltaTime;
            
            yield return null;
        }

        if (currentReserveAmmo >= magSize && isReloading)
        {
            Debug.Log($"Reserve is greater than {magSize}. Reloading...");
            
            currentReserveAmmo -= magSize;
            currentAmmo += magSize;
        }
        else
        {
            if (!isReloading)
                yield break;
            Debug.Log($"Reserve is less than {magSize}. Reloading...");
            currentAmmo += currentReserveAmmo;
            currentReserveAmmo = 0;
        }
        
        UpdateAmmoCounter(currentAmmo, currentReserveAmmo);
        isReloading = false;
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
}
