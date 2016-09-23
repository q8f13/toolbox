using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class qfDetectionMessenger : MonoBehaviour
{
	public System.Action<Collider> OnTriggerEnterAction;
	public System.Action<Collider> OnTriggerExitAction;
	public System.Action<Collider> OnTriggerStayAction;

	public System.Action<Collision> OnCollisionEnterAction;
	public System.Action<Collision> OnCollisionExitAction;
	public System.Action<Collision> OnCollisionStayAction;

	private Collider _collider;
	public Collider Collider { get { return _collider; } }

	void Start()
	{
		_collider = GetComponent<Collider>();
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

