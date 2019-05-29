using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// cooldown event manager
/// track item by key
/// dispatch 'complete' via delegate
/// </summary>
public class CooldownManager : MonoBehaviour
{
	private static CooldownManager _instance = null;
	public static CooldownManager Instance { get { return _instance; } }
	
	private Dictionary<string, CooldownItem> _cooldownPool = new Dictionary<string, CooldownItem>();

	#region UnityEvents
	void Awake()
	{
		_instance = this;
	}

	void OnDestroy()
	{
		foreach (KeyValuePair<string, CooldownItem> item in _cooldownPool)
		{
			item.Value=null;
			_cooldownPool.Clear();
		}
	}

	void Update()
	{
		foreach (KeyValuePair<string, CooldownItem> item in _cooldownPool)
		{
			item.Value.Update(Time.deltaTime);
		}
	}

	#endregion

	public bool TrackCooldown(string key, float time, CooldownItem.OnCooldownReadyEvent readyHandler)
	{
		if (!_cooldownPool.ContainsKey(key))
		{
			CooldownItem tracker = new CooldownItem(key);
			tracker.OnCooldownReadyTrigger = readyHandler;
			_cooldownPool.Add(key, tracker);
		}

		bool success = _cooldownPool[key].TryStartCounting(time);

		if(!success)
			Debug.LogError(string.Format("fail to track cooldown of key {0}", key));

		return success;
	}

	public bool IsReady(string key)
	{
		if (!_cooldownPool.ContainsKey(key))
		{
			Debug.LogError(string.Format("no tracking item key mapped as {0}", key));
			return false;
		}

		return _cooldownPool[key].IsReady();
	}

	public float GetCooldownTimeLeft(string key)
	{
		if (!_cooldownPool.ContainsKey(key))
		{
			Debug.LogError(string.Format("no tracking item key mapped as {0}", key));
			return -1.0f;
		}

		return _cooldownPool[key].Timer;
	}
}

/// <summary>
/// cooldown item data structure
/// container for cooldown tracking
/// </summary>
public class CooldownItem
{
	public string Key;
	public float Timer = 0.0f;

	public delegate void OnCooldownReadyEvent(string key);
	public OnCooldownReadyEvent OnCooldownReadyTrigger;

	public CooldownItem(string key)
	{
		Key = key;
	}

	/// <summary>
	/// start a item cooldown tracking by key for the constructor
	/// </summary>
	/// <param name="time"></param>
	/// <returns>is tracking succeed</returns>
	public bool TryStartCounting(float time)
	{
		if (!IsReady())
			return false;

		Timer = time;
		return true;
	}

	public bool IsReady()
	{
		return Timer <= 0.0f;
	}

	/// <summary>
	/// refresh time counting
	/// </summary>
	/// <param name="deltaTime"></param>
	public void Update(float deltaTime)
	{
		if (Timer > 0.0f)
		{
			Timer -= deltaTime;
			if (Timer <= 0.0f && OnCooldownReadyTrigger != null)
				OnCooldownReadyTrigger(Key);
		}
	}
}
