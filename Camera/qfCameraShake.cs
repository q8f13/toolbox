using UnityEngine;
using System.Collections;

public class qfCameraShake : MonoBehaviour
{
	public float MagnitudeX = 1.0f;
	public float MagnitudeY = 1.0f;

	private Vector3 _originCamPos;

	IEnumerator Shake_Co(float duration)
	{
		float elapsed = 0.0f;

		if(_originCamPos == default(Vector3))
			_originCamPos = transform.position;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;

			float percentComplete = elapsed / duration;
			float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

			// map value to [-1, 1]
			float x = Random.value*2.0f - 1.0f;
			float y = Random.value * 2.0f - 1.0f;
//			float z = Random.value * 2.0f - 1.0f;
			x *= MagnitudeX * damper;
			y *= MagnitudeY * damper;
//			z *= Magnitude * damper;

//			transform.position = new Vector3(x, y, originalCamPos.z);
			transform.position = _originCamPos + new Vector3(x, y,0);
//			transform.position = new Vector3(x, y, z);

			yield return null;
		}

		transform.position = _originCamPos;
	}

	public void Shake(float duration)
	{
		StartCoroutine(Shake_Co(duration));
	}
}
