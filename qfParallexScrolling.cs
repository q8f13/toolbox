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
		public float Multiply = 1.0f;
		public ScrollingBackground[] Backgrounds;		// 所需滚动的背景
		public Camera TargetCamera;						// 基于这个相机的移动来做卷屏

		public bool HorizontalOnly = false;				// 是否限于水平方向

		public bool IsExecuting = false;				// 是否生效（编辑器下有效）

		public float Z_Offset = 1.0f;

		private Vector3 _camLastPos;					// 上一帧的相机位置，用来计算位移

		private float _horizionOffsetToScreen = 0.0f;

		private Vector3 _viewportToCameraOffset;

		void Start()
		{
			IsExecuting = true;
			_viewportToCameraOffset = transform.position - TargetCamera.transform.position;
			_viewportToCameraOffset.z = 0;

		}

		void Update()
		{
			if (Backgrounds.Length == 0 || TargetCamera == null)
			{
				Debug.LogError("configuration not ready yet");
				return;
			}

			if (_camLastPos == Vector3.zero)
			{
				_camLastPos = TargetCamera.transform.position;
				return;
			}

			if (!IsExecuting || _camLastPos == Vector3.zero)
				return;

			Vector3 camPos_xy = TargetCamera.transform.position;
			camPos_xy.z = 0;

			Vector2 cam_delta_offset = new Vector2(_camLastPos.x - camPos_xy.x, _camLastPos.y - camPos_xy.y);

			if (cam_delta_offset == Vector2.zero)
				return;

			// 如果只水平卷动，则不计算y轴的变化
			if (HorizontalOnly)
				cam_delta_offset.y = 0f;

			_horizionOffsetToScreen = cam_delta_offset.x/Screen.width;

			// 相机保持相对位置
			Vector3 nextPos = camPos_xy + _viewportToCameraOffset;
			transform.position = new Vector3(nextPos.x, nextPos.y, transform.position.z); 

			ScrollingBackground currBg = null;
			for (int i = 0; i < Backgrounds.Length; i++)
			{
				currBg = Backgrounds[i];
				float uvSpeed = _horizionOffsetToScreen * currBg.SpeedRate * Multiply;
				Material mat = currBg.Container.GetComponent<MeshRenderer>().sharedMaterial;
				if (currBg.IsUVLooping
				 && BondAssetOutOfViewport(currBg.Container, currBg.AssetBond))
				{
					if (mat)
					{
						Vector2 offset = mat.GetTextureOffset("_MainTex");
						offset.x -= uvSpeed;
						mat.SetTextureOffset("_MainTex", offset);
					}
				}
				else
				{
					float width = currBg.Container.localScale.x;
					if (currBg.IsUVLooping)
					{
						Vector3 pos = currBg.Container.localPosition;
						pos.x = currBg.AssetBond.localPosition.x - currBg.AssetBond.localScale.x/2 - currBg.Container.localScale.x/2;
						currBg.Container.localPosition = pos;
					}
					else
					{
						if(currBg.AssetBond != null && !currBg.IsUVLooping)
							width = currBg.AssetBond.localScale.x;
						Debug.Assert(width > 0f, "width should above 0");
	//					float width = Screen.width;
	//					Backgrounds[i].Container.Translate(cam_delta_offset*Backgrounds[i].SpeedRate, Space.Self);
	//					Backgrounds[i].Container.Translate(new Vector3(uvSpeed * width * 202.7f / Backgrounds[i].Container.localScale.x, 0, 0), Space.Self);
						currBg.Container.Translate(new Vector3(uvSpeed * width, 0, 0), Space.Self);
					}
				}
			}

			// cache camera position this frame
			_camLastPos = TargetCamera.transform.position;
		}

		/// <summary>
		/// 判断uv滚动素材和移动素材的衔接
		/// </summary>
		/// <param name="baseAsset"></param>
		/// <param name="bond"></param>
		/// <returns></returns>
		bool BondAssetOutOfViewport(Transform baseAsset, Transform bond)
		{
			if (bond == null)
				return true;

//			// 移动素材在uv滚动素材右侧
//			if (bond.localPosition.x > baseAsset.localPosition.x)
//			{
				float bondAssetLeftBoundPosition = bond.localPosition.x - bond.localScale.x/2;
				float baseAssetRightBoundOnScreenPosition = baseAsset.localScale.x/2;

				return bondAssetLeftBoundPosition > baseAssetRightBoundOnScreenPosition;
//			}
//			// 反过来，移动素材在uv素材左侧
//			else
//			{
//				float bondAssetRightBoundPosition = bond.localPosition.x + bond.localScale.x/2;
//				float baseAssetLeftBoundOnScreenPosition = -baseAsset.localScale.x/2;
//
//				return bondAssetRightBoundPosition < baseAssetLeftBoundOnScreenPosition;
//			}
		}

		#region contextMenu
		[ContextMenu("Sort Elements by Z Offset")]
		void SortElementsByZOffset()
		{
			for (int i = 0; i < Backgrounds.Length; i++)
			{
				Vector3 pos = Backgrounds[i].Container.localPosition;
				pos.z = (i + 1)*Z_Offset*(Backgrounds[i].IsCover ? -1 : 1);
				Backgrounds[i].Container.localPosition = pos;
			}
		}

		[ContextMenu("mark all elements UV Loop")]
		void MarkAllElementsUVLooping()
		{
			for (int i = 0; i < Backgrounds.Length; i++)
			{
				Backgrounds[i].IsUVLooping = true;
			}
		}

		[ContextMenu("mark all elements NOT UV Loop")]
		void MarkAllElementsNOTLooping()
		{
			for (int i = 0; i < Backgrounds.Length; i++)
			{
				Backgrounds[i].IsUVLooping = false;
			}
		}
		#endregion
	}

	/// <summary>
	/// 卷动背景的配置
	/// </summary>
	[Serializable]
	public class ScrollingBackground
	{
		public Transform Container;		// 背景所在的transform
		[Range(0,2)]
		public float SpeedRate = 1.0f;			// 相对相机的移动速度的比率

		public Transform AssetBond;

		public bool IsUVLooping = false;
		public bool IsCover = false;		// 能覆盖场景大多数内容的前景素材

	}
}

