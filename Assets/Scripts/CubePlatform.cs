using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CubePlatform : MonoBehaviour
{
    private enum CubeState
    {
        Background,
        Forground,
        Moving
    }

    private CubeState state = CubeState.Background;
    private bool movingToForeground;

    private float lerpTime;
    private float lerpValue;
    public float timeToMove = 1f;

    public AnimationCurve curve;

    private Vector3 foregroundPosition = new Vector3(0, 0, -1);
    private Vector3 backgroundPosition = new Vector3(0, 0, 0);

    private Color foregroundColor = new Color(1f, 1f, 1f);
    private readonly Color backgroundColor = new Color(0.7f, 0.7f, 0.7f);

    private Renderer renderer;

    // Use this for initialization
	public void Start ()
	{
	    renderer = GetComponent<Renderer>();
	    renderer.material.SetColor("_Color", backgroundColor);

	    foregroundPosition += transform.position;
	    backgroundPosition += transform.position;
	}

	public void Update ()
	{
	    if (state != CubeState.Moving) return;

	    if (movingToForeground)
	    {
	        lerpTime += 1f / timeToMove * Time.deltaTime;
	    }
	    else
	    {
	        lerpTime -= 1f / timeToMove * Time.deltaTime;
	    }

	    if (lerpTime < 0)
	    {
	        Debug.Log("Hit lower bound");
	        lerpTime = 0;
	        state = CubeState.Background;
	    }
	    else if (lerpTime > 1)
	    {
	        Debug.Log("Hit upper bound");
	        lerpTime = 1;
	        state = CubeState.Forground;
	    }

	    lerpValue = curve.Evaluate(lerpTime);
	    Debug.Log(lerpValue);

	    transform.position = Vector3.LerpUnclamped(backgroundPosition, foregroundPosition, lerpValue);
	    renderer.material.SetColor("_Color", Color.LerpUnclamped(backgroundColor, foregroundColor, lerpValue));

	}

    public void MoveToForeground()
    {
        state = CubeState.Moving;
        movingToForeground = true;
    }

    public void MoveToBackground()
    {
        state = CubeState.Moving;
        movingToForeground = false;
    }

    private void OnMouseDown()
    {
        Debug.Log("Mouse down");

        switch (state)
        {
            case CubeState.Background:
                MoveToForeground();
                break;
            case CubeState.Forground:
                MoveToBackground();
                break;
            case CubeState.Moving:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
