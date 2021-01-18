using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton
{
    private Rigidbody2D rb;
    private Collider2D col;

    private enum State
    {
        Idle,
        Moving,
        Jumping,
        Attacking,
        Dashing,
        Dead
    }
    private State curState, prevState;

    private bool isDead = false;
    public int maxHealth = 100;
    private int curHealth;
    private HealthBar healthBar;

    private Animator animator;

    private bool canMove = true;
    private bool facingRight = true;
    private Vector2 moveDir = Vector2.zero;
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
    private bool canAttack = true;
    private float attackCooldown = .5f;
    private bool hitboxEnabled = false;
    private int attackNum = 0;

    private bool canDash = true;
    private float dashCooldown = 1.0f;
    [SerializeField] private float dashForce = 4.0f;

    [SerializeField]
    private float platformCollisionTime = .2f;

    private bool isVulnerable = true;
    private bool _isHurt = false;
    public bool isHurt
    {
        get { return _isHurt; }
    }

    private Transform hitboxPoint;
    private LayerMask enemyLayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        groundCheck = transform.GetChild(0);
        hitboxPoint = transform.GetChild(1);
        animator = GetComponent<Animator>();
        groundLayer = LayerMask.GetMask("Ground");
        enemyLayer = LayerMask.GetMask("Enemy");
        healthBar = GameObject.Find("Health Bar").GetComponent<HealthBar>();

        curState = State.Idle;
        prevState = curState;
        curHealth = maxHealth;

        if (healthBar)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    private void Update()
    {
        if (!isDead)
        {
            switch (curState)
            {
                case State.Idle:
                    IdleState();
                    break;
                case State.Moving:
                    MovementState();
                    break;
                case State.Jumping:
                    JumpState();
                    break;
                case State.Attacking:
                    AttackState();
                    break;
                case State.Dashing:
                    DashState();
                    break;
                case State.Dead:
                    DeadState();
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        PlayerPhysics();
    }

    private Vector2 CurrentDirection()
    {
        if (facingRight)
            moveDir = Vector2.right;
        else
            moveDir = Vector2.left;
        return moveDir;
    }

    private void IdleState()
    {
        if (canMove)
        {
            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                curState = State.Moving;
            }
            else if (Input.GetAxisRaw("Horizontal") == 0)
            {
                // Change sprite anim to idle
            }
            if (!Input.GetButton("Down") && Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                curState = State.Jumping;
            }
            else if (Input.GetButton("Down") && Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                StartCoroutine(CooldownTimer(i => { col.enabled = i; }, platformCollisionTime));
            }
            if (Input.GetMouseButtonDown(0) && canAttack)
            {
                animator.SetTrigger("Attack");
                prevState = curState;
                curState = State.Attacking;
            }
            if (Input.GetMouseButtonDown(1) && canDash && isGrounded)
            {
                curState = State.Dashing;
            }
        }
    }

    private void MovementState()
    {
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            if (!facingRight)
            {
                FlipSprite();
            }
        }
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            if (facingRight)
            {
                FlipSprite();
            }
            
        }

        RaycastHit2D wallCheck = Physics2D.Raycast(groundCheck.position, 
            CurrentDirection(), 0.54f, groundLayer);

        // Horizontal movement
        if (canMove)
        {
            float moveX = Input.GetAxisRaw("Horizontal") * (isGrounded ? 1f : airMult) * speed;
            Vector2 targetVelocity = new Vector2(moveX, rb.velocity.y);

            if (!wallCheck.collider)
            {
                rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref vel, moveSmoothing);
            }
        }

        // Change states
        if (!Input.GetButton("Down") && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            curState = State.Jumping;
        }
        else if (Input.GetButton("Down") && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            StartCoroutine(CooldownTimer(i => { col.enabled = i; }, platformCollisionTime));
        }
        if (Input.GetAxisRaw("Horizontal") == 0 && isGrounded)
        {
            curState = State.Idle;
        }
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            animator.SetTrigger("Attack");
            prevState = curState;
            curState = State.Attacking;
        }
        if (Input.GetMouseButtonDown(1) && canDash && isGrounded)
        {
            curState = State.Dashing;
        }
    }

    private void FlipSprite()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void JumpState()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        curState = State.Idle;
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

    private void AttackState()
    {
        if (hitboxEnabled)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitboxPoint.position, new Vector2(1f, .9f), 0, enemyLayer);
            if (hitEnemies.Length > 0)
            {
                foreach (Collider2D enemy in hitEnemies)
                {
                    EnemyController ec = enemy.GetComponent<EnemyController>();
                    if (ec)
                    {
                        if (!ec.isHurt)
                        {
                            ec.TakeDamage(atkDamage);
                        }
                    }
                }
                // Attack chain moves if previous hit connects
                attackNum = (attackNum + 1) % 3;
                animator.SetInteger("AttackNum", attackNum);
            }
        }
    }

    #region Attack Animation Event Functions
    private void StartAttackAnimation()
    {
        canAttack = false;
    }

    
    private void EnableAttackHitbox()
    {
        hitboxEnabled = true;
    }

    private void DisableAttackHitbow()
    {
        hitboxEnabled = false;
        canAttack = true;
    }

    private void FinishAttackAnimation()
    {
        attackNum = 0;
        StartCoroutine(CooldownTimer(i => { canAttack = i; }, attackCooldown));
        curState = prevState;
    }
    #endregion

    //private void OnDrawGizmosSelected()
    //{
    //    if (hitboxPoint == null)
    //        return;

    //    Gizmos.DrawCube(hitboxPoint.position, new Vector2(1f, .9f));
    //}

    private void DashState()
    {
        animator.SetTrigger("Dash");
        StartCoroutine(Dash());
        curState = prevState;
    }

    private IEnumerator Dash()
    {
        canMove = false;
        rb.AddForce(CurrentDirection() * dashForce, ForceMode2D.Impulse);
        isVulnerable = false;
        yield return new WaitForSeconds(.2f);
        rb.velocity = new Vector2(0, rb.velocity.y);
        isVulnerable = true;
        canMove = true;
        yield return StartCoroutine(CooldownTimer(i => { canDash = i; }, dashCooldown));
    }

    private IEnumerator CooldownTimer(System.Action<bool> toggleVar, float time)
    {
        toggleVar(false);
        yield return new WaitForSeconds(time);
        toggleVar(true);
    }

    public void TakeDamage(int damage)
    {
        if (curHealth > 0 && isVulnerable)
        {
            curHealth -= damage;
            animator.SetTrigger("TakeDamage");
        }
        if (curHealth <= 0)
        {
            curHealth = 0;
            curState = State.Dead;
        }
        healthBar.SetHealth(curHealth);
    }

    #region Hurt Animation Event Functions
    private void StartHurtAnimation()
    {
        _isHurt = true;
        isVulnerable = false;
    }

    private void FinishHurtAnimation()
    {
        _isHurt = false;
        isVulnerable = true;
    }
    #endregion

    private void DeadState()
    {
        isDead = true;
        Destroy(gameObject);
    }
}
