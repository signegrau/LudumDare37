using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class PlayerScript : MonoBehaviour {

	public float maxSpeed = 10f;
	public float jumpForce = 700f;
    float springForce = 400;

    Animator animator;

	bool isGrounded = false;
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

	    isGrounded =
	        Physics2D.Linecast(groundCheckStart.position, groundCheckEnd.position, 1 << LayerMask.NameToLayer("Solid"));


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



	}

	void Flip ()
	{
		Vector3 tmp = transform.localScale;
		tmp.x *= -1;
		transform.localScale = tmp;

		facingRight = !facingRight;
	}
}
