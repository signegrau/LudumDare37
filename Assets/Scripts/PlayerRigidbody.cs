using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRigidbody : MonoBehaviour
{
    public delegate void DeathHandler();
    public static event DeathHandler OnDeath;

    private Rigidbody2D _rigidbody2D;
    private Animator _animator;

    public Transform groundCheckLeft;
    public Transform groundCheckRight;
    public GameObject bloodExplosion;

    public float moveSpeed;
    public float jumpSpeed;
    public float springSpeed;
    public float boostUpSpeed;
    public float boostHorizontalSpeed;

    private Transform boostUpTransform;
    private Transform boostLeftTransform;
    private Transform boostRightTransform;

    private float moveInput;

    private bool facingRight = true;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isBall;
    private bool hasRespawned;

    private float timeFromGround;
    private Vector3 startPosition;

    private bool toSpring;
    private bool boostUp;
    private bool boostLeft;
    private bool boostRight;

    private bool isJumping;

    private Vector2 velocity;

    public BoxCollider2D groundCheck;

    private Vector2 groundCheckOffset;
    private Vector2 groundCheckSize;

    private float uncontrollableTimeLeft;

    private void Start ()
	{
	    _rigidbody2D = GetComponent<Rigidbody2D>();
	    _animator = GetComponent<Animator>();

	    groundCheckOffset = groundCheck.offset;
	    groundCheckSize = groundCheck.size;
	    groundCheck.enabled = false;
	}

	private void FixedUpdate ()
	{
	    velocity = _rigidbody2D.velocity;

	    if (transform.position.y < -6)
	    {
	        Respawn();
	    }

	    if (hasRespawned)
	    {
	        velocity.y = 0;
	        hasRespawned = false;
	    }

	    wasGrounded = isGrounded;

	    if (uncontrollableTimeLeft <= 0)
	    {
	        moveInput = Input.GetAxisRaw("Horizontal") * moveSpeed;

	        velocity.x = moveInput;
	    }
	    else
	    {
	        uncontrollableTimeLeft -= Time.fixedDeltaTime;
	    }

	    isGrounded = Physics2D.OverlapBox(transform.position + (Vector3) groundCheckOffset, groundCheckSize, 0,
	        1 << LayerMask.NameToLayer("Solid"));

	    Debug.DrawLine(transform.position - (Vector3)groundCheckSize/2, transform.position + (Vector3)groundCheckSize / 2);

	    if (isGrounded)
	    {
	        //_rigidbody2D.gravityScale = 0;
	        isJumping = false;
	        isBall = false;
	        timeFromGround = 0;


	    }
	    else
	    {
	        //_rigidbody2D.gravityScale = 1.5f;
	        timeFromGround += Time.fixedDeltaTime;

	        if (isJumping && velocity.y > 0 && Input.GetButtonUp("Jump"))
	        {
	            velocity.y /= 2;
	        }
	    }

	    if (Input.GetButtonDown("Jump") && (isGrounded || timeFromGround < 0.2f))
	    {
	        velocity.y = jumpSpeed;
	        isJumping = true;
	    }

	    if (toSpring)
	    {
	        velocity.y = springSpeed;
	        toSpring = false;
	        isJumping = false;
	        isBall = true;
	    }

	    if (boostUp)
	    {
	        velocity.y = boostUpSpeed;
	        velocity.y += boostUpTransform.position.y - transform.position.y;
	        isBall = true;
	        isJumping = false;
	        boostUp = false;
	    }

	    if (boostRight)
	    {
	        velocity.x = boostHorizontalSpeed;
	        velocity.x += boostRightTransform.position.x - transform.position.x;
	        velocity.y = 2f;

	        isBall = true;
	        isJumping = false;
	        boostRight = false;

	        uncontrollableTimeLeft = 0.5f;
	    }

	    if (boostLeft)
	    {
	        velocity.x = -boostHorizontalSpeed;
	        velocity.x -= boostLeftTransform.position.x - transform.position.x;
	        velocity.y = 2f;

	        isBall = true;
	        isJumping = false;
	        boostLeft = false;

	        uncontrollableTimeLeft = 0.5f;
	    }

	    if ( moveInput > 0 && !facingRight
	         || moveInput < 0 && facingRight )
	    {
	        Flip();
	    }

	    _rigidbody2D.velocity = velocity;

	    _animator.SetFloat("Speed", Mathf.Abs(velocity.x / moveSpeed));
	    _animator.SetBool("IsGrounded", isGrounded && timeFromGround < 0.2f );
	    _animator.SetBool("IsJumping", isBall || isJumping);

	    if (wasGrounded && !isGrounded)
	    {
	        //leftGround = true;
	        _animator.SetTrigger("LeftGround");
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
        isJumping = false;
        isBall = false;

        if (OnDeath != null)
            OnDeath();

        Instantiate(bloodExplosion, transform.position, Quaternion.identity);
        transform.position = startPosition;
        hasRespawned = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.collider;

        if (other.CompareTag("Spring"))
        {
            Debug.Log(collision.contacts[0].point.y - transform.position.y);

            if (collision.contacts[0].point.y < transform.position.y + 0.01f)
            {
                toSpring = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pickup"))
        {
            startPosition = other.transform.position;
        }
        else if (other.CompareTag("Spike"))
        {
            Respawn();
        }
        else if (other.CompareTag("BoostUp"))
        {
            boostUpTransform = other.transform;
            boostUp = true;
        }
        else if (other.CompareTag("BoostLeft"))
        {
            boostLeftTransform = other.transform;
            boostLeft = true;
        }
        else if (other.CompareTag("BoostRight"))
        {
            boostRightTransform = other.transform;
            boostRight = true;
        }
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
}
