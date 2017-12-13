using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardQuad : MonoBehaviour
{
	[Tooltip("要朝向的相机，不设置则自动寻找主相机来使用")]
	public Camera FacingCam;

	// Update is called once per frame
	void Update ()
	{
		if (FacingCam == null)
		{
			FacingCam = Camera.main;
			Debug.LogWarning("没有找到手动设置的billboard相机，使用Camera.main");
		}

		transform.LookAt(transform.position + FacingCam.transform.rotation * Vector3.forward, FacingCam.transform.rotation * Vector3.up);
	}
}
