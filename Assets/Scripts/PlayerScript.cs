using UnityEngine;
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

	public Transform groundCheckStart;
    public Transform groundCheckEnd;

    public Transform sideCheckStart;
    public Transform sideCheckEnd;

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
		bool oldIsGrounded = isGrounded;

	    isGrounded =
	        Physics2D.Linecast(groundCheckStart.position, groundCheckEnd.position, 1 << LayerMask.NameToLayer("Solid"));


	    if (onSpring)
	    {
	        velocity.y = springForce;
	        onSpring = false;
	    }


	    if (!isGrounded)
	    {
	        velocity.y -= gravity * Time.deltaTime;

	        var raycast = Physics2D.Raycast(transform.position, Vector3.down, 10, 1 << LayerMask.NameToLayer("Solid"));

	        if (raycast && raycast.distance <= -(velocity.y * Time.deltaTime))
	        {
	            velocity.y = -raycast.distance / Time.deltaTime;

	            if (raycast.collider.CompareTag("Spring"))
	            {
	                var spring = raycast.collider.GetComponent<Spring>();
	                spring.OnPlayerCollision();
	                onSpring = true;
	            }
	        }

	        raycast = Physics2D.Raycast(transform.position, Vector3.up, 10, 1 << LayerMask.NameToLayer("Solid"));

	        if (raycast && raycast.distance <= velocity.y * Time.deltaTime)
	        {
	            velocity.y = 0;
	        }

	        raycast =
	            Physics2D.Raycast(transform.position + new Vector3(0, collider2D.size.y, 0), Vector2.up, 10, 1 << LayerMask.NameToLayer("Solid"));

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

	        var raycast = Physics2D.Raycast(transform.position + new Vector3(0, 0.2f, 0), Vector3.down, 10, 1 << LayerMask.NameToLayer("Solid"));

	        if (raycast)
	        {
	            if (raycast.collider.CompareTag("Spring"))
	            {
	                var spring = raycast.collider.GetComponent<Spring>();
	                spring.OnPlayerCollision();
	                onSpring = true;
	            }
	        }

	        if (Input.GetButtonDown("Jump"))
	        {
	            velocity.y += jumpForce;
	        }
	    }

	    float move = Input.GetAxisRaw("Horizontal");

	    if ( move > 0 && !facingRight
	         || move < 0 && facingRight )
	    {
	        Flip ();
	    }

	    sideFree = !Physics2D.Linecast(sideCheckStart.position, sideCheckEnd.position, 1 << LayerMask.NameToLayer("Solid"));

	    if (sideFree)
	    {
	        velocity.x = move * maxSpeed;

	        var raycast =
	            Physics2D.Raycast(transform.position + new Vector3(Mathf.Sign(move) * collider2D.size.x / 2, collider2D.size.y / 2, 0),
	                Vector3.right * Mathf.Sign(move), 10, 1 << LayerMask.NameToLayer("Solid"));

	        if (raycast && raycast.distance <= Mathf.Abs(velocity.x * Time.deltaTime))
	        {
	            velocity.x = raycast.distance * Mathf.Sign(move) / Time.deltaTime;
	        }
	    }
	    else
	    {
	        velocity.x = 0;
	    }

	    transform.position += (Vector3)velocity * Time.deltaTime;

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
		}*/


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

    void FixedUpdate ()
    {
/*
        bool sideFree =
            !Physics2D.Linecast(sideCheckStart.position, sideCheckEnd.position, 1 << LayerMask.NameToLayer("Solid"));

        float move = Input.GetAxisRaw("Horizontal") * maxSpeed;

        if ( move > 0 && !facingRight
             || move < 0 && facingRight )
        {
            Flip ();
        }

        if (!sideFree)
            move = 0;

		Vector2 v = rigidbody2D.velocity;

		v.x = move;

		rigidbody2D.velocity = v;

		animator.SetFloat("Speed", Mathf.Abs( rigidbody2D.velocity.x ) );


*/
	}

	void Flip ()
	{
		Vector3 tmp = transform.localScale;
		tmp.x *= -1;
		transform.localScale = tmp;

		facingRight = !facingRight;
	}
}
