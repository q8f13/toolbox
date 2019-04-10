/// <summary>
/// Spline Components and Editor extension from Unity official tutorial
/// https://unity3d.com/learn/tutorials/topics/scripting/creating-spline-tool
/// </summary>

using System.Collections.Generic;
using UnityEngine;

public class SplineComponent : MonoBehaviour, ISpline
{
    public int ControlPointCount { get { return points.Count; } }

	public bool closed = false;
    public List<Vector3> points = new List<Vector3>();
    public float? length;

    public float CurrentPointT = 0.0f;

    public Vector3 FindClosest(Vector3 worldPoint)
    {
        var smallestDelta = float.MaxValue;
        var step = 1f / 1024;
        var closestPoint = Vector3.zero;
        for (var i = 0; i <= 1024; i++)
        {
            var p = GetPoint(i * step);
            var delta = (worldPoint - p).sqrMagnitude;
            if (delta < smallestDelta)
            {
                closestPoint = p;
                smallestDelta = delta;
            }
        }
        return closestPoint;
    }

	/// <summary>
    /// Index is used to provide uniform point searching.
    /// </summary>
    SplineIndex uniformIndex;
    SplineIndex Index
    {
        get
        {
            if (uniformIndex == null) uniformIndex = new SplineIndex(this);
            return uniformIndex;
        }
    }

	void Reset() {
        points = new List<Vector3>() {
            Vector3.forward * 3,
            Vector3.forward * 6,
            Vector3.forward * 9,
            Vector3.forward * 12
        };
	}

	private void OnValidate() {
		if (uniformIndex != null)
			uniformIndex.ReIndex();	
	}
    
    public void ResetIndex()
    {
        uniformIndex = null;
        length = null;
    }
    
    public Vector3 GetPoint(float t) => Index.GetPoint(t);

    public Vector3 GetTangent(float t) => Index.GetTangent(t);

    public Vector3 GetNormal(float t, Vector3 up) => Index.GetNormal(t, up);

    public Vector3 GetBackward(float t)=> -GetForward(t);

    public Vector3 GetControlPoint(int index)
    {
		return points[index];
    }


    public Vector3 GetDistance(float distance)
    {
		if (length == null) length = GetLength();
        return uniformIndex.GetPoint(distance / length.Value);
    }


    public Vector3 GetDown(float t)=> -GetUp(t);


    public Vector3 GetForward(float t)
    {
        var A = GetPoint(t - 0.001f);
        var B = GetPoint(t + 0.001f);
        return (B - A).normalized;
    }

    public Vector3 GetLeft(float t) => -GetRight(t);

    public float GetLength(float step = 0.001f)
    {
        var D = 0f;
        var A = GetNonUniformPoint(0);
        for (var t = 0f; t < 1f; t += step)
        {
            var B = GetNonUniformPoint(t);
            var delta = (B - A);
            D += delta.magnitude;
            A = B;
        }
        return D;
    }


    public Vector3 GetNonUniformPoint(float t)
    {
        switch (points.Count)
        {
            case 0:
                return Vector3.zero;
            case 1:
                return transform.TransformPoint(points[0]);
            case 2:
                return transform.TransformPoint(Vector3.Lerp(points[0], points[1], t));
            case 3:
                return transform.TransformPoint(points[1]);
            default:
                return Hermite(t);
        }
    }

    public Vector3 GetRight(float t)
    {
        var A = GetPoint(t - 0.001f);
        var B = GetPoint(t + 0.001f);
        var delta = (B - A);
        return new Vector3(-delta.z, 0, delta.x).normalized;
    }


    public Vector3 GetUp(float t)
    {
        var A = GetPoint(t - 0.001f);
        var B = GetPoint(t + 0.001f);
        var delta = (B - A).normalized;
        return Vector3.Cross(delta, GetRight(t));
    }


    public void InsertControlPoint(int index, Vector3 position)
    {
        ResetIndex();
        if (index >= points.Count)
            points.Add(position);
        else
            points.Insert(index, position);
    }


    public void RemoveControlPoint(int index)
    {
		ResetIndex();
        points.RemoveAt(index);
    }

    public void SetControlPoint(int index, Vector3 position)
    {
		ResetIndex();
		points[index] = position;
    }

	Vector3 GetPointByIndex(int i)
	{
		if(i<0)
			i+=points.Count;
		return points[i % points.Count];
	}

