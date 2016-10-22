using System;
using UnityEngine;

/// <summary>
/// 2d scrolling camera
/// </summary>

namespace qbfox
{
	[RequireComponent(typeof(Camera))]
	public class qf2DScrollingCamera : MonoBehaviour
	{
		public Transform Target;

		// boundary
		public float Top = 0.3f;
		public float Bottom = 0.3f;
		public float Left = 0.3f;
		public float Right = 0.3f;

		public bool HorizontalOnly = false;

		// scrolling style
		public ScrollingType Policy = ScrollingType.Starring;

		// boundarys
		public BoxCollider2D LeftBorder;
		public BoxCollider2D RightBorder;

		// target bound in screen
		private Vector3 _targetSize;

		// target world position in 
		private Vector3 _targetInViewportPoint;

		// ref to cam
		private Camera _cam;

		// screen offset to target while scrolling
		private float _offsetX = float.NaN;
		private float _offsetY = float.NaN;
		private float _offsetZ = float.NaN;

		// debug toggle (draw gizmos)
		private bool _debugOn = false;

		// points for drawing boundary
		private Vector3 _bleedout_tl;
		private Vector3 _bleedout_tr;
		private Vector3 _bleedout_bl;
		private Vector3 _bleedout_br;

		private Vector3 _targetLastPosition;

		private float _offsetXInLerping = -1f;

		private Vector2 _camHalfSizeInWorld;

		public void SetTarget(Transform target)
		{
			// lock z offset from cam to target
			Target = target;
			_offsetZ = target.position.z - transform.position.z;

			_targetInViewportPoint = _cam.WorldToViewportPoint(Target.position);
		}

		void Start()
		{
			_cam = GetComponent<Camera>();

			_debugOn = true;

			_camHalfSizeInWorld.x = _cam.ViewportToWorldPoint(new Vector3(0.5f, 0, 0)).x - _cam.ViewportToWorldPoint(Vector3.zero).x;
			_camHalfSizeInWorld.y = _cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)).y - _cam.ViewportToWorldPoint(Vector3.zero).y;

			if (Target == null)
				return;

