using Assets.Scripts.Singleton;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static InteractionController;

public class PlayerMovement : Singleton<PlayerMovement> {
    [SerializeField] private CharacterController controller;
    [SerializeField] private float speed = 7f;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference leftMouseAction;
    private InputAction moveInputAction;
    private InputAction lookInputAction;
    private InputAction leftMouseInputAction;
    private bool isMoving;
    private Transform mainCameraTransform;

    [Header("Animation Velocity")]
    [SerializeField] private float acceleration = 1f; // higher value means faster acceleration.
    [SerializeField] private float deceleration = 3f; // higher value means faster deceleration. Should be higher than acceleration to feel responsive when stopping.

    [SerializeField] private float playerVelocityZ;
    public float PlayerVelocityZ {
        get => playerVelocityZ;
        private set {
            playerVelocityZ = value;
            PlayerAnimationController.Instance.UpdateMovementInput(value);
        }
    } // value between -1 and 3.

    private Coroutine velocityCoroutine; // Reference to the currently running velocity coroutine
    private Coroutine moveCoroutine;

    private Vector2 moveInput;

    private void OnEnable() {
        // TODO : Move this to a better place, GameManager
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //

        this.moveInputAction = this.moveAction.action;
        moveInputAction.Enable();
        moveInputAction.performed += OnMove;
        moveInputAction.canceled += OnMove;

        this.lookInputAction = this.lookAction.action;
        this.lookInputAction.Enable();
        this.lookInputAction.performed += OnLook;

        this.leftMouseInputAction = this.leftMouseAction.action;
        this.leftMouseInputAction.Enable();
        this.leftMouseInputAction.performed += OnMouseAction;

    }
    private void OnDisable() {
        moveInputAction.Disable();
        moveInputAction.performed -= OnMove;
        moveInputAction.canceled -= OnMove;

        this.lookInputAction.Disable();
        this.lookInputAction.performed -= OnLook;

        this.leftMouseInputAction.Disable();
        this.leftMouseInputAction.performed -= OnMouseAction;
    }


    private void Start() {
        this.mainCameraTransform = Camera.main.transform;
    }

    private void FixedUpdate() {
        if (!this.controller.isGrounded && !PlayerAnimationController.Instance.IsClimbingLadder) {
            this.controller.Move(new Vector3(0, -4f, 0) * Time.fixedDeltaTime);
        }
    }

    private void OnMove(InputAction.CallbackContext context) {

        if (context.performed) {
            this.moveInput = this.moveInputAction.ReadValue<Vector2>();
            if (!this.isMoving) {
                this.isMoving = true;
                this.moveCoroutine = StartCoroutine(PerformeMove());
                if (this.velocityCoroutine == null) this.velocityCoroutine = StartCoroutine(UpdateVelocity());

            } else {
                // Direction have changed, without letting go of the movement input.
                StopCoroutine(this.moveCoroutine);
                this.moveCoroutine = StartCoroutine(PerformeMove());
                if (this.velocityCoroutine == null) this.velocityCoroutine = StartCoroutine(UpdateVelocity());
            }
        } else if (context.canceled) {
            this.isMoving = false;
        }
    }
    private void OnLook(InputAction.CallbackContext context) {
        if (!context.performed) return;
        if (PlayerAnimationController.Instance.IsAimingWaterHose) {
            Vector2 lookInput = this.lookInputAction.ReadValue<Vector2>();
            Vector2 currentAim = PlayerAnimationController.Instance.WaterHoseAim;
            lookInput *= 0.001f;
            PlayerAnimationController.Instance.UpdateWaterHoseAimHorizontal(currentAim.x + lookInput.x, currentAim.y + lookInput.y);
        }
    }


    private void OnMouseAction(InputAction.CallbackContext context) {
        if (!context.performed) return;
        if (PlayerAnimationController.Instance.currentInteraction == null) return;

        if (PlayerAnimationController.Instance.currentInteraction.InteractionType == EnumInteractionType.Axe) {
            PlayerAnimationController.Instance.TriggerAxeStrike();
        }
    }