	Vector3 Hermite(float t)
    {
        var count = points.Count - (closed ? 0 : 3);
        var i = Mathf.Min(Mathf.FloorToInt(t * (float)count), count - 1);
        var u = t * (float)count - (float)i;
        var a = GetPointByIndex(i);
        var b = GetPointByIndex(i + 1);
        var c = GetPointByIndex(i + 2);
        var d = GetPointByIndex(i + 3);
        return transform.TransformPoint(Interpolate(a, b, c, d, u));
    }

	/// <summary>
	/// This is a hermite spline interpolation function. 
	/// It takes 4 vectors (a and b are control points, b and c are the start and end points)
	///  and a u parameter which specifies the interpolation position.
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <param name="c"></param>
	/// <param name="d"></param>
	/// <param name="u"></param>
	/// <returns></returns>
	internal static Vector3 Interpolate(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u)
    {
        return (
            0.5f *
            (
                (-a + 3f * b - 3f * c + d) *
                (u * u * u) +
                (2f * a - 5f * b + 4f * c - d) *
                (u * u) +
                (-a + c) *
                u + 2f * b
            )
        );
    }
}

public class SplineIndex
{
    public Vector3[] linearPoints;
    SplineComponent spline;


    public int ControlPointCount => spline.ControlPointCount;


    public SplineIndex(SplineComponent spline)
    {
        this.spline = spline;
        ReIndex();
    }


    public void ReIndex()
    {
        var searchStepSize = 0.00001f;
        var length = spline.GetLength(searchStepSize);
        var indexSize = Mathf.FloorToInt(length * 2);
        var _linearPoints = new List<Vector3>(indexSize);
        var t = 0f;


        var linearDistanceStep = length / 1024;
        var linearDistanceStep2 = Mathf.Pow(linearDistanceStep, 2);


        var start = spline.GetNonUniformPoint(0);
        _linearPoints.Add(start);
        while (t <= 1f)
        {
            var current = spline.GetNonUniformPoint(t);
            while ((current - start).sqrMagnitude <= linearDistanceStep2)
            {
                t += searchStepSize;
                current = spline.GetNonUniformPoint(t);
            }
            start = current;
            _linearPoints.Add(current);
        }
        linearPoints = _linearPoints.ToArray();
    }

    public Vector3 GetNormal(float t, Vector3 up)
    {
        Vector3 tng = GetTangent(t);
        Vector3 binormal = Vector3.Cross(up, tng).normalized;
        return Vector3.Cross(tng, binormal);
    }

    public Vector3 GetTangent(float t)
    {
        var sections = linearPoints.Length - (spline.closed ? 0 : 3);
        var i = Mathf.Min(Mathf.FloorToInt(t * (float)sections), sections - 1);
        var count = linearPoints.Length;
        if(i<0)
            i+=count;
        var a = linearPoints[(i + 0) % count];
        var b = linearPoints[(i + 1) % count];
        var c = linearPoints[(i + 2) % count];
        var d = linearPoints[(i + 3) % count];
		Vector3 tangent = a * (-Mathf.Pow((1 - t), 2))
						+ b * (t * (3 * t - 4) + 1)
						+ c * (-3 * (t * t) + 2 * t)
						+ d * (t * t);
		return tangent.normalized;
    }

    public Vector3 GetPoint(float t)
    {
        var sections = linearPoints.Length - (spline.closed ? 0 : 3);
        var i = Mathf.Min(Mathf.FloorToInt(t * (float)sections), sections - 1);
        var count = linearPoints.Length;
        if (i < 0) 
            i += count;
        var u = t * (float)sections - (float)i;
        var a = linearPoints[(i + 0) % count];
        var b = linearPoints[(i + 1) % count];
        var c = linearPoints[(i + 2) % count];
        var d = linearPoints[(i + 3) % count];
        return SplineComponent.Interpolate(a, b, c, d, u);
    }
}

/// <summary>
/// A interface for general spline data.
/// NB: - All Vector3 arguments and Vector3 return values are in world space.
///     - All t arguments specify a uniform position along the spline, apart
///       from the GetNonUniformPoint method.
/// </summary>
public interface ISpline
{
    Vector3 GetNonUniformPoint(float t);
    Vector3 GetPoint(float t);

    Vector3 GetLeft(float t);
    Vector3 GetRight(float t);
    Vector3 GetUp(float t);
    Vector3 GetDown(float t);
    Vector3 GetForward(float t);
    Vector3 GetBackward(float t);

    float GetLength(float stepSize);

    Vector3 GetControlPoint(int index);
    void SetControlPoint(int index, Vector3 position);
    void InsertControlPoint(int index, Vector3 position);
    void RemoveControlPoint(int index);

    Vector3 GetDistance(float distance);
    Vector3 FindClosest(Vector3 worldPoint);

    int ControlPointCount { get; }
}
