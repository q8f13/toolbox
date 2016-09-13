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

		// scrolling style
		public ScrollingType Policy = ScrollingType.Starring;

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

		void Start()
		{
			_cam = GetComponent<Camera>();

			_debugOn = true;

			if (Target == null)
				return;

			// lock z offset from cam to target
			_offsetZ = Target.position.z - transform.position.z;
		}

		void Update()
		{
			if (Target == null)
				return;

			//UNDONE: various scrolling style
			switch (Policy)
			{
				case ScrollingType.Starring:
					break;
				case ScrollingType.BoxBinded:
					break;
			}

			// 根据目标在世界空间投射到屏幕后的位置来决定是否需要卷屏
			_targetInViewportPoint = _cam.WorldToViewportPoint(Target.position);
			Vector3 pos = transform.position;

			// 水平方向
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
			if (IsOutOfBoundVertical())
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

			_targetLastPosition = Target.position;

			transform.position = pos;
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
			BoxBindedMirrow = 2,
			TBD = 3,
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


