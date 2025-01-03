using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FrameRateCappedCamera : MonoBehaviour {
    [Header("FPS Limit"), Range(1, 60)]
    public int fps = 20;
    float elapsed;
    Camera cam;

	void Start () {
        cam = GetComponent<Camera>();
        cam.enabled = false;
	}
	
	void Update () {
        elapsed += Time.deltaTime;
        if (elapsed > 1.0f / fps) {
            elapsed = 0.0f;
            cam.Render();
        }
	}
}