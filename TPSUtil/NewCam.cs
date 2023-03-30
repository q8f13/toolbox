using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TPS Camera Controller
/// New implemented, more steady and robust version
/// Need VirtualCam component for future kill cam and other cam blending, like execution clip
/// </summary>
[RequireComponent(typeof(Cinemachine.CinemachineVirtualCamera))]
public class NewCam : MonoBehaviour
{
    [SerializeField, Header("Player Body")]
    Transform body;

	[SerializeField, Header("Cam looking offset")]
    Vector3 lookOffset;

	[SerializeField, Header("Smooth time for mouse movement")]
    float smoothTime = 0.2f;
    Vector3 dampingVelocity;

    Vector3 offset;

    [SerializeField, Header("Control Toggler")]
	private bool controlOn;
    Vector3 lastMousePos;

    [Header("RotateSpeed")]
    [SerializeField]
	private float rotSpeedX = 1f;
    [SerializeField]
	private float rotSpeedY = 1f;

    Vector3 lockPoint;
    Vector3 mp = Vector3.zero;

	private void Start() {
        offset = transform.position - body.position;
    }

    private void Update() {

        // damp mouse position not wpos or rotation
        // may cause offset shrinking when smoothing
        if(controlOn)
        {
            if(mp == default)
                mp = Input.mousePosition;
            else
                mp = Vector3.SmoothDamp(mp, Input.mousePosition, ref dampingVelocity, smoothTime);
            Vector3 dt = Vector3.zero;
            if(lastMousePos != default)
            {
                dt = mp - lastMousePos;
                Quaternion q = Quaternion.AngleAxis(-dt.x * rotSpeedX, Vector3.up);
                q = Quaternion.AngleAxis(-dt.y * rotSpeedY, transform.right) * q;
                offset = q * offset;
            }

            lastMousePos = mp;
        }

        DoUpdate();
    }

    // manually update cam position when not running
	[ContextMenu("DoUpdateDebug")]
    void DoUpdateDebug()
    {
        offset = transform.position - body.position;
        DoUpdate();
    }

    void DoUpdate()
    {
        transform.position = body.position + body.TransformVector(offset);
        lockPoint = body.position + transform.TransformVector(lookOffset);
        transform.LookAt(lockPoint);

        Debug.DrawLine(transform.position, lockPoint, Color.green);
        Debug.DrawLine(transform.position, body.position, Color.blue);
    }
}
