using Assets.Scripts.Singleton;
using System;
using UnityEngine;

public class PlayerAnimationController : Singleton<PlayerAnimationController> {
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationLiftTrigger animationLiftTrigger;

    public bool IsArmsOverwritte { get; private set; } = false;
    public bool IsAimingWaterHose { get; private set; } = false;
    public bool IsClimbingLadder { get; set; } = false;
    public Vector2 WaterHoseAim { get; private set; } = Vector2.zero;

    public InteractionController currentInteraction = null;

    protected override void Awake() {
        base.Awake();
        if (this.animator == null) {
            this.animator = transform.root.GetComponentInChildren<Animator>();
        }

        if (this.animationLiftTrigger == null) {
            this.animationLiftTrigger = this.animator.GetBehaviour<AnimationLiftTrigger>();
        }

    }

    private void OnEnable() {
        CameraManager.OnAimStateChanged += HandleAimStateChanged;
    }

    private void OnDisable() {
        CameraManager.OnAimStateChanged -= HandleAimStateChanged;
    }

    private void HandleAimStateChanged(bool isAiming) {
        //if (!this.IsAimingWaterHose || (!isAiming && this.IsAimingWaterHose)) return;

        if (isAiming) {
            this.animator.SetTrigger(AnimationState.AimWaterHoseTrigger);
            this.IsAimingWaterHose = true;

        } else if (!isAiming) {
            this.animator.SetTrigger(AnimationState.CarryWaterHoseTrigger);
            this.IsAimingWaterHose = false;

            // Reset the water hose aim parameters when stopping aiming
            this.WaterHoseAim = Vector2.zero;
            this.animator.SetFloat(AnimationState.AimWaterHoseHorizontal, 0f);
            this.animator.SetFloat(AnimationState.AimWaterHoseVertical, 0f);
        }
    }

    public void UpdateMovementInput(float newVelocity) {
        this.animator.SetFloat(AnimationState.MovementVelocity, newVelocity);
    }

    public void UpdateWaterHoseAimHorizontal(float horizontal, float vertical) {
        if (!this.IsAimingWaterHose) return;
        horizontal = Mathf.Clamp(horizontal, -1f, 1f);
        vertical = Mathf.Clamp(vertical, -1f, 1f);

        this.WaterHoseAim = new Vector2(horizontal, vertical);
        this.animator.SetFloat(AnimationState.AimWaterHoseHorizontal, horizontal);
        this.animator.SetFloat(AnimationState.AimWaterHoseVertical, vertical);
    }

    [ContextMenu("Pick Up Axe")]
    public void PickUpAxe() {
        FireKittenModelController.Instance.HideTools();
        if (this.currentInteraction != null) this.currentInteraction.Drop();
        this.animator.SetTrigger(AnimationState.LiftAxeTrigger);
    }

    [ContextMenu("Drop Axe")]
    public void DropAxe() {
        this.animator.SetTrigger(AnimationState.LiftAxeTrigger);

        this.IsArmsOverwritte = false;
        this.animator.SetBool(AnimationState.ArmsOverwritte, false);
        FireKittenModelController.Instance.HideAxe();
    }

    [ContextMenu("Pick Up Water Hose")]
    public void PickUpWaterHose() {
        FireKittenModelController.Instance.HideTools();
        if (this.currentInteraction != null) this.currentInteraction.Drop();
        this.animator.SetTrigger(AnimationState.LiftWaterHoseTrigger);
    }

    [ContextMenu("Drop Water Hose")]
    public void DropWaterHose() {
        this.animator.SetTrigger(AnimationState.LiftWaterHoseTrigger);
        this.IsArmsOverwritte = false;
        this.animator.SetBool(AnimationState.ArmsOverwritte, false);
        FireKittenModelController.Instance.HideWaterHoseNosal();
    }

