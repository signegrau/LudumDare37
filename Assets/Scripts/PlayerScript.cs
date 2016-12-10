using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class PlayerScript : MonoBehaviour {

	public float maxSpeed = 10f;
	public float jumpForce = 700f;

	Animator animator;

	bool isGrounded = false;
	bool isJumping = false;
	bool leftGround = false;
	bool facingRight = true;

	public Transform groundCheck;

	public LayerMask layersToLandOn;
	private Rigidbody2D rigidbody2D;

	// Use this for initialization
	void Start ()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		bool oldIsGrounded = isGrounded;

		isGrounded = Physics2D.OverlapCircle( groundCheck.position, 0.1f, layersToLandOn );


		if ( isGrounded )
		{
			if ( Input.GetButtonDown("Jump") )
			{
				isJumping = true;
				leftGround = false;
				rigidbody2D.AddForce( new Vector2 ( 0, jumpForce ) );
			}
			else if ( leftGround )
			{
				isJumping = false;
			}
		}

		animator.SetBool("IsGrounded", isGrounded );
		animator.SetBool("IsJumping", isJumping );

		if (oldIsGrounded && !isGrounded)
		{
			leftGround = true;
			animator.SetTrigger("LeftGround");
		}


	}

	void FixedUpdate ()
	{

		float move = Input.GetAxisRaw("Horizontal") * maxSpeed;

		Vector2 v = rigidbody2D.velocity;

		v.x = move;

		rigidbody2D.velocity = v;

		animator.SetFloat("Speed", Mathf.Abs( rigidbody2D.velocity.x ) );

		if ( rigidbody2D.velocity.x > 0 && !facingRight
		  || rigidbody2D.velocity.x < 0 && facingRight )
		{
			Flip ();
		}

	}

	void Flip ()
	{
		Vector3 tmp = transform.localScale;
		tmp.x *= -1;
		transform.localScale = tmp;

		facingRight = !facingRight;
	}
}
