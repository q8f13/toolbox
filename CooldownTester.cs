using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

/// <summary>
/// Test case for CooldownManager
/// </summary>
public class CooldownTester : MonoBehaviour
{
	private static Dictionary<string, int> PRESET_ITEMS = new Dictionary<string, int>()
	{
		{"aaa", 15},
		{"bbb", 6},
		{"ccc", 9},
		{"ddd", 2},
		{"eee", 4},
		{"fff", 5}
	};

	private List<string> _trackingKeys = new List<string>();
	private string _nextRandomizedKey = null;

	private static string[] PRESET_KEYS = null;

	private CooldownManager _manager;

	// Use this for initialization
	void Start ()
	{
		_manager = gameObject.AddComponent<CooldownManager>();
		
		PRESET_KEYS = PRESET_ITEMS.Keys.ToArray();
		_nextRandomizedKey = GetRandomKey();
	}

	string GetRandomKey()
	{
		int rnd = Random.Range(0, PRESET_ITEMS.Count);
		return PRESET_KEYS[rnd];
	}
	
	void OnGUI()
	{
		if (GUI.Button(new Rect(0, 0, 400, 80), string.Format("add {0}", _nextRandomizedKey)))
		{
			if (_manager.TrackCooldown(_nextRandomizedKey, PRESET_ITEMS[_nextRandomizedKey], CooldownCompleteHint))
			{
				_trackingKeys.Add(_nextRandomizedKey);
			}
			_nextRandomizedKey = GetRandomKey();
		}

		for (int i = 0; i < _trackingKeys.Count; i++)
		{
			GUI.Label(new Rect(0, 200 + 40*i, 400, 90), string.Format("{0} : {1:f1}"
				, _trackingKeys[i]
				, _manager.GetCooldownTimeLeft(_trackingKeys[i])));
		}
	}

	void CooldownCompleteHint(string key)
	{
		Debug.Log(string.Format("cooldown of =========>{0}<======== is ready!", key));
		_trackingKeys.Remove(key);
	}
}
