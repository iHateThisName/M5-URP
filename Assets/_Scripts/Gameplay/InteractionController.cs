using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionController : MonoBehaviour {

    [Header("Refrences")]
    [SerializeField] private InputActionReference interactionRefrence;
    [SerializeField] private GameObject model;

    [Header("Interaction Settings")]
    [field: SerializeField] public EnumInteractionType InteractionType { get; set; }

    private InputAction interactionInputAction;
    private bool isPickedUp = false;
    [field: SerializeField] public bool IsPlayerInRange { get; private set; } = false;

    [field: SerializeField] public Transform StartTeleport { get; private set; }
    [field: SerializeField] public Transform EndTeleport { get; private set; }
    [field: SerializeField] public Transform LadderEndPoint { get; private set; }

    private void Start() {
        if (InteractionType == EnumInteractionType.None) {
            Debug.LogWarning("Interaction type is set to None, this component will not do anything.");
        }
        this.interactionInputAction = this.interactionRefrence.action;

    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            this.IsPlayerInRange = true;
            this.interactionInputAction.Enable();
            this.interactionInputAction.performed += OnInteraction;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            this.IsPlayerInRange = false;
            this.interactionInputAction.performed -= OnInteraction;
            this.interactionInputAction.Disable();

            if (this.InteractionType == EnumInteractionType.Ladder && isPickedUp) {
                PlayerAnimationController.Instance.DetachFromLadder();
                PlayerMovement.Instance.Teleport(EndTeleport.position, EndTeleport.rotation);
                isPickedUp = false;

            }
        }
    }

    private void OnInteraction(InputAction.CallbackContext context) {
        if (!context.performed) return;
        if (!this.IsPlayerInRange) return;

        switch (this.InteractionType) {
            case EnumInteractionType.Axe:
                if (!isPickedUp) {

                    PlayerAnimationController.Instance.PickUpAxe();
                    isPickedUp = true;
                }
                break;
            case EnumInteractionType.WaterHose:
                if (!isPickedUp) {
                    PlayerAnimationController.Instance.PickUpWaterHose();
                    isPickedUp = true;
                }
                break;
            case EnumInteractionType.Ladder:
                if (!isPickedUp) {
                    PlayerMovement.Instance.Teleport(StartTeleport.position, StartTeleport.rotation);
                    PlayerAnimationController.Instance.AttacheToLadder();
                    isPickedUp = true;
                }
                break;
            default:
                Debug.LogWarning("Interaction type is set to None, this component will not do anything.");
                break;
        }

        PlayerAnimationController.Instance.currentInteraction = this;
        if (this.InteractionType != EnumInteractionType.Ladder) {
            this.model.SetActive(false);
        }

    }

    public void Drop() {
        if (!isPickedUp) return;
        this.model.SetActive(true);
        isPickedUp = false;
    }

    public Vector3 GetLadderClimbDirection() {
        Vector3 ladderMoveDirection = this.LadderEndPoint.position - this.StartTeleport.position;
        return ladderMoveDirection.normalized;
    }

    public enum EnumInteractionType { None, Axe, WaterHose, Ladder, }
}