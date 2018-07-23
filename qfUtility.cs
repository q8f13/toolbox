using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// static utility functions
/// author: q8f13
/// repo: https://github.com/q8f13/toolbox
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

	/// <summary>
	/// 解一元二次方程
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="c"></param>
	/// <param name="solution"></param>
	/// <returns></returns>
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

	/// <summary>
	/// 计算预瞄点
	/// </summary>
	/// <param name="targetSpeed">目标即时速度</param>
	/// <param name="bulletSpeed">投射物速度</param>
	/// <param name="targetPosW">目标当前世界空间坐标</param>
	/// <param name="shooterPosW">射击口当前世界空间坐标</param>
	/// <param name="targetDirection">目标即使朝向</param>
	/// <returns></returns>
	public static Vector3 GetPredictAimingPosition(float targetSpeed
												, float bulletSpeed
												, Vector3 targetPosW
												, Vector3 shooterPosW
												, Vector3 targetDirection)
	{
		float t = -1.0f;

		// 求出夹角的cos
		float cos_c = Vector3.Dot((targetPosW - shooterPosW).normalized, targetDirection);

		float t2s_d = (targetPosW - shooterPosW).magnitude;

		/*	步骤
			 target_to_self : A
			 target_to_prev : B
			 self_to_prev : C
			 A = target_to_self_distant
			 C = BulletSpeed * t;
			 B = TargetSpeed * t;
			 2ABCos_c = 2(TargetSpeed*t)*target_to_self_distant * cos_c
			 A^2 + B^2 - 2ABCos_c = C^2
			 target_to_self_d^2 + (TargetSpeed*t)^2 - 2*(target_to_self_d)*(TargetSpeed*t)*cos_c = (BulletSpeed * t)^2
			 target_to_self_d^2 + TargetSpeed^2 * t^2 - 2*(target_to_self_d)*(TargetSpeed*t)*cos_c = BulletSpeed^2 * t^2;
			 TargetSpeed^2 * t^2 - BulletSpeed^2 * t^2 - 2*target_to_self_d*TargetSpeed*cos_c * t + target_to_self_d^2 = 0
			 (TargetSpeed^2 - BulletSpeed^2) * t^2 - (2 * target_to_self_d * TargetSpeed * cos_c) * t + target_to_self_d^2 = 0
			
			// 解一元二次方程，保留正数求出t
			// ax^2 + bx + c = 0
			// x = (-b +(-) sqrt(b^2 - 4ac)) / 2a
		*/

		double a = targetSpeed*targetSpeed - bulletSpeed*bulletSpeed;
		double b = -2.0f*targetSpeed*t2s_d*cos_c;
		double c = t2s_d*t2s_d;

		double[] result = new double[2];
		int count = Solution2Equation(a, b, c, result);

		if (count < 0)
		{
			throw new Exception("delta 's below 0");
		}
		else
		{
			t = (float) (Math.Max(result[0], result[1]));
		}

		Debug.Assert(t >= 0.0f, "t should above 0");

		return targetPosW + targetDirection*targetSpeed*t;
	}

	/// <summary>
	/// 小工具，延时调用方法的管理
	/// </summary>
	public class DelayCallbackManager
	{
		private readonly MonoBehaviour _base;
		private HashSet<int> _cache;

		public bool Verbose = false;

		public DelayCallbackManager(MonoBehaviour target)
		{
			_base = target;
			_cache = new HashSet<int>();
		}

		public void DelayCall(UnityAction action, float delayInSec)
		{
			int hashCode = action.GetHashCode();
			if (_cache.Contains(hashCode))
			{
				Debug.LogError(string.Format("delay call {0} already running", hashCode));
				return;
			}

			_base.StartCoroutine(DelayCall_Co(action, delayInSec));
			_cache.Add(hashCode);

			if (Verbose)
				Debug.Log(string.Format("delay call {0} started in {1} secs", hashCode, delayInSec));
		}

		private IEnumerator DelayCall_Co(UnityAction action, float delayInSec)
		{
			yield return new WaitForSeconds(delayInSec);
			action();
			int hashCode = action.GetHashCode();
			_cache.Remove(hashCode);
			if (Verbose)
				Debug.Log(string.Format("delay call {0} cleared", hashCode));
		}
	}

	/// <summary>
	/// 让scrollView的content自动适应尺寸
	/// </summary>
	/// <param name="contentRt"></param>
	/// <param name="gap"></param>
	/// <param name="isVertical"></param>
	/// <returns></returns>
	public static float AutoAdjustForScrollView(RectTransform contentRt, float gap = 0.0f, bool isVertical = true)
	{
		// boundary issues
		if (contentRt.childCount == 0)
			return 0.0f;

		float result = 0.0f;

		// accumulate direct childs height
		RectTransform rtChild = null;
		int directChildCount = 0;
		for (int i = 0; i < contentRt.childCount; i++)
		{
			if(contentRt.GetChild(i).parent == contentRt.transform)
			{
				rtChild = contentRt.GetChild(0).GetComponent<RectTransform>();
				result += isVertical ? rtChild.sizeDelta.y : rtChild.sizeDelta.x;
				directChildCount++;
			}
		}

		// get gaps
		if(gap > 0.0f)
			result += gap*(directChildCount - 1);

		Vector2 sd = contentRt.sizeDelta;
		if(isVertical)
			sd.y = result;
		else
			sd.x = result;

		contentRt.sizeDelta = sd;

		return result;
	}

	/// <summary>
	/// 将世界空间坐标转换到屏幕空间的时候，有可能因为物体与相机的朝向问题
	/// 导致距离在超出一定范围后出现反转，x = -x, y = -y 类似这种的情况
	/// 这个方法是先将物体位置与屏幕空间计算，保持将物体位置信息转算为在相机前方
	/// 注意这个方法返回值仍然是世界空间坐标
	/// </summary>
	/// <param name="position">目标世界空间位置</param>
	/// <param name="cam">所用相机</param>
	/// <returns>转算后的目标世界空间位置</returns>
	public static Vector3 CalculateWorldPositionAsCamera(Vector3 position, Camera cam)
	{
		//if the point is behind the camera then project it onto the camera plane
		Vector3 camNormal = cam.transform.forward;
		Vector3 vectorFromCam = position - cam.transform.position;
		float camNormDot = Vector3.Dot(camNormal, vectorFromCam.normalized);
		if (camNormDot <= 0f)
		{
			//we are beind the camera, project the position on the camera plane
			float camDot = Vector3.Dot(camNormal, vectorFromCam);
			Vector3 proj = (camNormal * camDot * 1.01f);   //small epsilon to keep the position infront of the camera
			position = cam.transform.position + (vectorFromCam - proj);
		}

		return position;
	}

	/// <summary>
	/// x^2 / a^2 + y^2 / b^2 = 1
	/// </summary>
	/// <param name="isHorizon"></param>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static float[] GetEllipseBound(bool forHorizon, float a, float b, float value, Vector2 offset)
	{
		float[] bound = new float[2];
		// x^2 / a^2 + y^2 / b^2 = 1
		// x^2 = (- y^2 / b^2 + 1) * a^2
		if (forHorizon)
		{
			float x_square = (1 - (value*value)/(b*b))*(a*a);
			bound[0] = -Mathf.Sqrt(x_square) + offset.x;
			bound[1] = Mathf.Sqrt(x_square) + offset.x;
		}
		// y^2 = (- y^2 / a^2 + 1) * b^2
		else
		{
			float y_square = (1 - (value*value)/(a*a))*(b*b);
			bound[0] = -Mathf.Sqrt(y_square) + offset.y;
			bound[1] = Mathf.Sqrt(y_square) + offset.y;
		}

		return bound;
	}
}
