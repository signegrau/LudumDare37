using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]

public class PlayerScript : MonoBehaviour {

    public delegate void DeathHandler();

    public static event DeathHandler OnDeath;
	public float maxSpeed = 10f;
	public float jumpForce = 700f;
    public float springForce = 400;
    public float boostUpForce = 400;
    public float boostSideForce = 400;

    private Vector2 velocity = new Vector2(0, 0);

    public float gravity = 9.81f;
    public float jumpHoldGravity;
    public float fallingGravity;

    private float currentGravity;
    public float drag = 0.05f;

    Animator animator;

	bool isGrounded;
    private bool wasGrouneded;
    private bool sideFree;
    private bool onSpring;
    private bool boostUp;
    private bool boostSide;
    private bool boostLeft;
    private bool boostRight;
	bool isJumping = false;
    bool hasJumped = false;
	bool leftGround = false;
	bool facingRight = true;
    private bool isBall;

    public GameObject bloodExplosion;
	public Transform groundCheckLeft;
    public Transform groundCheckRight;
    private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);

    public Transform sideCheckTop;
    public Transform sideCheckBottom;

    public Transform altSideCheckTop;
    public Transform altSideCheckBottom;

    public Transform headCheckLeft;
    public Transform headCheckRight;

    public LayerMask layersToLandOn;
	private Rigidbody2D rigidbody2D;

    private CapsuleCollider2D collider2D;

    private Vector3 startPosition;

    private float timeFromGround;

    private List<Collider2D> previousColliders = new List<Collider2D>();

    private float uncontrollableTimer;

	private int FootStepAnimationEvent() {
		SoundManager.single.PlayFootstepSound();
		return 0;
	}

	// Use this for initialization
	void Start ()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		startPosition = transform.position;
		collider2D = GetComponent<CapsuleCollider2D>();

		startPosition = transform.position;
		currentGravity = gravity;
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

        if (onSpring)
        {
            velocity.y = springForce;
            onSpring = false;
            isJumping = false;
            isBall = false;
        }

        if (boostUp)
        {
            isJumping = false;
            isBall = true;
            boostUp = false;
            boostSide = false;
			SoundManager.single.PlayBoostSound();

        }

        if (boostSide)
        {
            isJumping = false;
            isBall = true;
			SoundManager.single.PlayBoostSound();
        }
			
        RaycastHit2D raycast = CheckGround();

        isGrounded = raycast && raycast.distance <= groundCheckOffset.y + 0.01f;
        isGrounded = isGrounded && velocity.y <= 0;

        if (!isGrounded)
	    {
	        velocity.y -= currentGravity * Time.deltaTime;

	        if (raycast && raycast.distance - groundCheckOffset.y <= -(velocity.y * Time.deltaTime) && velocity.y <= 0)
	        {
	            velocity.y = -(raycast.distance - groundCheckOffset.y) / Time.deltaTime;

	            if (raycast.collider.CompareTag("Spring") && velocity.y <= 0)
	            {
	                currentGravity = gravity;
	                var spring = raycast.collider.GetComponent<Spring>();
	                spring.OnPlayerCollision();
	                onSpring = true;
                    velocity.y = 0;
                    Debug.Log("ho");
	            }
	        }

	        raycast = CheckHead();

	        if (raycast && raycast.distance <= velocity.y * Time.deltaTime && velocity.y <= 0)
	        {
	            velocity.y = raycast.distance / Time.deltaTime;
	        }



	        timeFromGround += Time.deltaTime;
	    }
	    else
	    {
            hasJumped = false;
	        isJumping = false;
            isBall = false;
            timeFromGround = 0;
            currentGravity = gravity;
	        velocity.y = 0;

	        if (raycast)
	        {

                //if (raycast.collider.CompareTag("Spring"))
                if (raycast.collider.CompareTag("Spring") && velocity.y <= 0)
                {
                    currentGravity = gravity;
                    var spring = raycast.collider.GetComponent<Spring>();
                    spring.OnPlayerCollision();
                    onSpring = true;
                    velocity.y = 0;
                    Debug.Log("hi");
                }


	            if (raycast.distance < groundCheckOffset.y - 0.01f && !onSpring)
	            {
                    velocity.y += (groundCheckOffset.y - raycast.distance);
	            }
	        }
	    }

	    if (Input.GetButtonDown("Jump") && (isGrounded || (!hasJumped)))
	    {
	        currentGravity = jumpHoldGravity;

	        velocity.y = jumpForce;
	        isJumping = true;
            hasJumped = true;
			SoundManager.single.PlayJumpSound();

	    }

	    if (Input.GetButtonUp("Jump") && isJumping)
	    {
	        currentGravity = gravity;
	        isJumping = false;
	    }

	    if (!isJumping && velocity.y < 0)
	    {
	        currentGravity = fallingGravity;
	    }

	    ///
	    /// Horizontal movement
	    ///

	    float move = 0;
	    if (uncontrollableTimer <= 0)
	    {
	        move = Input.GetAxisRaw("Horizontal");
	        velocity.x = move * maxSpeed;
	    }
	    else
	    {
	        uncontrollableTimer -= Time.deltaTime;

	        if (Mathf.Abs(velocity.x - drag * Time.deltaTime * Mathf.Sign(velocity.x)) > 0)
	        {
	            velocity.x -= drag * Time.deltaTime * Mathf.Sign(velocity.x);
	        }
	        else
	        {
	            velocity.x = 0;
	        }
	    }


	    if ( move > 0 && !facingRight
	         || move < 0 && facingRight )
	    {
	        Flip ();
	    }

	    raycast = CheckSide(velocity.x);

	    var blocked = raycast && raycast.distance <= Mathf.Abs(velocity.x * Time.deltaTime);

	    if (blocked)
	    {
	        velocity.x = raycast.distance * Mathf.Sign(velocity.x) / Time.deltaTime;
	    }

	    if (boostSide)
	    {
	        //velocity.y = 0;
	    }

	    transform.position += (Vector3)velocity * Time.deltaTime;

        ///
        /// Collisions
        ///
        var colliders =
	        Physics2D.OverlapCapsuleAll(transform.position + (Vector3) collider2D.offset, collider2D.size, collider2D.direction, 0);
	    foreach (var collider in colliders)
	    {
	        if (collider.CompareTag("Pickup"))
	        {
	            startPosition = collider.transform.position;

	            var pickup = collider.GetComponent<Pickup>();
	            pickup.OnPlayerCollision();
	        }
	        else if (collider.CompareTag("Spike"))
	        {
	            Respawn();
	        }

	        if (previousColliders.Contains(collider)) continue;

	        if (collider.CompareTag("BoostUp"))
	        {
	            isBall = true;
	            isJumping = false;
	            boostUp = true;
	            currentGravity = gravity;
	            velocity.y = boostUpForce;
	            velocity.y += collider.transform.position.y - transform.position.y;
	        }
	        else if (collider.CompareTag("BoostLeft"))
	        {
	            boostSide = true;

	            isBall = true;

	            isJumping = false;
	            currentGravity = gravity;

	            velocity.x = -boostSideForce;
	            //velocity.x -= collider.transform.position.x - transform.position.x;
	            uncontrollableTimer = 0.5f;
	            velocity.y = 3f;
	        }
	        else if (collider.CompareTag("BoostRight"))
	        {
	            boostSide = true;

	            isBall = true;

	            isJumping = false;
	            currentGravity = gravity;

	            velocity.x = boostSideForce;
	            //velocity.x += collider.transform.position.x - transform.position.x;
	            velocity.y = 3f;

	            uncontrollableTimer = 0.5f;
	        }
	    }

	    previousColliders = colliders.ToList();

	    animator.SetFloat("Speed", Mathf.Abs(velocity.x / maxSpeed));
		animator.SetBool("IsGrounded", isGrounded );
		animator.SetBool("IsJumping", isBall );

		if (wasGrouneded && !isGrounded)
		{
			leftGround = true;
			animator.SetTrigger("LeftGround");
		}
		else if(!wasGrouneded && isGrounded) {
			SoundManager.single.PlayLandSound();
		}


	}

    private RaycastHit2D CheckSide(float velocity)
    {
        var rays = new List<RaycastHit2D>
        {
            Physics2D.Raycast(sideCheckBottom.position, Vector2.right * Mathf.Sign(velocity), 10,
                1 << LayerMask.NameToLayer("Solid")),
            Physics2D.Raycast(sideCheckTop.position, Vector2.right * Mathf.Sign(velocity), 10,
                1 << LayerMask.NameToLayer("Solid")),
        };

        if (uncontrollableTimer > 0)
        {
            rays.Add(Physics2D.Raycast(altSideCheckBottom.position, Vector2.right * Mathf.Sign(velocity), 11,
                1 << LayerMask.NameToLayer("Solid")));
            rays.Add(Physics2D.Raycast(altSideCheckTop.position, Vector2.right * Mathf.Sign(velocity), 11,
                1 << LayerMask.NameToLayer("Solid")));
        }

        return rays.Where(ray => ray)
            .OrderBy(ray => ray.distance)
            .FirstOrDefault();
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

    /*private void OnCollisionEnter2D(Collision2D collision)
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
    }*/

	void Flip ()
	{
		Vector3 tmp = transform.localScale;
		tmp.x *= -1;
		transform.localScale = tmp;

		facingRight = !facingRight;
	}

    private void Respawn()
    {
		SoundManager.single.PlaySplatSound();

        isJumping = false;
        isBall = false;

        if (OnDeath != null)
            OnDeath();

        Instantiate(bloodExplosion, transform.position, Quaternion.identity);
        transform.position = startPosition;
        velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.tag);

        if (other.CompareTag("BoostUp"))
        {
            boostUp = true;

            velocity.y = boostUpForce;
            velocity.y += other.transform.position.y - transform.position.y;
        }
        else if (other.CompareTag("BoostLeft"))
        {
            boostSide = true;

            velocity.x = -boostSideForce;
            //velocity.x -= other.transform.position.x - transform.position.x;
        }
        else if (other.CompareTag("BoostRight"))
        {
            boostSide = true;

            velocity.x = boostSideForce;
            
			//velocity.x += other.transform.position.x - transform.position.x;
        }
    }
}
