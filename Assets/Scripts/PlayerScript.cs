using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]

public class PlayerScript : MonoBehaviour {

	public float maxSpeed = 10f;
	public float jumpForce = 700f;
    public float springForce = 400;
    public float boostUpForce = 400;

    private Vector2 velocity = new Vector2(0, 0);
    public float gravity = 9.81f;

    Animator animator;

	bool isGrounded;
    private bool wasGrouneded;
    private bool sideFree;
    private bool onSpring;
    private bool boostUp;
	bool isJumping = false;
	bool leftGround = false;
	bool facingRight = true;
    private bool isBall;

    public GameObject bloodExplosion;
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

    private Vector3 startPosition;

    private float timeFromGround;

	// Use this for initialization
	void Start ()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	    collider2D = GetComponent<BoxCollider2D>();

	    startPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
	    wasGrouneded = isGrounded;

	    isBall = isBall || isJumping;

	    if (transform.position.y < -6)
	    {
	        Respawn();
	    }

	    RaycastHit2D raycast = CheckGround();

	    isGrounded = !(!raycast || raycast.distance > groundCheckOffset.y + 0.01f);

	    if (onSpring)
	    {
	        velocity.y = springForce;
	        onSpring = false;
	        isJumping = false;
	        isBall = true;
	    }

	    if (boostUp)
	    {
	        isJumping = false;
	        isBall = true;
	        boostUp = false;
	    }


	    if (!isGrounded)
	    {
	        velocity.y -= gravity * Time.deltaTime;

	        if (raycast && raycast.distance - groundCheckOffset.y <= -(velocity.y * Time.deltaTime))
	        {
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
	            velocity.y = raycast.distance / Time.deltaTime;
	        }

	        if (isJumping && velocity.y > 0 && Input.GetButtonUp("Jump"))
	        {
	            velocity.y /= 2;
	        }

	        if (Input.GetButtonDown("Jump") && timeFromGround < 0.15f && !isJumping)
	        {
	            velocity.y += jumpForce;
	            isJumping = true;
	        }

	        timeFromGround += Time.deltaTime;
	    }
	    else
	    {

	        isJumping = false;
	        isBall = false;
	        timeFromGround = 0;

	        if (velocity.y < 0)
	        {
	            velocity.y = 0;
	        }

	        raycast = CheckGround();

	        if (raycast)
	        {

                //if (raycast.collider.CompareTag("Spring"))
                switch(raycast.collider.tag)
                {
                    case "Spring":
    	            {
    	                var spring = raycast.collider.GetComponent<Spring>();
    	                spring.OnPlayerCollision();
    	                onSpring = true;
    	            } break;
                    case "Spike":
                    {
                        // ...
                    } break;
                }


	            if (raycast.distance <= groundCheckOffset.y + Mathf.Epsilon)
	            {
	                velocity.y += (groundCheckOffset.y - raycast.distance);
	            }
	        }

	        if (Input.GetButtonDown("Jump"))
	        {
	            velocity.y += jumpForce;
	            isJumping = true;
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
	            startPosition = collision.transform.position;

	            var pickup = collision.GetComponent<Pickup>();
	            pickup.OnPlayerCollision();
	        }
	        else if (collision.CompareTag("Spike"))
	        {
	            Respawn();
	        }
	    }

	    animator.SetFloat("Speed", Mathf.Abs(velocity.x / maxSpeed));
		animator.SetBool("IsGrounded", isGrounded );
		animator.SetBool("IsJumping", isBall );

		if (wasGrouneded && !isGrounded)
		{
			leftGround = true;
			animator.SetTrigger("LeftGround");
		}


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

    private void Respawn()
    {
        Instantiate(bloodExplosion, transform.position, Quaternion.identity);
        transform.position = startPosition;
        velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BoostUp"))
        {
            boostUp = true;

            velocity.y = boostUpForce;
            velocity.y += other.transform.position.y - transform.position.y;
        }
    }
}
