using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameEvent : ScriptableObject
{
	public string Description;

	private List<GameEventListener> _events = new List<GameEventListener>();

	public void Raise()
	{
		for(int i=0;i<_events.Count;i++)
			_events[i].OnEventRaised();
	}

	public void Register(GameEventListener listener)
	{
		Debug.Assert(!_events.Contains(listener), string.Format("target listener {0} already exist in list", listener.name));
		_events.Add(listener);
	}

	public void UnRegister(GameEventListener listener)
	{
		_events.Remove(listener);
	}
}
