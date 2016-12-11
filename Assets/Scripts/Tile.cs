using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum State
    {
        Wall,
        Platform,
        Pickup,
        Spring
    }

    public enum TransisionState
    {
        None,
        Background,
        Wall,
        Foreground
    }

    public struct TransisionStateData
    {
        public Vector3 position;
        public Color color;
    }

    public GameObject pickupPrefab;
    public GameObject springPrefab;

    private State state = State.Wall;
    private State previousState;
    private TransisionState transisionState = TransisionState.None;

    private Vector3 previousPosition;
    private Vector3 targetPosition;

    private Color previousColor;
    private Color targetColor;

    private Vector3 basePosition;

    private bool movingToForeground;

    private float lerpTime;
    private float lerpValue;
    public float timeToMoveMin = 1f;
	public float timeToMoveMax = 1.2f;

    private GameObject attachment;
    private Renderer attachmentRenderer;

    public AnimationCurve curve;

    public delegate void MouseClickEventHandler(object sender);
    public event MouseClickEventHandler OnMouseClick;

    private Dictionary<TransisionState, Vector3> targetPositions = new Dictionary<TransisionState, Vector3>
    {
        {TransisionState.Background, new Vector3(0, 0, 1)},
        {TransisionState.Foreground, new Vector3(0, 0, -1)},
        {TransisionState.Wall, new Vector3(0, 0, 0)}
    };

    private Dictionary<TransisionState, Color> targetColors = new Dictionary<TransisionState, Color>
    {
        {TransisionState.Background, new Color(0.0f, 0.0f, 0.0f)},
        {TransisionState.Foreground, new Color(1f, 1f, 1f)},
        {TransisionState.Wall, new Color(0.7f, 0.7f, 0.7f)}
    };

    private Renderer renderer;

    public BoxCollider2D collider;

    // Use this for initialization
	public void Start ()
	{
	    renderer = GetComponent<Renderer>();
	    renderer.material.SetColor("_Color", targetColors[TransisionState.Wall]);

	    basePosition = transform.position;

	    targetPositions[TransisionState.Background] += basePosition;
	    targetPositions[TransisionState.Foreground] += basePosition;
	    targetPositions[TransisionState.Wall] += basePosition;
	}


    private bool transisionFinished;
	public void Update ()
	{
	    if (transisionState == TransisionState.None) return;

		lerpTime += 1f / Random.Range(timeToMoveMin, timeToMoveMax) * Time.deltaTime;

        if (lerpTime >= 1)
	    {
	        lerpTime = 1;
	        transisionFinished = true;
	    }

	    lerpValue = curve.Evaluate(lerpTime);
	    // Debug.Log(lerpValue);

	    transform.position = Vector3.LerpUnclamped(previousPosition, targetPosition, lerpValue);
	    renderer.material.SetColor("_Color", Color.LerpUnclamped(previousColor, targetColor, lerpValue));

	    if (transisionFinished)
	    {
	        lerpTime = 0;

	        Debug.Log(transisionState);

	        if ((state == State.Pickup || state == State.Spring) && transisionState == TransisionState.Background)
	        {
	            switch (state)
	            {
                    case State.Pickup:
	                    AttachObject(pickupPrefab);
	                    break;
	                case State.Spring:
	                    AttachObject(springPrefab);
	                    break;
	                case State.Wall:
	                    break;
	                case State.Platform:
	                    break;
	            }

	            BeginTransision(TransisionState.Wall);
	        }
	        else
	        {
	            transisionState = TransisionState.None;
	        }

	        transisionFinished = false;
	    }

	    if (state != State.Platform && !collider.enabled) return;
	    if (lerpValue >= 0.5f && !collider.enabled)
	    {
	        collider.enabled = true;
	    }
	    else if (lerpValue < 0.5f && collider.enabled)
	    {
	        collider.enabled = false;
	    }
	}

    public void AttachObject(GameObject gameObject)
    {
        attachment = Instantiate(gameObject, transform);
        attachment.transform.localPosition = new Vector3(0, 0, -1f);
        attachmentRenderer = attachment.GetComponent<Renderer>();
    }

    public void BeginTransision(TransisionState transisionState)
    {
        targetPosition = targetPositions[transisionState];
        previousPosition = transform.position;

        targetColor = targetColors[transisionState];
        previousColor = renderer.material.GetColor("_Color");

        this.transisionState = transisionState;
    }

    private void OnMouseDown()
    {
        if (OnMouseClick == null) return;
        OnMouseClick(this);
    }

    public void GotoState(State newState)
    {
        if (state == newState) return;

        switch (newState)
        {
            case State.Wall:
                BeginTransision(TransisionState.Wall);
                break;
            case State.Platform:
                BeginTransision(TransisionState.Foreground);
                break;
            case State.Pickup:
                BeginTransision(TransisionState.Background);
                break;
            case State.Spring:
                BeginTransision(TransisionState.Background);
                break;
        }

        if ((newState != State.Pickup || newState != State.Spring) && attachment != null)
        {
            Destroy(attachment);
            attachment = null;
        }

        state = newState;
    }
}
