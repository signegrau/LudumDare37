using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeControl : MonoBehaviour {

    public KeyCode pauseKey;
    private bool paused;

    public AnimationCurve curve;

    public float maxTimeScale = 1, minTimeScale = 0.04f;
    public float timeScaleChangeRate = 5f;

    private float t = 0;
    private float a;
    private float b;

	// Use this for initialization
	void Start () {
		paused = false;
        a = maxTimeScale;
        b = minTimeScale;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(pauseKey)) {
            paused = !paused;
            a = paused ? maxTimeScale : minTimeScale;
            b = paused ? minTimeScale : maxTimeScale;
            t = 1 - t;
        }

        if (t < 1) {
            t += Time.unscaledDeltaTime * timeScaleChangeRate;
        }
        else {
            t = 1;
        }

        Time.timeScale = Mathf.LerpUnclamped(a, b, curve.Evaluate(t));
	}
}
