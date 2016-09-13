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
}
