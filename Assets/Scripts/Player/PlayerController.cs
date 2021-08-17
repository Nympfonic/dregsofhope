using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private Transform hitboxPoint;

    private enum State
    {
        Idle,
        Moving,
        Jumping,
        Attacking,
        Dashing,
        Hurt,
        Dead
    }
    private State curState, prevState;

    [Header("Player State")]
    [SerializeField] private int _maxHealth = 100;
    public int MaxHealth
    {
        get { return _maxHealth; }
        set { _maxHealth = MaxHealth; }
    }
    private int _curHealth;
    public int CurrentHealth
    {
        get { return _curHealth; }
        set { _curHealth = CurrentHealth; }
    }
    private HealthBar healthBar;
    private bool isVulnerable = true;
    private bool _isHurt = false;
    public bool isHurt { get { return _isHurt; } }
    private bool hasPlayedDeathAnim = false;
    private bool _isDead = false;
    public bool isDead { get { return _isDead; } }

    [Header("Misc")]
    private LayerMask enemyLayer;
    private Animator animator;

    [Header("Movement")]
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float jumpForce = 5.0f;
    private bool canMove = true;
    private bool facingRight = true;
    private Vector2 moveDir = Vector2.zero;
    private float fallMult = 2.5f;
    private const float airMult = .85f;
    private const float moveSmoothing = .0018f;
    private bool isGrounded = false;
    private Vector2 vel = Vector2.zero;
    private Transform groundCheck;
    private const float checkGroundRadius = .05f;
    private LayerMask groundLayer;
    private bool canPlatformDrop = false;
    private const float platformCollisionTime = .25f;

    [Header("Attacking")]
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackCooldown = .3f;
    [SerializeField] private float knockbackForce = 5f;
    private bool canAttack = true;
    private bool hitboxEnabled = false;
    private int attackNum = 0;

    [Header("Dashing")]
    [SerializeField] private float dashForce = 4.0f;
    [SerializeField] private float dashCooldown = 1.0f;
    private bool canDash = true;

    private void Initialization()
    {
        curState = State.Idle;
        prevState = curState;
        _curHealth = _maxHealth;
        isVulnerable = true;

        if (healthBar)
        {
            healthBar.SetMaxHealth(_maxHealth);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        groundCheck = transform.GetChild(0);
        hitboxPoint = transform.GetChild(1);
        animator = GetComponent<Animator>();
        groundLayer = LayerMask.GetMask("Ground");
        enemyLayer = LayerMask.GetMask("Enemy");
        healthBar = GameObject.Find("Health Bar").GetComponent<HealthBar>();

        Initialization();
    }

    private void Update()
    {
        if (!_isDead)
        {
            //Debug.Log(curState);
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
                case State.Hurt:
                    HurtState();
                    break;
                case State.Dead:
                    Death();
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        PlayerPhysics();
    }

    /// <summary>
    /// Returns the current direction that the player is facing as a Vector2.
    /// </summary>
    private Vector2 CurrentDirection()
    {
        if (facingRight)
            moveDir = Vector2.right;
        else
            moveDir = Vector2.left;
        return moveDir;
    }

    /// <summary>
    /// Handles the Idle state.
    /// </summary>
    private void IdleState()
    {
        if (canMove)
        {
            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                curState = State.Moving;
            }
            if (!Input.GetButton("Down")
                && Input.GetKeyDown(KeyCode.Space)
                && isGrounded)
            {
                curState = State.Jumping;
            }
            else if (Input.GetButton("Down")
                && Input.GetKeyDown(KeyCode.Space)
                && isGrounded
                && canPlatformDrop)
            {
                StartCoroutine(CooldownTimer(i => { col.enabled = i; }, platformCollisionTime));
            }
            if (Input.GetButtonDown("Attack")
                && canAttack)
            {
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("Attack");
                prevState = curState;
                curState = State.Attacking;
            }
            if (Input.GetButtonDown("Dash")
                && canDash && isGrounded)
            {
                curState = State.Dashing;
            }
        }
    }

    /// <summary>
    /// Handles the Movement state.
    /// </summary>
    private void MovementState()
    {
        if (Input.GetAxisRaw("Horizontal") > 0
            && !facingRight)
        {
            FlipSprite();
        }
        if (Input.GetAxisRaw("Horizontal") < 0
            && facingRight)
        {
            FlipSprite();
        }

        //RaycastHit2D wallCheck = Physics2D.Raycast(groundCheck.position,
        //    CurrentDirection(),
        //    col.size.x / 2 + 0.01f,
        //    groundLayer);

        // Bug where platform also counts as wall - unintended
        Collider2D wallCheck = Physics2D.OverlapArea(new Vector2(groundCheck.position.x, transform.position.y + col.offset.y + col.size.y / 2 - .05f),
            new Vector2(groundCheck.position.x + CurrentDirection().x * (col.size.x / 2 + .01f), groundCheck.position.y + .05f),
            groundLayer);
        //if (wallCheck.Length > 0)
        //{
        //    foreach (Collider2D collider in wallCheck)
        //    {
        //        if (!collider.CompareTag("Ground"))
        //        {
        //            isThereWall = false;
        //            continue;
        //        }
        //        else if (collider.CompareTag("Ground"))
        //        {
        //            isThereWall = true;
        //            break;
        //        }
        //    }
        //}

        // Horizontal movement
        if (canMove)
        {
            float moveX = Input.GetAxisRaw("Horizontal") * (isGrounded ? 1f : airMult) * speed;
            Vector2 targetVelocity = new Vector2(moveX, rb.velocity.y);

            if (!wallCheck)
            {
                rb.velocity = Vector2.SmoothDamp(rb.velocity,
                    targetVelocity,
                    ref vel,
                    moveSmoothing);
            }

            animator.SetBool("IsMoving", true);
        }

        // Change states
        if (!Input.GetButton("Down")
            && Input.GetKeyDown(KeyCode.Space)
            && isGrounded)
        {
            animator.SetBool("IsMoving", false);
            curState = State.Jumping;
        }
        else if (Input.GetButton("Down")
            && Input.GetKeyDown(KeyCode.Space)
            && isGrounded
            && canPlatformDrop)
        {
            animator.SetBool("IsMoving", false);
            StartCoroutine(CooldownTimer(i => { col.enabled = i; }, platformCollisionTime));
        }
        if (Input.GetAxisRaw("Horizontal") == 0
            && isGrounded)
        {
            animator.SetBool("IsMoving", false);
            curState = State.Idle;
        }
        if (Input.GetButtonDown("Attack")
            && canAttack)
        {
            if (isGrounded)
            {
                rb.velocity *= Vector2.up;
            }
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("Attack");
            prevState = curState;
            curState = State.Attacking;
        }
        if (Input.GetButtonDown("Dash")
            && canDash
            && isGrounded)
        {
            animator.SetBool("IsMoving", false);
            curState = State.Dashing;
        }
    }

    /// <summary>
    /// Handles flipping the player's sprite.
    /// </summary>
    private void FlipSprite()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    /// <summary>
    /// Handles the Jump state.
    /// </summary>
    private void JumpState()
    {
        animator.SetBool("IsJumping", true);
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        curState = State.Moving;
    }

    /// <summary>
    /// Handles the fixed physics for the player.
    /// </summary>
    private void PlayerPhysics()
    {
        // Grounded check
        Collider2D groundCol = Physics2D.OverlapArea(new Vector2(groundCheck.position.x - .72f, groundCheck.position.y), 
            new Vector2(groundCheck.position.x + .72f, groundCheck.position.y - checkGroundRadius),
            groundLayer);

        if (groundCol)
        {
            animator.SetBool("IsFalling", false);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        // Platform check
        Collider2D[] platformCol = Physics2D.OverlapAreaAll(new Vector2(groundCheck.position.x - .5f, groundCheck.position.y),
            new Vector2(groundCheck.position.x + .5f, groundCheck.position.y - checkGroundRadius),
            groundLayer);

        /* Check if player is above platform.
         * Do not let player drop down if they are above solid ground. */
        if (platformCol.Length > 0)
        {
            foreach (Collider2D collider in platformCol)
            {
                if (collider.CompareTag("Platform"))
                {
                    canPlatformDrop = true;
                    continue; // continue checking the other colliders for solid ground
                }
                else if (collider.CompareTag("Ground"))
                {
                    canPlatformDrop = false;
                    break; // if they are above solid ground, stop looping, and disable platform dropping
                }
            }
        }

        // Jumping physics
        if (rb.velocity.y < 0)
        {
            if (!isGrounded)
            {
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsFalling", true);
            }
            rb.velocity += Vector2.up * Physics2D.gravity * (fallMult - 1) * Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// Handles the Attack state.
    /// </summary>
    private void AttackState()
    {
        animator.SetBool("IsAttacking", true);
        if (hitboxEnabled)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(hitboxPoint.position,
                new Vector2(1f, .9f), 0, enemyLayer);
            if (hitEnemies.Length > 0)
            {
                foreach (Collider2D enemy in hitEnemies)
                {
                    EnemyArchetype ec = enemy.GetComponent<EnemyArchetype>();
                    if (!ec.IsHurt)
                    {
                        ec.TakeDamage(attackDamage, (ec.transform.position - transform.position).normalized, knockbackForce);
                    }
                }
                // Attack chain moves if previous hit connects
                attackNum = (attackNum + 1) % 2;
                animator.SetInteger("AttackNum", attackNum);
            }
        }
        if (Input.GetButtonDown("Attack")
            && canAttack)
        {
            animator.SetTrigger("Attack");
        }
    }

    #region Attack Animation Event Functions

    /// <summary>
    /// Should be called when the Attack animation begins.
    /// </summary>
    private void StartAttackAnimation()
    {
        canAttack = false;
    }

    /// <summary>
    /// Should be called when the Attack hitbox is enabled.
    /// </summary>
    private void EnableAttackHitbox()
    {
        hitboxEnabled = true;
    }

    /// <summary>
    /// Should be called when the Attack hitbox is disabled.
    /// </summary>
    private void DisableAttackHitbox()
    {
        hitboxEnabled = false;
        canAttack = true;
    }

    /// <summary>
    /// Should be called when the Attack animation ends.
    /// </summary>
    private void FinishAttackAnimation()
    {
        attackNum = 0;
        StartCoroutine(CooldownTimer(i => { canAttack = i; }, attackCooldown));
        curState = prevState;
        animator.SetBool("IsAttacking", false);
    }
    #endregion

    /// <summary>
    /// Handles the Dash state.
    /// </summary>
    private void DashState()
    {
        animator.SetTrigger("Dash");
        StartCoroutine(Dash());
        curState = prevState;
    }

    /// <summary>
    /// Coroutine for applying the Dash force and managing the cooldown.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Dash()
    {
        canMove = false;
        isVulnerable = false;
        rb.velocity = new Vector2(0, rb.velocity.y);
        rb.AddForce(CurrentDirection() * dashForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(.2f);

        rb.velocity = new Vector2(0, rb.velocity.y);
        canMove = true;
        isVulnerable = true;
        yield return StartCoroutine(CooldownTimer(i => { canDash = i; }, dashCooldown));
    }

    /// <summary>
    /// Utility method for managing cooldowns.
    /// </summary>
    /// <param name="toggleVar">The boolean to be toggled.</param>
    /// <param name="time">The time to wait before toggling the boolean back.</param>
    /// <returns></returns>
    private IEnumerator CooldownTimer(System.Action<bool> toggleVar, float time)
    {
        toggleVar(false);
        yield return new WaitForSeconds(time);
        toggleVar(true);
    }

    /// <summary>
    /// Handles an instance of damage to player on method call.
    /// </summary>
    /// <param name="damage">The amount of damage dealt to the player's health.</param>
    public void TakeDamage(int damage)
    {
        if (_curHealth > 0 && isVulnerable)
        {
            _curHealth -= damage;
            rb.velocity *= Vector2.up;
            animator.SetTrigger("TakeDamage");

            if (_curHealth <= 0)
            {
                _curHealth = 0;
                curState = State.Dead;
            }
        }
        healthBar.SetHealth(_curHealth);
    }

    /// <summary>
    /// Overloaded TakeDamage method to add a knockback force in addition to damage.
    /// </summary>
    /// <param name="damage">The amount of damage dealt to the player's health.</param>
    /// <param name="knockbackDir">The direction vector of the knockback force.</param>
    /// <param name="knockbackForce">The amount of knockback force applied.</param>
    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        TakeDamage(damage);
        if (_curHealth - damage > 0 && isVulnerable)
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Handles the Hurt state.
    /// </summary>
    private void HurtState()
    {
        animator.SetBool("IsMoving", false);
        animator.SetBool("IsFalling", false);
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsAttacking", false);
    }

    #region Hurt Animation Event Functions

    /// <summary>
    /// Should be called when the Hurt animation begins.
    /// </summary>
    private void StartHurtAnimation()
    {
        _isHurt = true;
        isVulnerable = false;
        curState = State.Hurt;
    }

    /// <summary>
    /// Should be called when the Hurt animation ends.
    /// </summary>
    private void FinishHurtAnimation()
    {
        isVulnerable = true;
        _isHurt = false;
        curState = State.Idle;
    }

    #endregion

    /// <summary>
    /// Handles the death of the player.
    /// </summary>
    private void Death()
    {
        isVulnerable = false;
        if (!hasPlayedDeathAnim)
        {
            StartCoroutine(DeathAnimation());
            hasPlayedDeathAnim = true;
        }
    }

    private IEnumerator DeathAnimation()
    {
        // Trigger death animation
        animator.SetTrigger("Death");
        // Wait until death animation finishes
        yield return new WaitUntil(() => _isDead);
        // Destroy game object
        Destroy(gameObject);
    }

    #region Death Animation Events

    /// <summary>
    /// This should be called when the Death animation ends.
    /// </summary>
    private void FinishDeathAnimation()
    {
        _isDead = true;
    }

    #endregion

    //private void OnDrawGizmosSelected()
    //{
    //    if (hitboxPoint == null)
    //        return;

    //    Gizmos.DrawCube(hitboxPoint.position, new Vector2(1f, .9f));
    //}
}
