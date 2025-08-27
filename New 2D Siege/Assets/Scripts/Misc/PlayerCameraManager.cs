using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    private List<PlayerCamera> _allPlayerCameras = new();
    
    private bool _canSwitchCamera = false;
    private int _currentCameraIndex = 0;
    
    public List<DroneCamera> _allDroneCameras = new();
    private bool _canSwitchDroneCamera = false;
    private int _currentDroneIndex = 0;
    private playerController _player;
    
    /*
        CAMERA SYSTEM LOGIC FLOW
        - EACH PLAYER PREFAB CONTAINS A CAMERA AND PlayerCamera.cs, WHEN PLAYER IS SPAWNED, RegisterCamera() IS CALLED.
        - WHEN A PLAYER DIES, UnregisterCamera() IS CALLED, ALLOWING THEM TO SWAP BETWEEN OTHER REGISTERED CAMERAS IN LIST.
        
        DRONE & CAMERA SYSTEM
        - ADD TWO MORE CAMERA LISTS. ONE FOR DRONES, ONE FOR DEFENDER CAMERAS.
        - USE SAME LOGIC FLOW FOR DRONES. WHEN A DRONE IS SPAWNED, REGISTER ITS CAMERA. WHEN DESTROYED, UNREGISTER.
        - WHEN DRONE BUTTON IS PRESSED, DISABLE PLAYER CAMERA AND ACTIVATE THEIR OWNED DRONE.
        - WHEN DRONE IS EXITED, DISABLE DRONE CAMERA AND ACTIVATE THEIR PLAYER CAMERA.
    */
    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<PlayerCameraManager>();
    }

    public void RegisterCamera(PlayerCamera cam)
    {
        if (!_allPlayerCameras.Contains(cam))
            _allPlayerCameras.Add(cam);

        if (cam.isOwner)
        {
            _canSwitchCamera = false;
            cam.ToggleCamera(true);
        }
    }

    public void UnregisterCamera(PlayerCamera cam)
    {
        if (_allPlayerCameras.Contains(cam))
            _allPlayerCameras.Remove(cam);

        if (cam.isOwner)
        {
            _canSwitchCamera = true;
            SwitchNext();
        }
    }

    public void RegisterDroneCamera(DroneCamera cam)
    {
        if (!_allDroneCameras.Contains(cam))
            _allDroneCameras.Add(cam);
    }

    public void UnregisterDroneCamera(DroneCamera cam)
    {
        if (_allDroneCameras.Contains(cam))
            _allDroneCameras.Remove(cam);
        
        if (cam.droneCamera.Priority == 10)
        {
            DroneSwitchNext();
        }
    }

    private void Update()
    {
        if (_canSwitchDroneCamera)
        {
            if (Input.GetKeyDown(KeyCode.E))
                DroneSwitchNext();
            if (Input.GetKeyDown(KeyCode.Q))
                DroneSwitchPrevious();
        }

        if (!_canSwitchCamera) 
            return;
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
            SwitchNext();
        if (Input.GetKeyDown(KeyCode.Mouse1))
            SwitchPrevious();
    }

    public void GoOnDrones(PlayerCamera cam, playerController player)
    {
        if (_allDroneCameras.Count <= 0)
            return;
        
        if (cam.isOwner)
        {
            foreach (var drone in _allDroneCameras)
            {
                drone.GetComponentInParent<DroneController>().PlayerIsOnCamerasToggle(true);
            }
            
            player.ToggleWeaponEquipped();
            player.OnCamerasToggle(true);
            _player = player;
            _canSwitchDroneCamera = true;
            cam.ToggleCamera(false);
            _currentDroneIndex = 0;
            _allDroneCameras[_currentDroneIndex].ToggleCamera(true);
        }
    }

    public void ExitDrones(PlayerCamera cam, playerController player)
    {
        if (cam.isOwner)
        {
            foreach (var drone in _allDroneCameras)
            {
                drone.GetComponentInParent<DroneController>().PlayerIsOnCamerasToggle(false);
            }
            
            player.ToggleWeaponEquipped();
            player.OnCamerasToggle(false);
            _canSwitchDroneCamera = false;
            _allDroneCameras[_currentDroneIndex].ToggleCamera(false);
            cam.ToggleCamera(true);
        }
    }

    private void SwitchNext()
    {
        if (_allPlayerCameras.Count <= 0)
            return;

        _allPlayerCameras[_currentCameraIndex].ToggleCamera(false);
        _currentCameraIndex++;
        if (_currentCameraIndex >= _allPlayerCameras.Count)
            _currentCameraIndex = 0;
        _allPlayerCameras[_currentCameraIndex].ToggleCamera(true);
    }
    
    private void SwitchPrevious()
    {
        if (_allPlayerCameras.Count <= 0)
            return;
        
        _allPlayerCameras[_currentCameraIndex].ToggleCamera(false);
        _currentCameraIndex--;
        if (_currentCameraIndex < 0)
            _currentCameraIndex = _allPlayerCameras.Count - 1;
        _allPlayerCameras[_currentCameraIndex].ToggleCamera(true);
    }

    private void DroneSwitchNext()
    {
        if (_allDroneCameras.Count <= 0)
            return;
        
        _allDroneCameras[_currentDroneIndex].ToggleCamera(false);
        _currentDroneIndex++;
        if (_currentDroneIndex >= _allDroneCameras.Count)
            _currentDroneIndex = 0;
        _allDroneCameras[_currentDroneIndex].ToggleCamera(true);
        
    }

    private void DroneSwitchPrevious()
    {
        if (_allDroneCameras.Count <= 0)
            return;
        
        _allDroneCameras[_currentDroneIndex].ToggleCamera(false);
        _currentDroneIndex--;
        if (_currentDroneIndex < 0)
            _currentDroneIndex = _allDroneCameras.Count - 1;
        _allDroneCameras[_currentDroneIndex].ToggleCamera(true);
    }
}
