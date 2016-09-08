using System;
using UnityEngine;

/// <summary>
/// 2d parallax scrolling
/// </summary>

namespace qbfox
{
	[ExecuteInEditMode]
	public class qfParallexScrolling : MonoBehaviour
	{
		public ScrollingBackground[] Backgrounds;		// 所需滚动的背景
		public Camera TargetCamera;						// 基于这个相机的移动来做卷屏

		public bool HorizontalOnly = false;				// 是否限于水平方向

		public bool IsExecuting = false;				// 是否生效（编辑器下有效）

		private Vector3 _camLastPos;					// 上一帧的相机位置，用来计算位移

		void Update()
		{
			if (Backgrounds.Length == 0 || TargetCamera == null)
			{
				Debug.LogError("configuration not ready yet");
				return;
			}

			if (!IsExecuting || _camLastPos == Vector3.zero)
				return;

			Vector3 camPos = TargetCamera.transform.position;
			Vector2 cam_delta_offset = new Vector2(_camLastPos.x - camPos.x, _camLastPos.y - camPos.y);

			if (cam_delta_offset == Vector2.zero)
				return;

			// 如果只水平卷动，则不计算y轴的变化
			if (HorizontalOnly)
				cam_delta_offset.y = 0f;

			for (int i = 0; i < Backgrounds.Length; i++)
			{
				Backgrounds[i].Container.Translate(cam_delta_offset*Backgrounds[i].SpeedRate);
			}

			// cache camera position this frame
			_camLastPos = TargetCamera.transform.position;
		}
	}

	/// <summary>
	/// 卷动背景的配置
	/// </summary>
	[Serializable]
	public class ScrollingBackground
	{
		public Transform Container;		// 背景所在的transform
		[Range(0,1)]
		public float SpeedRate;			// 相对相机的移动速度的比率
	}
}

