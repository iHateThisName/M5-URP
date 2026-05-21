using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour {
    [SerializeField] private InputActionReference cameraMoveAction;
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private float speed = 2f;
    private InputAction cameraInputAction;
    private Camera mainCamera;

    private void OnEnable() {
        this.cameraInputAction = this.cameraMoveAction.action;
        this.cameraInputAction.Enable();
        this.cameraInputAction.performed += OnCameraMove;
    }


    private void OnDisable() {
        this.cameraInputAction.Disable();
    }
    private void OnCameraMove(InputAction.CallbackContext context) {
        // Get the input value as a Vector2
        Vector2 input = context.ReadValue<Vector2>();
        Vector3 move = new Vector3(-input.x, input.y, 0);
        float step = this.speed * Time.deltaTime;

        this.mainCamera.transform.RotateAround(this.lookAtTarget.position, Vector3.up, move.x * step);
        this.mainCamera.transform.RotateAround(this.lookAtTarget.position, this.mainCamera.transform.right, move.y * step);

        this.mainCamera.transform.LookAt(this.lookAtTarget); // Double-check that the camera is always looking at the target
    }

    private void Start() {
        this.mainCamera = Camera.main;
    }

}
