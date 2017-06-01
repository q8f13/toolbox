using UnityEngine;
using System.Collections;

/// <summary>
/// static utility functions
/// </summary>

public class qfUtility {

	public static T GetComponentByPath<T>(Transform parent, string path) where T : Component
	{
		T result = null;
	
		Transform target = parent.FindChild(path);
		if (target == null)
		{
			Debug.LogError(string.Format("cannot get child by path: {0}", path));
			return result;
		}

		result = target.GetComponent<T>();

		return result;
	}

	public static T CreateInstance<T>(GameObject prefab, Transform parent = null) where T : Component
	{
		T instance = null;

		GameObject go = GameObject.Instantiate(prefab);
		go.transform.SetParent(parent);
		go.transform.localPosition = Vector3.zero;
		go.transform.localScale = Vector3.one;
		go.transform.localRotation = Quaternion.identity;
		instance = go.GetComponent<T>();

		return instance;
	}
}
