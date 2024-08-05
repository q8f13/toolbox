using System;
using UnityEngine;

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
	/// 从0开始生成一串不重复的整型数字
	/// 基于Fisher and Yates算法， Durstenfeld's version
	/// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
	/// </summary>
	/// <param name="count">要生成的数量</param>
	/// <returns></returns>
	public static int[] GetRandomUniqueIntegers(int count)
	{
		Debug.Assert(count > 1, "count should above 1");

		int[] arr = new int[count];
		int c = count;
		while (--c >= 0)
		{
			arr[c] = c;
		}

		int n = count - 1;
		while (n >= 0)
		{
			int rnd = UnityEngine.Random.Range(0, n);
			if (rnd == n)
			{
				n--;
				continue;
			}
			int swap = arr[n];
			arr[n] = arr[rnd];
			arr[rnd] = swap;
			n--;
		}

		return arr;
	}

	/// <summary>
	/// 四元数指数计算
	/// 可用来算取百分比后的某个角位移的四元数是多少
	/// </summary>
	public static Quaternion QuaternionExp(Quaternion q, float exponent)
	{
		if(Mathf.Abs(q.w) < 0.9999f)
		{
			float alpha = Mathf.Acos(q.w);
			float new_alpha = alpha * exponent;
			float w = Mathf.Cos(new_alpha);
			float multi = Mathf.Sin(new_alpha) / Mathf.Sin(alpha);

			q.x*=multi;
			q.y*=multi;
			q.z*=multi;
		}

		return q;
	}

	// number damper
	// https://theorangeduck.com/page/spring-roll-call#damper
	public static float DamperExponential(
        float x,
        float g, 
        float damping, 
        float dt, 
        float ft = 1.0f / 60.0f)
    {
        return Mathf.Lerp(x, g, 1.0f - Mathf.Pow(1.0f / (1.0f - ft * damping), -dt / ft));
    }
}
