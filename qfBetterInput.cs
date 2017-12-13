using UnityEngine;
using System.Collections;

public class qfBetterInput : MonoBehaviour
{
	[Range(0.0f, 0.4f)]
	public float InnerDeadZoneThreshold = 0.22f;
	[Range(0.6f, 0.99f)]
	public float RadialDeadZoneThreshold = 0.85f;

	public float AngularDeadZoneInDeg = 5.0f;

	public float MagnitudeThreshold = 0.08f;

	private Vector3 _currentInput = Vector3.zero;
	private Vector3 _outputInput = Vector3.zero;

	private Vector3 _inputFiltered = Vector3.zero;

	private Color _rawColor = Color.red;
	private Color _darkRawColor = new Color(178f/255f, 0, 0);
	private Color _deadZone = new Color(0, 75f/255f, 197f/255f);

	private float _currTimeScale = 1.0f;
	private float _length = -1f;


	// Use this for initialization
	void Start ()
	{
		Camera.main.orthographic = true;
		Camera.main.clearFlags = CameraClearFlags.SolidColor;
		_length = Camera.main.orthographicSize;
	}
	
	// Update is called once per frame
	void Update ()
	{
		Time.timeScale = _currTimeScale;

		_currentInput.x = Input.GetAxis("Horizontal");
		_currentInput.y = Input.GetAxis("Vertical");

		_outputInput = TweakInput(_currentInput);
	}

	Vector3 TweakInput(Vector3 rawData)
	{
		_inputFiltered = rawData;

		float len = rawData.magnitude;
		bool underInnerThreshold = len < InnerDeadZoneThreshold;
		bool inMiddleArea = len >= InnerDeadZoneThreshold && len < RadialDeadZoneThreshold;

		// magnitude remapping
		if (Mathf.Abs(len - 0f) < MagnitudeThreshold)
		{
			_inputFiltered.x = 0.0f;
			_inputFiltered.y = 0.0f;
		}

		if (Mathf.Abs(len - 1f) < MagnitudeThreshold)
		{
			_inputFiltered = rawData.normalized * _length;
		}

		// output optimized using filtered input data
		Vector3 output = _inputFiltered;
		len = _inputFiltered.magnitude;

		if (underInnerThreshold)
		{
			output.x = 0.0f;
			output.y = 0.0f;
		}
		else if (inMiddleArea)
		{
//			int x_dir, y_dir = 0;
//			x_dir = _inputFiltered.x > 0 ? 1 : -1;
//			y_dir = _inputFiltered.y > 0 ? 1 : -1;
			output.x = _inputFiltered.x * (len - InnerDeadZoneThreshold)/(RadialDeadZoneThreshold - InnerDeadZoneThreshold) * RadialDeadZoneThreshold;
			output.y = _inputFiltered.y * (len - InnerDeadZoneThreshold)/(RadialDeadZoneThreshold - InnerDeadZoneThreshold) * RadialDeadZoneThreshold;
//			output.x = rawData.x * (1.0f - (RadialDeadZoneThreshold - len)/(RadialDeadZoneThreshold - InnerDeadZoneThreshold)) * RadialDeadZoneThreshold;
//			output.y = rawData.y * (1.0f - (RadialDeadZoneThreshold - len)/(RadialDeadZoneThreshold - InnerDeadZoneThreshold))* RadialDeadZoneThreshold;
		}
		// out of radialDeadzone
		else
		{
			output = rawData.normalized;
		}

		// angularDeadZone
		if (output.x != 0f && output.y != 0)
		{
			float angleDegFromAxisY = Mathf.Atan(Mathf.Abs(output.x) / (Mathf.Abs(output.y))) * Mathf.Rad2Deg;
			float angleDegFromAxisX = Mathf.Atan(Mathf.Abs(output.y)/(Mathf.Abs(output.x)))*Mathf.Rad2Deg;
			if (angleDegFromAxisY < AngularDeadZoneInDeg)
				output.x = 0f;

			if (angleDegFromAxisX < AngularDeadZoneInDeg)
				output.y = 0f;
		}

		return output;
	}

	void OnGUI()
	{
		_currTimeScale = GUI.HorizontalSlider(new Rect(0, 20, 200, 30), _currTimeScale, 0.0f, 1.0f);
		GUI.Label(new Rect(0, 60, 200, 30), string.Format("TimeScale : {0}", _currTimeScale.ToString("f1")));
	}

	void OnDrawGizmos()
	{
//		float scaleFromCamSize = 0.8f;
//		GUI.Box(new Rect(), )
//		GUI.Box(new Rect(-Screen.width/2, -Screen.height/2, ScalarValue, ScalarValue), "");
		Vector3 scrCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
		scrCenter.z = 50f;
		float length = _length;
		// outframe
		Gizmos.color = _rawColor;
		Gizmos.DrawWireSphere(scrCenter, length );
		Gizmos.color = _darkRawColor;
		Gizmos.DrawWireCube(scrCenter, Vector3.one * length * 2 );
		// deadzone
		scrCenter.z = 80f;
		Gizmos.color = _deadZone;
		Gizmos.DrawWireSphere(scrCenter, length * RadialDeadZoneThreshold);
		Gizmos.DrawWireSphere(scrCenter, length * InnerDeadZoneThreshold);

//		scrCenter.z = 30f;
//		Gizmos.DrawRay(scrCenter, Vector3.up * length);
//		Gizmos.DrawRay(scrCenter, Vector3.down * length);
//		Gizmos.DrawRay(scrCenter, Vector3.left * length);
//		Gizmos.DrawRay(scrCenter, Vector3.right * length);

		// angular deadzone
		float offsetFromAxis = Mathf.Tan(AngularDeadZoneInDeg*Mathf.Deg2Rad)*length;
		Gizmos.DrawRay(scrCenter, (Vector3.up * length + new Vector3(-offsetFromAxis, 0, 0)));
		Gizmos.DrawRay(scrCenter, (Vector3.up * length + new Vector3(offsetFromAxis, 0, 0)));
		Gizmos.DrawRay(scrCenter, (Vector3.down * length + new Vector3(-offsetFromAxis, 0, 0)));
		Gizmos.DrawRay(scrCenter, (Vector3.down * length + new Vector3(offsetFromAxis, 0, 0)));
		Gizmos.DrawRay(scrCenter, (Vector3.right * length + new Vector3(0, offsetFromAxis, 0)));
		Gizmos.DrawRay(scrCenter, (Vector3.right * length + new Vector3(0, -offsetFromAxis, 0)));
		Gizmos.DrawRay(scrCenter, (Vector3.left * length + new Vector3(0, offsetFromAxis, 0)));
		Gizmos.DrawRay(scrCenter, (Vector3.left * length + new Vector3(0, -offsetFromAxis, 0)));

		// inputs
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(scrCenter + _inputFiltered * length, 0.02f * length);
//		Gizmos.DrawWireSphere(scrCenter + _currentInput * length, 0.02f * length);
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(scrCenter + _outputInput * length, 0.02f * length);

	}
}
