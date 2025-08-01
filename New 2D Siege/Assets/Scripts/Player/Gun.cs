using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class Gun : StateNode
{
    [Header("Stats")]
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private bool automatic;

    [Header("Recoil")] 
    [SerializeField] private float recoilStrength = 1f;
    [SerializeField] private float recoilDuration = 0.2f;
    [SerializeField] private AnimationCurve recoilCurve;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameController gameController;
    [SerializeField] private List<Renderer> renderers = new();
    [SerializeField] private ParticleSystem enviroHitEffect, playerHitEffect;

    private float _lastFireTime;
    private Vector2 _originalPosition;
    private Coroutine _recoilCoroutine;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        enabled = isOwner;
        
        _originalPosition = transform.localPosition;
        
    }

    private void Awake()
    {
        ToggleVisuals(false);
    }

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        ToggleVisuals(true);
    }

    public override void Exit(bool asServer)
    {
        base.Exit(asServer);
        ToggleVisuals(false);
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
        
        if (automatic && !Input.GetKey(KeyCode.Mouse0) || !automatic && !Input.GetKeyDown(KeyCode.Mouse0))
            return;

        if (_lastFireTime + fireRate > Time.unscaledTime)
            return;
        
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

    private void GadgetHit(GadgetBase gadget)
    {
        gadget.GadgetShot();
    }

    [ServerRpc]
    private void BarricadeHit(Barricade barricade)
    {
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
            playerHealth.ChangeHealth(-damage, info.sender);
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
        if (playerHitEffect && player)
        {
            Instantiate(playerHitEffect, player.transform.TransformPoint(localPosition), Quaternion.LookRotation(normal));
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
        float elapsed = 0f;

        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            float curveTime = elapsed / recoilDuration;
            
            float recoilValue = recoilCurve.Evaluate(curveTime);
            Vector2 recoilOffset = Vector2.down * (recoilValue * recoilStrength);
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
}
