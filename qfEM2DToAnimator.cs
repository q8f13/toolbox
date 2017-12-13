#if SEALED
using EasyMotion2D;
using UnityEngine;
using System.Collections;

/// <summary>
/// EasyMotion2D spriteAnimationClip to state in animator wrapper
/// attach this script on every state in controller
/// UNDONE
/// </summary>

public class qfEM2DToAnimator : StateMachineBehaviour
{
	public SpriteAnimationClip Clip;

	private SpriteAnimation _animation;
	private SpriteAnimationState _state;

	private float _startTime;
	private float _length;
	private bool _isLoop = false;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if(_animation == null)
			_animation = animator.GetComponent<SpriteAnimation>();

		_animation.StopAll();
		_animation.Play(Clip.name);
		_animation.CalculateData();
		_startTime = Time.time;
		if(_state == null)
			_state = _animation[Clip.name];

		_length = _animation[Clip.name].length*(_state.wrapMode == WrapMode.PingPong ? 2 : 1);
		_isLoop = _state.wrapMode == WrapMode.Loop || _state.wrapMode == WrapMode.PingPong;
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (Time.time - _startTime >= _length)
		{
			if (_isLoop)
			{
				animator.Play(stateInfo.shortNameHash,0, 0f);
				_startTime = Time.time;
			}
			else
				animator.Play(animator.GetNextAnimatorStateInfo(0).shortNameHash);
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
//	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
//	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}


}
#endif
