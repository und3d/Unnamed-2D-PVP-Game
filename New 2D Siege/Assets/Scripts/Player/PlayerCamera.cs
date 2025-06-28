using PurrNet;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    public CinemachineCamera playerCamera;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        InstanceHandler.GetInstance<PlayerCameraManager>().RegisterCamera(this);
    }

    protected override void OnDespawned()
    {
        base.OnDespawned();
        InstanceHandler.GetInstance<PlayerCameraManager>().UnregisterCamera(this);
    }

    public void ToggleCamera(bool toggle)
    {
        playerCamera.Priority = toggle ? 10 : 0;
    }
}
