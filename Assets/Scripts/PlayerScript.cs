﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]

public class PlayerScript : MonoBehaviour {

	public float maxSpeed = 10f;
	public float jumpForce = 700f;
    public float springForce = 400;

    private Vector2 velocity = new Vector2(0, 0);
    public float gravity = 9.81f;

    Animator animator;

	bool isGrounded = false;
    private bool sideFree;
    private bool onSpring = false;
	bool isJumping = false;
	bool leftGround = false;
	bool facingRight = true;

	public Transform groundCheckLeft;
    public Transform groundCheckRight;
    private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);

    public Transform sideCheckTop;
    public Transform sideCheckBottom;

    public Transform headCheckLeft;
    public Transform headCheckRight;

    public LayerMask layersToLandOn;
	private Rigidbody2D rigidbody2D;

    private BoxCollider2D collider2D;

	// Use this for initialization
	void Start ()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	    collider2D = GetComponent<BoxCollider2D>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    RaycastHit2D raycast;

	    isGrounded =
	        Physics2D.Linecast(groundCheckRight.position - groundCheckOffset, groundCheckRight.position - groundCheckOffset, 1 << LayerMask.NameToLayer("Solid"));


	    if (onSpring)
	    {
	        velocity.y = springForce;
	        onSpring = false;
	    }


	    if (!isGrounded)
	    {
	        velocity.y -= gravity * Time.deltaTime;

	        raycast = CheckGround();

	        if (raycast && raycast.distance - groundCheckOffset.y <= -(velocity.y * Time.deltaTime))
	        {
	            Debug.Log("Ground check: " + raycast.distance);
	            velocity.y = -(raycast.distance - groundCheckOffset.y) / Time.deltaTime;

	            if (raycast.collider.CompareTag("Spring"))
	            {
	                var spring = raycast.collider.GetComponent<Spring>();
	                spring.OnPlayerCollision();
	                onSpring = true;
	            }
	        }

	        raycast = CheckHead();

	        if (raycast && raycast.distance <= velocity.y * Time.deltaTime)
	        {
	            Debug.Log("Can't jump");
	            velocity.y = raycast.distance / Time.deltaTime;
	        }

	        if (velocity.y > 0 && Input.GetButtonUp("Jump"))
	        {
	            velocity.y /= 2;
	        }
	    }
	    else
	    {
	        if (velocity.y < 0)
	        {
	            velocity.y = 0;
	        }

	        raycast = CheckGround();

	        if (raycast)
	        {
	            if (raycast.collider.CompareTag("Spring"))
	            {
	                var spring = raycast.collider.GetComponent<Spring>();
	                spring.OnPlayerCollision();
	                onSpring = true;
	            }

	            Debug.Log("Standing on " + raycast.collider.name + " with distance " + raycast.distance);

	            if (raycast.distance < groundCheckOffset.y)
	            {
	                velocity.y += (groundCheckOffset.y - raycast.distance);
	            }
	        }

	        if (Input.GetButtonDown("Jump"))
	        {
	            velocity.y += jumpForce;
	        }
	    }

	    ///
	    /// Horizontal movement
	    ///

	    var move = Input.GetAxisRaw("Horizontal");

	    if ( move > 0 && !facingRight
	         || move < 0 && facingRight )
	    {
	        Flip ();
	    }

	    velocity.x = move * maxSpeed;

	    raycast = CheckSide(move);

	    var blocked = raycast && raycast.distance <= Mathf.Abs(velocity.x * Time.deltaTime);

	    if (blocked)
	    {
	        Debug.Log(raycast.collider.name + " is stopping our movement!");
	        Debug.Log("Side check: " + raycast.distance);

	        velocity.x = raycast.distance * Mathf.Sign(move) / Time.deltaTime;
	    }

	    transform.position += (Vector3)velocity * Time.deltaTime;

	    ///
	    /// Collisions
	    ///

	    var collisions = Physics2D.OverlapBoxAll(transform.position + (Vector3) collider2D.offset, collider2D.size, 0);

	    foreach (var collision in collisions)
	    {
	        if (collision.CompareTag("Pickup"))
	        {
	            var pickup = collision.GetComponent<Pickup>();
	            pickup.OnPlayerCollision();
	        }
	    }

	    /*
		animator.SetBool("IsGrounded", isGrounded );
		animator.SetBool("IsJumping", isJumping );

		if (oldIsGrounded && !isGrounded)
		{
			leftGround = true;
			animator.SetTrigger("LeftGround");
		}*/


	}

    private RaycastHit2D CheckSide(float sign)
    {
        var bottom = Physics2D.Raycast(sideCheckBottom.position, Vector2.right * Mathf.Sign(sign), 10,
            1 << LayerMask.NameToLayer("Solid"));

        var top = Physics2D.Raycast(sideCheckTop.position, Vector2.right * Mathf.Sign(sign), 10,
            1 << LayerMask.NameToLayer("Solid"));

        if (top && bottom)
        {
            return bottom.distance > top.distance ? top : bottom;
        }

        return top ? top : bottom;
    }

    private RaycastHit2D CheckGround()
    {
        var left = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 10,
            1 << LayerMask.NameToLayer("Solid"));

        var right = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 10,
            1 << LayerMask.NameToLayer("Solid"));

        if (left && right)
        {
            return left.distance > right.distance ? right : left;
        }

        return left ? left : right;
    }

    private RaycastHit2D CheckHead()
    {
        var left = Physics2D.Raycast(headCheckLeft.position, Vector2.up, 10,
            1 << LayerMask.NameToLayer("Solid"));

        var right = Physics2D.Raycast(headCheckRight.position, Vector2.up, 10,
            1 << LayerMask.NameToLayer("Solid"));

        if (left && right)
        {
            return left.distance > right.distance ? right : left;
        }

        return left ? left : right;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.collider;

        if (other.CompareTag("Spring"))
        {
            if (collision.contacts[0].point.y < transform.position.y)
            {
                isJumping = true;
                leftGround = false;
                rigidbody2D.AddForce(new Vector2(0, springForce));
            }
        }
        else if (other.CompareTag("Tile"))
        {
            if (collision.contacts[0].point.y < transform.position.y)
            {
                isGrounded = true;
            }
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
