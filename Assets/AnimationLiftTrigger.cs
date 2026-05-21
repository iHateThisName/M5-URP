using UnityEngine;

public class AnimationLiftTrigger : StateMachineBehaviour {
    [SerializeField] private bool isAxe = true; // if false should be water hose.
    [SerializeField] private bool isPickUp = true; // if false should be drop.
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (this.isPickUp) {
            if (isAxe) {
                PlayerAnimationController.Instance.TriggerAxeCarry();
            } else {
                PlayerAnimationController.Instance.TriggerWaterHoseCarry();
            }
        } else {
            if (isAxe) {
                PlayerAnimationController.Instance.OnAxeStrikeFinished();
            }
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
