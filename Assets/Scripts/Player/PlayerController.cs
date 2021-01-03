using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;

    private enum PlayerState
    {
        Idle,
        Moving,
        Jumping,
        Attacking,
        Dashing,
        Dead
    }
    private PlayerState CurPlayerState;

    private bool isDead = false;
    public int health = 100;


    private bool canMove = true;
    private bool facingRight = true;
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float fallMult = 2.5f;
    [Range(0, 1)] [SerializeField] private float airMult = .85f;
    [Range(0, .3f)] [SerializeField] private float moveSmoothing = .05f;
    private bool isGrounded = false;
    private Vector2 vel = Vector2.zero;
    private Transform groundCheck;
    [SerializeField] private float checkGroundRadius = .05f;
    private LayerMask groundLayer;

    private int atkDamage = 75;

    private Transform hitboxPoint;
    private LayerMask enemyLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheck = transform.GetChild(0);
        hitboxPoint = transform.GetChild(1);
        groundLayer = LayerMask.GetMask("Ground");
        enemyLayer = LayerMask.GetMask("Enemy");

        CurPlayerState = PlayerState.Idle;
    }

    void Update()
    {
        if (!isDead)
        {
            switch (CurPlayerState)
            {
                case PlayerState.Idle:
                    Idle();
                    break;
                case PlayerState.Moving:
                    Movement();
                    break;
                case PlayerState.Jumping:
                    Jump();
                    break;
                case PlayerState.Attacking:
                    Attack();
                    break;
                case PlayerState.Dashing:
                    Dash();
                    break;
                case PlayerState.Dead:
                    break;
                default: break;
            }
        }
    }

    private void FixedUpdate()
    {
        PlayerPhysics();
    }

    private void Idle()
    {
        if (canMove)
        {
            if (Input.GetAxisRaw("Horizontal") == 0)
            {
                // Change sprite anim to idle
            }
            else if (Input.GetAxisRaw("Horizontal") != 0)
            {
                CurPlayerState = PlayerState.Moving;
            }
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                CurPlayerState = PlayerState.Jumping;
            }
        }
    }

    private void Movement()
    {
        Vector2 moveDir = Vector2.zero;

        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            moveDir = Vector2.right;

            if (!facingRight)
            {
                FlipSprite();
            }
        }
        else if (Input.GetAxisRaw("Horizontal") < 0)
        {
            moveDir = Vector2.left;

            if (facingRight)
            {
                FlipSprite();
            }
            
        }

        RaycastHit2D wallCheck = Physics2D.Raycast(groundCheck.position, 
            moveDir, 0.54f, groundLayer);

        // Horizontal movement
        if (canMove)
        {
            float moveX = Input.GetAxisRaw("Horizontal") * (isGrounded ? 1f : airMult) * speed;
            Vector2 targetVelocity = new Vector2(moveX, rb.velocity.y);

            if (wallCheck.collider == false)
            {
                rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref vel, moveSmoothing);
            }
        }

        // Change states
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            CurPlayerState = PlayerState.Jumping;
        }
        else if (Input.GetAxisRaw("Horizontal") == 0 && isGrounded)
        {
            CurPlayerState = PlayerState.Idle;
        }
    }

    private void FlipSprite()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        CurPlayerState = PlayerState.Idle;
    }

    private void PlayerPhysics()
    {
        // Grounded check
        Collider2D col = Physics2D.OverlapCircle(groundCheck.position, 
            checkGroundRadius, groundLayer);

        if (col != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        // Jumping physics
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity * (fallMult - 1) * Time.fixedDeltaTime;
        }
    }

    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitboxPoint.position, new Vector2(1f, .9f), enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyController>().TakeDamage(atkDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hitboxPoint == null)
            return;

        Gizmos.DrawCube(hitboxPoint.position, new Vector2(1f, .9f));
    }

    private void Dash()
    {

    }

    public void TakeDamage(int damage)
    {
        if (health > 0)
        {
            health -= damage;
        }
        if (health <= 0)
        {
            health = 0;
            Death();
        }
    }

    private void Death()
    {
        isDead = true;
    }
}
