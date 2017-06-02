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

	public static int Solution2Equation(double a, double b, double c, double[] solution)
	{
		double delt = b * b - 4 * a * c;
		if (delt >= 0)
		{
			if (a > 1e-10)
			{
				solution[0] = (-b + System.Math.Sqrt(delt)) / (2 * a);
				solution[1] = (-b - System.Math.Sqrt(delt)) / (2 * a);

			}
			else
			{
				solution[0] = (2 * c) / (-b + System.Math.Sqrt(delt));
				solution[1] = (2 * c) / (-b - System.Math.Sqrt(delt));
			}
			return 2;
		}
		else
		{
			return 0;
		}
	}
}