			// lock z offset from cam to target
			_offsetZ = Target.position.z - transform.position.z;

		}

		void Update()
		{
			if (Target == null)
				return;


			// 根据目标在世界空间投射到屏幕后的位置来决定是否需要卷屏
			_targetInViewportPoint = _cam.WorldToViewportPoint(Target.position);
			Vector3 pos = transform.position;

			//UNDONE: various scrolling style
			switch (Policy)
			{
				case ScrollingType.Starring:
					break;
				case ScrollingType.BoxBinded:
					// 水平方向
					BoxBindedScrolling(ref pos);
					break;
				case ScrollingType.Smart:
					if (_offsetXInLerping < 0f)
					{
						_offsetXInLerping = (_cam.ViewportToWorldPoint(new Vector3(0.5f + Left, 0, 0)) -
						                     _cam.ViewportToWorldPoint(new Vector3(0.5f, 0, 0)))
							.x;
					}

					if (_targetLastPosition != Target.position
						&& IsOutOfBoundHorizon())
					{
						BoxBindedScrolling(ref pos);
					}
					else
					{
						bool targetIsFacingForward = Target.localScale.x >= 0;

						Vector3 targetPos = Target.position;
						targetPos.x += _offsetXInLerping * (targetIsFacingForward ? 1f : -1f);
						targetPos.z = transform.position.z;
						targetPos.y = transform.position.y;
						transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime);
					}

					break;
			}

			// boundary limit
			if (LeftBorder && RightBorder)
			{
				Vector3 post_pos = transform.position;
				post_pos.x = Mathf.Clamp(post_pos.x
					, LeftBorder.transform.position.x + _camHalfSizeInWorld.x
					, RightBorder.transform.position.x - _camHalfSizeInWorld.x);

				transform.position = post_pos;
			}

			_targetLastPosition = Target.position;

		}

		/// <summary>
		/// 几种不同的卷屏策略
		/// 1. starring	位置锁定
		/// 2. boxBinded	设定屏幕空间的一个rect，如果角色位置要移出则跟随位置，并保持偏移量
		/// 3. boxBindedMirror	同2， 但目标换向的时候相机tween到与角色朝向(x axis)相反的方向
		/// 4. ...
		/// </summary>
		public enum ScrollingType
		{
			Starring = 0,
			BoxBinded = 1,
			BoxBindedMirrow = 2,		// TBD
			Smart = 3,
		}

		void BoxBindedScrolling(ref Vector3 pos)
		{
			if (IsOutOfBoundHorizon())
			{
				// 给水平offset赋初始值
				if (float.IsNaN(_offsetX))
					_offsetX = Mathf.Abs(Target.position.x - transform.position.x);

				// 朝向修正
				int prefix = Target.position.x > transform.position.x
					? -1
					: 1;

				// 相机跟随
				pos.x = Target.position.x + prefix*_offsetX;
			}

			// 垂直方向
			if (IsOutOfBoundVertical() && HorizontalOnly)
			{
				// 垂直offset初始赋值
				if (float.IsNaN(_offsetY))
					_offsetY = Mathf.Abs(Target.position.y - transform.position.y);

				// 朝向修正
				int prefix = Target.position.y > transform.position.y
					? -1
					: 1;

				// 相机跟随
				pos.y = Target.position.y + prefix*_offsetY;
			}

			// lock住深度偏移量
			pos.z = Target.position.z - _offsetZ;
			transform.position = pos;

		}

		void OnGUI()
		{
			if (!_debugOn || Target == null)
				return;

			GUI.Label(new Rect(0, 100, 300,40),string.Format("viewport pt: {0}", _targetInViewportPoint) );
		}

		void OnDrawGizmos()
		{
			if (!_debugOn || Target == null)
				return;

			Renderer r = Target.GetComponentInChildren<Renderer>();
			_targetSize = _cam.WorldToScreenPoint(r.bounds.size);
	//		r.bounds
			Vector3 pos = transform.position;
			pos.z += _cam.farClipPlane;

			Gizmos.color = Color.green;
			Vector3 tarPos = Target.position;
			Vector3 screenBasedSize = _cam.ScreenToWorldPoint(_targetSize);
			tarPos.y += screenBasedSize.y/2;
			screenBasedSize.z = 0f;
			Gizmos.DrawWireCube(tarPos, screenBasedSize);


			// draw bleedout

			_bleedout_tl = _cam.ViewportToWorldPoint(new Vector3(0.5f - Left, 0.5f + Top, _cam.nearClipPlane));
			_bleedout_tr = _cam.ViewportToWorldPoint(new Vector3(0.5f + Right, 0.5f + Top, _cam.nearClipPlane));
			_bleedout_bl = _cam.ViewportToWorldPoint(new Vector3(0.5f - Left, 0.5f - Bottom, _cam.nearClipPlane));
			_bleedout_br = _cam.ViewportToWorldPoint(new Vector3(0.5f + Right, 0.5f - Bottom, _cam.nearClipPlane));

			Gizmos.color = Color.red;
			Gizmos.DrawLine(_bleedout_tl, _bleedout_tr);
			Gizmos.DrawLine(_bleedout_tl, _bleedout_bl);
			Gizmos.DrawLine(_bleedout_bl, _bleedout_br);
			Gizmos.DrawLine(_bleedout_tr, _bleedout_br);

		}

		// 判断水平方向出界
		bool IsOutOfBoundHorizon()
		{
			if (_targetInViewportPoint.x < 0.5f - Left
			    && Target.position.x < _targetLastPosition.x)
				return true;

			if ( _targetInViewportPoint.x > 0.5f + Right
			    && Target.position.x > _targetLastPosition.x)
				return true;

			return false;
		}

		// 判断垂直方向出界
		bool IsOutOfBoundVertical()
		{
			if (_targetInViewportPoint.y > 0.5f + Top || _targetInViewportPoint.y < 0.5f - Bottom)
				return true;

			return false;
		}
	}
}


