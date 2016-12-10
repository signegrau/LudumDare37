using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum State
    {
        Wall,
        Platform,
        Moving
    }

    private State state = State.Wall;
    private bool movingToForeground;

    private float lerpTime;
    private float lerpValue;
    public float timeToMove = 1f;

    public AnimationCurve curve;

    private Vector3 foregroundPosition = new Vector3(0, 0, -1);
    private Vector3 backgroundPosition = new Vector3(0, 0, 0);

    private readonly Color foregroundColor = new Color(1f, 1f, 1f);
    private readonly Color backgroundColor = new Color(0.7f, 0.7f, 0.7f);

    private Renderer renderer;
    private Rigidbody _rigidbody;

    public BoxCollider2D collider;

    // Use this for initialization
	public void Start ()
	{
	    renderer = GetComponent<Renderer>();
	    renderer.material.SetColor("_Color", backgroundColor);

	    _rigidbody = GetComponent<Rigidbody>();
	    _rigidbody.isKinematic = true;

	    foregroundPosition += transform.position;
	    backgroundPosition += transform.position;
	}

	public void Update ()
	{
	    if (state != State.Moving) return;

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
	        state = State.Wall;
	        collider.enabled = false;
	    }
	    else if (lerpTime > 1)
	    {
	        Debug.Log("Hit upper bound");
	        lerpTime = 1;
	        state = State.Platform;
	        collider.enabled = true;
	        //_rigidbody.isKinematic = false;
	    }

	    lerpValue = curve.Evaluate(lerpTime);
	    Debug.Log(lerpValue);

	    transform.position = Vector3.LerpUnclamped(backgroundPosition, foregroundPosition, lerpValue);
	    renderer.material.SetColor("_Color", Color.LerpUnclamped(backgroundColor, foregroundColor, lerpValue));

	    if (lerpValue >= 0.5f && !collider.enabled)
	    {
	        collider.enabled = true;
	    }
	    else if (lerpValue < 0.5f && collider.enabled)
	    {
	        collider.enabled = false;
	    }

	}

    public void MoveToForeground()
    {
        state = State.Moving;
        movingToForeground = true;
    }

    public void MoveToBackground()
    {
        state = State.Moving;
        movingToForeground = false;
    }

    private void OnMouseDown()
    {
        Debug.Log("Mouse down");

        switch (state)
        {
            case State.Wall:
                MoveToForeground();
                break;
            case State.Platform:
                MoveToBackground();
                break;
            case State.Moving:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void GotoState(State newState)
    {
        switch (newState)
        {
            case State.Wall:
                MoveToBackground();
                break;
            case State.Platform:
                MoveToForeground();
                break;
            case State.Moving:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
