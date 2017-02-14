using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.Director;

public class qfCancelActions : StateMachineBehaviour
{
	public CancelAnimation[] CancelAction;

	public const int FRAME_SAMPLE = 36;

//	private bool _IsInWindow = false;

	private int _currentFrame = 0;

	private string _triggerBuffered = null;
	private KeyCode _keyBuffered = KeyCode.None;

	private Animator _anim = null;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);

		_anim = animator;
		animator.GetComponent<nsActor>().CurrentCancelAction = this;
	}

	public bool TryBufferCancelAction(KeyCode key, string triggerName)
	{
		for (int i = 0; i < CancelAction.Length; i++)
		{
			if(CancelAction[i].Method != CancelAnimation.CancelMethod.Buffer)
				continue;
			if (CancelAction[i].Key == key && CancelAction[i].IsInWindow(_currentFrame))
			{
				_keyBuffered = CancelAction[i].Key;
				_triggerBuffered = triggerName;
				return true;
			}
		}

		return false;
	}

	public bool TryTriggerInstantAction(KeyCode key)
	{
		for (int i = 0; i < CancelAction.Length; i++)
		{
			if(CancelAction[i].Method != CancelAnimation.CancelMethod.Instant)
				continue;
			if (CancelAction[i].Key == key && CancelAction[i].IsInWindow(_currentFrame))
			{
				_anim.Play(CancelAction[i].Action,0, 0.0f);
				return true;
			}
		}

		return false;
	}

	public void ResetCancelBuffer()
	{
		_triggerBuffered = null;
		_keyBuffered = KeyCode.None;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		base.OnStateExit(animator, stateInfo, layerIndex, controller);
		animator.GetComponent<nsActor>().CurrentCancelAction = null;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex, controller);

//		_currentFrame =  Mathf.FloorToInt(stateInfo.normalizedTime*FRAME_SAMPLE);
		_currentFrame =  Mathf.FloorToInt(stateInfo.normalizedTime*stateInfo.length*FRAME_SAMPLE);

		if (_keyBuffered != KeyCode.None)
		{
			for (int i = 0; i < CancelAction.Length; i++)
			{
				// 先查有没有已经buffer过的输入键位
				bool keyBufferedSame = CancelAction[i].Key == _keyBuffered;
				bool methodIsInstant = CancelAction[i].Method == CancelAnimation.CancelMethod.Instant;
				bool isInWindow = CancelAction[i].IsInWindow(_currentFrame);
				if (keyBufferedSame && methodIsInstant && isInWindow)
				{
					animator.SetTrigger(_triggerBuffered);
					_triggerBuffered = null;
					_keyBuffered = KeyCode.None;
					break;
				}
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_currentFrame = 0;
		ResetCancelBuffer();
	}

}

[Serializable]
public class CancelAnimation
{
	public KeyCode Key;
	public CancelMethod Method;
	public string Action;

	public int On = -1;
	public int Off = -1;

	public enum CancelMethod
	{
		Instant = 0,
		Buffer,
	}

	public bool IsInWindow(int currentFrame)
	{
		if (On < 0 || Off < 0 || On > Off)
		{
			Debug.LogError("Cancel action frame has invalid settings");
			return false;
		}

		if (currentFrame >= On && currentFrame <= Off)
			return true;
		return false;
	}
}
