using UnityEngine;
using System.Collections;

/// <summary>
/// 将collider检测的结果通过delegate传出
/// </summary>
public class qfDetectionMessenger : MonoBehaviour
{
	public System.Action<Collider> OnTriggerEnterAction;
	public System.Action<Collider> OnTriggerExitAction;
	public System.Action<Collider> OnTriggerStayAction;

	public System.Action<Collider2D> OnTriggerEnter2DAction;
	public System.Action<Collider2D> OnTriggerExit2DAction;
	public System.Action<Collider2D> OnTriggerStay2DAction;

	public System.Action<Collision> OnCollisionEnterAction;
	public System.Action<Collision> OnCollisionExitAction;
	public System.Action<Collision> OnCollisionStayAction;

	void Start()
	{
//		_collider2D = GetComponent<Collider>();
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (OnTriggerEnter2DAction != null)
			OnTriggerEnter2DAction(collision);
	}

	void OnTriggerExit2D(Collider2D collision)
	{
		if (OnTriggerExit2DAction != null)
			OnTriggerExit2DAction(collision);
	}

	void OnTriggerStay2D(Collider2D collision)
	{
		if (OnTriggerStay2DAction != null)
			OnTriggerStay2DAction(collision);
	}

	void OnTriggerEnter(Collider col)
	{
		if (OnTriggerEnterAction != null)
			OnTriggerEnterAction(col);
	}

	void OnTriggerExit(Collider col)
	{
		if (OnTriggerExitAction != null)
			OnTriggerExitAction(col);
	}

	void OnTriggerStay(Collider col)
	{
		if (OnTriggerStayAction != null)
			OnTriggerStayAction(col);
	}

	void OnCollisionEnter(Collision col)
	{
		if (OnCollisionEnterAction != null)
			OnCollisionEnterAction(col);
	}

	void OnCollisionExit(Collision col)
	{
		if (OnCollisionExitAction != null)
			OnCollisionExitAction(col);
	}
	void OnCollisionStay(Collision col)
	{
		if (OnCollisionStayAction != null)
			OnCollisionStayAction(col);
	}
}

