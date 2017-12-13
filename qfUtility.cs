using UnityEngine;
using System.Collections;

/// <summary>
/// static utility functions
/// </summary>

public class qfUtility {

	public static T GetComponentByPath<T>(Transform parent, string path) where T : Component
	{
		T result = null;
	
		Transform target = parent.Find(path);
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

	public static void DrawLineArrow(Vector3 from, Vector3 to, int count = 1)
	{
		float distanceStep = (to - from).magnitude/(count + 1);
		Vector3 dir = (to - from).normalized;
		int idx = 1;
		Gizmos.DrawLine(from, to);
		int failsafe = 999;
		while (idx <= count)
		{
			Vector3 p = from + dir*distanceStep*idx;
			Gizmos.DrawRay(p, Quaternion.Euler(0,30,0) * -dir);
			Gizmos.DrawRay(p, Quaternion.Euler(0,-30,0) * -dir);
			idx++;
			failsafe--;
			if (failsafe < 0)
			{
				throw new System.Exception("failsafe");
			}
		}
	}
}
