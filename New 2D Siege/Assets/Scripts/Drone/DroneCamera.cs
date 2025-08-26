using System.Collections.Generic;
using PurrNet;
using Unity.Cinemachine;
using UnityEngine;

public class DroneCamera : NetworkBehaviour
{
    public CinemachineCamera droneCamera;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        
        InstanceHandler.GetInstance<PlayerCameraManager>().RegisterDroneCamera(this);
    }

    protected override void OnDespawned()
    {
        base.OnDespawned();
        
        InstanceHandler.GetInstance<PlayerCameraManager>().UnregisterDroneCamera(this);
    }

    public void ToggleCamera(bool toggle)
    {
        droneCamera.Priority = toggle ? 10 : 0;
        
        gameObject.GetComponentInParent<DroneController>().ToggleActive(toggle, isOwner);
    }
}
