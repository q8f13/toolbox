using UnityEngine;
using System.Collections;

/// <summary>
/// rotation in space with sensor in phone(compass, gyro)
/// Attach to First person camera for common use
/// </summary>

namespace qbfox
{
	public class qfRotationViaSensor : MonoBehaviour
	{
		private Vector3 _currEuler;

		// Use this for initialization
		void Start () {
			Input.gyro.enabled = true;
			Input.compass.enabled = true;
		}
		
		/// <summary>
		/// 用gravity的值计算当前设备的欧拉角
		/// 相机本身的旋转偏移量使用gyroscope的rotationRateUnbiased（已滤噪）
		/// 给这两个做插值，能得到更稳定和平滑的第一人称相机旋转
		/// TODO: 欧拉角pitch 90度限制
		/// </summary>
		void Update ()
		{
			Vector3 gravity = Input.gyro.gravity;
			Vector3 gyroOffset = Input.gyro.rotationRateUnbiased;

			_currEuler = transform.eulerAngles;

			_currEuler -= gyroOffset;

			Vector3 cameraEulerFromGravity = new Vector3(-gravity.z * 90f, _currEuler.y, -gravity.x * 90f);

			transform.rotation = Quaternion.Lerp
				(
					Quaternion.Euler(_currEuler)
					, Quaternion.Euler(cameraEulerFromGravity)
					, 0.5f
				);

		}
	}
}

