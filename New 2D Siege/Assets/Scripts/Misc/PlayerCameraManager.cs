using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    private List<PlayerCamera> _allPlayerCameras = new();
    
    private bool _canSwitchCamera = false;
    private int _currentCameraIndex = 0;

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

    private void Update()
    {
        if (!_canSwitchCamera)
            return;
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
            SwitchNext();
        if (Input.GetKeyDown(KeyCode.Mouse1))
            SwitchPrevious();
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
}
