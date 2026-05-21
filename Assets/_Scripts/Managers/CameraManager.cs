using Assets.Scripts.Singleton;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : Singleton<CameraManager> {
    [SerializeField] private CinemachineVirtualCameraBase aimCamera;
    [SerializeField] private CinemachineVirtualCameraBase followCamera;
    [SerializeField] private InputActionReference aimRefrence;
    private InputAction aimAction;

    private bool isAiming = false;

    public static Action<bool> OnAimStateChanged; // Action to notify other scripts about the aiming state change

    private void OnEnable() {
        this.aimAction = this.aimRefrence.action;
        this.aimAction.Enable();
        this.aimAction.performed += OnAim;
        this.aimAction.canceled += OnAim;
    }

    private void OnDisable() {
        this.aimAction.Disable();
        this.aimAction.performed -= OnAim;
        this.aimAction.canceled -= OnAim;
    }

    private void OnAim(InputAction.CallbackContext context) {
        if (context.performed) {
            this.isAiming = true;
            ToggleCameraPriorty();
            OnAimStateChanged?.Invoke(true);

        } else if (context.canceled) {
            this.isAiming = false;
            ToggleCameraPriorty();
            OnAimStateChanged?.Invoke(false);
        }
    }

    private void ToggleCameraPriorty() {
        if (this.isAiming) {
            this.aimCamera.Priority = 1;
            this.followCamera.Priority = 0;
        } else {
            this.aimCamera.Priority = 0;
            this.followCamera.Priority = 1;
        }
    }
}