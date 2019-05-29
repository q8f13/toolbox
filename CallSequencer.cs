using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CallSequencer {
	private Dictionary<float, UnityAction> _callbacks;

	private float _duration;
	private float _currTimer = 0.0f;
	private float _currTimerLastTick = 0.0f;
	private bool _end = false;

	private bool _liveProtect = false;

	private List<float> _dirtyCalls;

	public CallSequencer(float time, bool liveProtect)
	{
		_duration = time;
		_liveProtect = liveProtect;
	}

	public bool IsPlaying
	{
		get
		{
			return _currTimer > 0;
		}
	}

	public void Append(float time, UnityAction call)
	{
		if(IsPlaying && _liveProtect)
			return;

		if(_callbacks == null)
			_callbacks = new Dictionary<float, UnityAction>();

		_callbacks.Add(time, call);

		if(IsPlaying)
		{
			if(_dirtyCalls == null)
				_dirtyCalls = new List<float>();

			_dirtyCalls.Add(time);
		}
	}

	public void Reset()
	{
		_currTimer = 0.0f;
		foreach(float t in _dirtyCalls)
		{
			_callbacks.Remove(t);
		}

		_end = false;
	}

	public void Tick(float dt)
	{
		if(_end)
			return;

		_currTimer += dt;
		_currTimer = Mathf.Min(_currTimer, _duration);

		if(_currTimer == _duration && _currTimer == _currTimerLastTick)
		{
			_end = true;
			return;
		}

		if(_currTimer > _currTimerLastTick)
		{
			foreach(KeyValuePair<float, UnityAction> call in _callbacks)
			{
				if(_currTimer > call.Key && _currTimerLastTick < call.Key)
				{
					call.Value();
					break;
				}
			}

			_currTimerLastTick = _currTimer;
		}
	}
}