    private IEnumerator PerformeMove() {
        //Vector3 moveDirection = this.mainCameraTransform.right * moveInput.x + this.mainCameraTransform.forward * moveInput.y;
        Vector3 moveDirection = this.transform.right * moveInput.x + this.transform.forward * moveInput.y;
        moveDirection.y = 0; // Keep the movement on the horizontal plane

        if (PlayerAnimationController.Instance.IsClimbingLadder) {

            if (this.moveInput.y > 0) {
                PlayerAnimationController.Instance.TriggerLadderClimb(true);
            } else if (this.moveInput.y < 0) {
                PlayerAnimationController.Instance.TriggerLadderClimb(false);
            } else {
                PlayerAnimationController.Instance.TriggerIdleLadder();
            }
        }


        while (this.isMoving) {
            // This will give a multiplier between 1 and 2 for forward movement, and between 0.5 and 1 for backward movement.
            float dynamicSpeedMultiplier = 1f + (Mathf.Abs(this.PlayerVelocityZ / 2f));
            Vector3 motion = (this.speed * dynamicSpeedMultiplier) * Time.deltaTime * moveDirection;

            if (PlayerAnimationController.Instance.IsClimbingLadder) {
                //Vector3 ladderClimbDirection = PlayerAnimationController.Instance.currentInteraction.GetLadderClimbDirection();
                //this.controller.Move(ladderClimbDirection * Time.deltaTime);
                this.controller.Move(new Vector3(0, motion.z, 0));
            } else {

                if (!(moveInput.y < 0)) {
                    this.controller.Move(motion);
                    this.controller.gameObject.transform.rotation = Quaternion.Slerp(this.controller.gameObject.transform.rotation, Quaternion.LookRotation(moveDirection), Time.fixedDeltaTime * 2f);
                } else if (moveInput.y < 0) {
                    this.controller.Move(motion * 0.5f);
                    this.controller.gameObject.transform.rotation = Quaternion.Slerp(this.controller.gameObject.transform.rotation, Quaternion.LookRotation(-moveDirection), Time.fixedDeltaTime * 2f);
                }
            }
            yield return null;
        }
        if (PlayerAnimationController.Instance.IsClimbingLadder) {
            PlayerAnimationController.Instance.TriggerIdleLadder();
        }
        this.moveCoroutine = null;
        yield return null;
    }

    private IEnumerator UpdateVelocity() {
        float target = 0f;
        float newVelocity = 0f;

        // We want this loop to run while the player is moving OR while velocity hasn't settled back to 0
        while (this.isMoving || Mathf.Abs(this.PlayerVelocityZ) > 0.01f) {

            if (!this.isMoving) {
                target = 0f; // Not moving, target velocity is 0
            } else if (moveInput.y < 0) {
                target = -1f; // Walking backwards

            } else if (moveInput.y > 0 || moveInput.x != 0) {
                // Todo: Detect left shift for running, and set target to 3f for running.
                //target = 1f; // Walking forward
                target = 3f; // Running
            }

            float absVelocity = Mathf.Abs(this.PlayerVelocityZ);
            float absTarget = Mathf.Abs(target);

            if (absVelocity < absTarget) { //Accelerate
                if (this.PlayerVelocityZ > target) {
                    // accrelerateing to a more negative number, backwards momentum
                    newVelocity = this.PlayerVelocityZ - Time.deltaTime * this.acceleration;
                    if (newVelocity < target) newVelocity = target; // Prevent overshooting
                } else {
                    newVelocity = this.PlayerVelocityZ + Time.deltaTime * this.acceleration;
                    if (newVelocity > target) newVelocity = target; // Prevent overshooting
                }

                //Debug.Log($"Accelerating. Velocity: {this.PlayerVelocityZ}, Target: {target}, New Velocity: {newVelocity}");

            } else if (absVelocity > absTarget) { // Decelerate
                if (this.PlayerVelocityZ < target) {
                    // decelerating to a more positive number, stopping backwards momentum
                    newVelocity = this.PlayerVelocityZ + Time.deltaTime * this.deceleration;
                    if (newVelocity > target) newVelocity = target; // Prevent overshooting
                } else {
                    newVelocity = this.PlayerVelocityZ - Time.deltaTime * this.deceleration;
                    if (newVelocity < target) newVelocity = target; // Prevent overshooting
                }
            }

            newVelocity = Mathf.Round(newVelocity * 1000f) / 1000f; // round to 3 decimals

            // If we are close to 0 and not moving, snap to 0 to properly finish the loop
            if (!this.isMoving && Mathf.Abs(newVelocity) < 0.0019f) {
                this.PlayerVelocityZ = 0f;
            } else {
                this.PlayerVelocityZ = newVelocity;
            }

            yield return null;
        }
        this.velocityCoroutine = null;
    }

    public void Teleport(Vector3 position, Quaternion rotation) {
        this.controller.enabled = false;
        this.controller.transform.position = position;
        this.controller.transform.rotation = rotation;
        this.controller.enabled = true;
        this.controller.Move(Vector3.zero);
    }
}