    public void TriggerAxeCarry() {
        this.IsArmsOverwritte = true;
        this.animator.SetBool(AnimationState.ArmsOverwritte, true);
        this.animator.SetTrigger(AnimationState.CarryAxeTrigger);
        FireKittenModelController.Instance.ShowAxe();
    }

    public void TriggerWaterHoseCarry() {
        this.IsArmsOverwritte = true;
        this.animator.SetBool(AnimationState.ArmsOverwritte, true);
        this.animator.SetTrigger(AnimationState.CarryWaterHoseTrigger);
        FireKittenModelController.Instance.ShowWaterHoseNosal();
    }

    public void AttacheToLadder() {
        FireKittenModelController.Instance.HideTools();
        if (currentInteraction != null) this.currentInteraction.Drop();
        if (this.IsArmsOverwritte) {
            this.animator.SetBool(AnimationState.ArmsOverwritte, false);
            this.IsArmsOverwritte = false;
        }
        this.IsClimbingLadder = true;
        this.animator.SetTrigger(AnimationState.IdleLadderTrigger);
    }

    public void DetachFromLadder() {
        this.IsClimbingLadder = false;
        this.animator.SetTrigger(AnimationState.deafultTrigger);
    }

    public void TriggerIdleLadder() => this.animator.SetTrigger(AnimationState.IdleLadderTrigger);

    public void TriggerLadderClimb(bool isUp) {
        if (isUp) {
            this.animator.SetTrigger(AnimationState.LadderClimbUpTrigger);
        } else {
            this.animator.SetTrigger(AnimationState.LaderClimbDownTrigger);
        }
    }

    public void TriggerAxeStrike() { 
        this.animator.SetBool(AnimationState.ArmsOverwritte, false);
        this.animator.SetTrigger(AnimationState.AxeSwingTrigger); 
    }

    public void OnAxeStrikeFinished() {
        this.animator.SetBool(AnimationState.ArmsOverwritte, true);
        this.animator.SetTrigger(AnimationState.CarryAxeTrigger);
    }
    

    public class AnimationState {
        // Main States
        public static readonly int LiftAxeTrigger = Animator.StringToHash("Lift Trigger Axe");
        public static readonly int LiftWaterHoseTrigger = Animator.StringToHash("Lift Trigger Water Hose");
        public static readonly int AxeSwingTrigger = Animator.StringToHash("Axe Swing Trigger");
        public static readonly int deafultTrigger = Animator.StringToHash("Default Trigger");
        public static readonly int LadderClimbUpTrigger = Animator.StringToHash("Ladder Climb Up Trigger");
        public static readonly int LaderClimbDownTrigger = Animator.StringToHash("Ladder Climb Down Trigger");
        public static readonly int IdleLadderTrigger = Animator.StringToHash("Ladder Climb Stop Trigger");

        // Movement Blend Tree
        public static readonly int MovementVelocity = Animator.StringToHash("Movement Blend"); // 3 running, 1 walking, 0 Idle, -1 Backwards.

        // Arms Mask
        public static readonly int ArmsOverwritte = Animator.StringToHash("Apply Arms Overwrite"); // Enter/Exit condition, Boolean value to apply the arms overwrite layer. Needs to be true to play any of the arm animations. When false will stop looping arms animation.
        public static readonly int AimWaterHoseTrigger = Animator.StringToHash("Spray Hose Aim Trigger"); // Trigger to start aiming the water hose, starting the aiming blend tree.
        public static readonly int CarryWaterHoseTrigger = Animator.StringToHash("Carry Hose Trigger"); // Trigger to start carrying the water hose.
        public static readonly int CarryAxeTrigger = Animator.StringToHash("Axe Carry Trigger"); // Trigger to start carrying the axe.

        // Arms Mask, Water Hose Aiming Blend Tree
        public static readonly int AimWaterHoseHorizontal = Animator.StringToHash("Spray X"); // Horizontal input for aiming the water hose
        public static readonly int AimWaterHoseVertical = Animator.StringToHash("Spray Y"); // Vertical input for aiming the water hose

    }
}
