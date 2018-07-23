using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 屏幕外指示器
/// 需要注意指示箭头的父级Transform的Anchor需要是BottomLeft，以符合屏幕空间坐标系
/// </summary>
public class ScreenEdgeIndicator : MonoBehaviour
{
	[Header("要追踪的目标")]
	public Transform Target;
	[Header("指示箭头")]
	public Image ArrowToTarget;
	[Header("屏幕内沿距离")]
	public float Padding = 15.0f;
//	public float Padding = 15.0f;

	[Header("玩家角色所用Tag")] public string PlayerTargetTag = "Player";

	private CanvasGroup _arrowRenderer;

	private Transform _playerT;

	private Camera _cam;
	private Vector3 _posInScreen;
	public Vector3 ArrowPosInScreen { get { return _arrow_rt.anchoredPosition; } }

	private RectTransform _arrow_rt;

	private int _deviceWidth = 1080;
	private int _deviceHeight = 1920;


	private bool _enabled = false;
	private bool _out_of_screen = false;

	private Color _arrowColor;

	private float _deg;
	private float _degThreshold;

	// Use this for initialization
	void Start ()
	{

		if (Application.isMobilePlatform)
		{
			_deviceHeight = Screen.currentResolution.height;
			_deviceWidth = Screen.currentResolution.width;
		}

		_cam = Camera.main;
		_playerT = GameObject.FindGameObjectWithTag(PlayerTargetTag).transform;
		_arrow_rt = ArrowToTarget.transform.parent.GetComponent<RectTransform>();

//		_arrowRenderer = ArrowToTarget.GetComponent<CanvasGroup>();
		_arrowColor = ArrowToTarget.color;

		_degThreshold = Mathf.Atan((_deviceHeight * 0.5f)/(_deviceWidth * 0.5f)) * Mathf.Rad2Deg;

		Toggle(true);
	}

	public void Toggle(bool on)
	{
		_enabled = on;
	}

	public bool IsTargetOutOfScreen(Vector3 worldPosition, Camera cam)
	{
		Vector3 p = cam.WorldToScreenPoint(worldPosition);
		bool result = p.x < 0
		              || p.x > Screen.width
		              || p.y < 0
		              || p.y > Screen.height;
		return result;
	}

	// Update is called once per frame
	void Update ()
	{
		// 还是有反转的问题
		// TODO: 准备换一种方式，直接将世界空间的vector offset转换到屏幕空间看看行不行
		_posInScreen = _cam.WorldToScreenPoint(calculateWorldPosition(Target.position, _cam));

		_out_of_screen = IsTargetOutOfScreen(Target.position, _cam);

//		ArrowToTarget.enabled = _out_of_screen && _enabled;
		_arrowColor = _out_of_screen && _enabled ? Color.white : Color.clear;
		ArrowToTarget.color = _arrowColor;

//		if (_out_of_screen)
//		{
			Vector3 screenCenter = new Vector3(Screen.width*0.5f, Screen.height*0.5f, 0);

			Vector3 dir = (_posInScreen - screenCenter).normalized;
			Debug.DrawLine(screenCenter, _posInScreen, Color.red);

//			float d = GetClampedDistance(_posInScreen, screenCenter);

			_posInScreen.x = Mathf.Clamp(_posInScreen.x, Padding, Screen.width - Padding);
			_posInScreen.y = Mathf.Clamp(_posInScreen.y, Padding, Screen.height - Padding);

			_posInScreen.x = (_posInScreen.x/Screen.width)*_deviceWidth;
			_posInScreen.y = (_posInScreen.y/Screen.height)*_deviceHeight;

//			Debug.DrawRay(screenCenter, dir * d, Color.magenta);
			Vector3 dirOnPlane = Vector3.ProjectOnPlane(dir, Vector3.back);

			_arrow_rt.anchoredPosition = _posInScreen;
//			_arrow_rt.anchoredPosition = SCREEN_CENTER + dir*d;

			_arrow_rt.up = dirOnPlane;
//		}
	}

/*
	float GetClampedDistance(Vector3 screenPos, Vector3 center)
	{
		Vector3 offset = screenPos - center;
		float rad = Mathf.Atan(offset.y / offset.x);
		_deg = rad*Mathf.Rad2Deg;
		screenPos.x = Mathf.Clamp(screenPos.x, Padding, Screen.width - Padding);
		screenPos.y = Mathf.Clamp(screenPos.y, Padding, Screen.height - Padding);

		float proj = 0.0f;
		float d = 0.0f;
		if (Mathf.Abs(_deg) <= _degThreshold)
		{
			proj = Vector3.Dot((screenPos - center), Vector3.right);
			d = (proj / Screen.width)* _deviceWidth/Mathf.Cos(rad);
		}
		else
		{
			proj = Vector3.Dot((screenPos - center), Vector3.up);
			d = (proj/Screen.height)*_deviceHeight/Mathf.Sin(rad);
		}

		return Mathf.Abs(d);
	}
*/

	// position = the world position of the entity to be tested
	private Vector3 calculateWorldPosition(Vector3 position, Camera camera)
	{
		//if the point is behind the camera then project it onto the camera plane
		Vector3 camNormal = camera.transform.forward;
		Vector3 vectorFromCam = position - camera.transform.position;
		float camNormDot = Vector3.Dot(camNormal, vectorFromCam.normalized);
		if (camNormDot <= 0f)
		{
			//we are beind the camera, project the position on the camera plane
			float camDot = Vector3.Dot(camNormal, vectorFromCam);
			Vector3 proj = (camNormal * camDot * 1.01f);   //small epsilon to keep the position infront of the camera
			position = camera.transform.position + (vectorFromCam - proj);
		}

		return position;
	}
}
