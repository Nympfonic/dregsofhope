using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlanZucconi.AI.BT;

public class EnemySkeleton : EnemyArchetype
{
    private Transform hitboxPoint;
    private Transform pointA, pointB, fallbackPoint;
    int obstacleMask;

    [Header("Movement")]
    [SerializeField] private bool willPatrol = false;
    [SerializeField][Range(0.1f, 5.0f)] private float speed = 1.0f;

    private int attackDamage = 24;
    private bool canAttack = true;
    private bool _isAttacking = false;
    public bool IsAttacking { get { return _isAttacking; } }
    private int numOfAttacks;
    private bool hasHit = false;
    private float attackCooldown = 1.5f;
    private bool playerAlreadySpotted = false;
    [Header("Attacking")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float knockbackForce = 4f;

    [Header("Stunned")]
    [SerializeField][Range(.3f, 3.0f)] private float stunDuration = 1.0f;
    private bool begunStun = false;
    private bool _isStunned = false;
    public bool IsStunned { get { return _isStunned; } }

    protected override void Start()
    {
        base.Start();

        fallbackPoint = transform;
        obstacleMask = LayerMask.GetMask("Player", "Ground");

        // If a patrolling AI, prepare patrol points
        if (willPatrol)
        {
            pointA = transform.parent.Find("Point A");
            pointB = transform.parent.Find("Point B");

            if (!pointA || !pointB)
            {
                willPatrol = false;
                Debug.Log("Cannot patrol without 2 points");
            }
        }
    }

    private void Update()
    {
        if (TargetHit() && !hasHit)
        {
            if (!pc.isHurt && !pc.isDead)
            {
                pc.TakeDamage(attackDamage, (target.transform.position - transform.position).normalized, knockbackForce);
                StartCoroutine(AttackDamageCooldown());
            }
        }

        tree.Update();
    }

    /// <summary>
    /// Creates the behaviour tree that controls the AI.
    /// </summary>
    protected override Node CreateBehaviourTree()
    {
        return new Selector
        (
            // If not dead
            new Filter
            (
                () => !isDead,
                new Selector
                (
                    // If not hurt
                    new Filter
                    (
                        () => !_isHurt,
                        new Selector
                        (
                            // If stunned
                            new Filter
                            (
                                () => _isStunned,
                                new Action(() => Stunned())
                            ),
                            // If player in detection range
                            new Filter
                            (
                                () => !pc.isDead && PlayerInRange(),
                                new Selector
                                (
                                    // If player in attack radius
                                    new Filter
                                    (
                                        () => PlayerInSight() && PlayerAlreadySpotted() && PlayerInAttackRange() && PlayerInAttackRadius() && !_isAttacking,
                                        // AI will attack player
                                        new Action(() => Attack())
                                    ),
                                    // If player in line of sight, but not attack radius
                                    new Filter
                                    (
                                        () => PlayerInSight() && PlayerAlreadySpotted() && !PlayerInAttackRange() && !PlayerInAttackRadius() && !_isAttacking,
                                        // AI will follow player
                                        new Action(() => Follow())
                                    ),
                                    // If player not in line of sight, but already spotted
                                    new Filter
                                    (
                                        () => !PlayerInSight() && PlayerAlreadySpotted() && !_isAttacking,
                                        // AI will follow player
                                        new Action(() => Follow())
                                    )
                                )
                            ),
                            // Else if player not in detection range
                            new Filter
                            (
                                () => !pc.isDead && !PlayerInRange(),
                                new Selector
                                (
                                    // If a patrolling AI
                                    new Filter
                                    (
                                        () => willPatrol && !_isAttacking,
                                        // AI will patrol
                                        new Action(() => Patrol())
                                    ),
                                    // Else AI will idle
                                    new Action(() => Idle())
                                )

                            ),
                            new Action(() => Idle())
                        )
                    ),
                    // Else run hurt method
                    new Action(() => Hurt())
                )
            ),
            // Else run death method
            new Action(() => Death())
        );
    }

    /// <summary>
    /// Checks if the player is in detection range.
    /// </summary>
    private bool PlayerInRange()
    {
        Collider2D playerInRange = Physics2D.OverlapCircle(transform.position, sightDist, playerMask);
        return playerInRange;
    }

    /// <summary>
    /// Checks if the player is in line of sight.
    /// </summary>
    private bool PlayerInSight()
    {
        if (Vector2.Dot(CurrentDirection(), (target.position - transform.position).normalized) > 0)
        {
            RaycastHit2D playerInSight = Physics2D.Linecast(transform.position + new Vector3(0, .3f),
                target.position, obstacleMask);
            return playerInSight;
        }
        else
            return false;
    }

    /// <summary>
    /// Checks if the player is in attack range.
    /// </summary>
    /// <returns></returns>
    private bool PlayerInAttackRange()
    {
        if (Mathf.Abs(transform.position.x - target.position.x) > attackRange)
            return false;
        else
            return true;
    }

    /// <summary>
    /// Checks if the player is in attack radius.
    /// </summary>
    private bool PlayerInAttackRadius()
    {
        if ((target.position - transform.position).magnitude > attackRange)
            return false;
        else
            return true;
    }

    /// <summary>
    /// Checks if the player has already been spotted.
    /// It will return false if the player has left the enemy's detection range.
    /// </summary>
    private bool PlayerAlreadySpotted()
    {
        if (PlayerInSight() && !playerAlreadySpotted)
            playerAlreadySpotted = true;
        else if (!PlayerInRange() && playerAlreadySpotted)
            playerAlreadySpotted = false;
        return playerAlreadySpotted;
    }

    /// <summary>
    /// Handles the Idle state.
    /// </summary>
    private void Idle()
    {
        //Debug.Log(gameObject.name + ": Idle");
        animator.SetBool("IsMoving", false);
    }

    /// <summary>
    /// Handles the Patrol state.
    /// </summary>
    private void Patrol()
    {
        //Debug.Log(gameObject.name + ": Patrolling");
        animator.SetBool("IsMoving", true);

        if ((facingRight && transform.position.x < pointB.position.x)
            || transform.position.x > pointB.position.x)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(pointB.position.x, transform.position.y, transform.position.z),
                speed * Time.deltaTime);
        }
        else if ((!facingRight && transform.position.x > pointA.position.x)
            || transform.position.x < pointA.position.x)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(pointA.position.x, transform.position.y, transform.position.z),
                speed * Time.deltaTime);
        }

        if ((!facingRight && transform.position.x <= pointA.position.x)
            || (facingRight && transform.position.x >= pointB.position.x))
        {
            FlipSprite();
        }
    }

    /// <summary>
    /// Handles the Follow state.
    /// </summary>
    private void Follow()
    {
        //Debug.Log(gameObject.name + ": Following");
        animator.SetBool("IsMoving", true);

        if ((facingRight && transform.position.x > target.position.x)
            || (!facingRight && transform.position.x < target.position.x))
        {
            FlipSprite();
        }
        /*transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(target.position.x, transform.position.y, transform.position.z),
                speed * Time.deltaTime);*/
        //rb.AddForce(speed * Vector2.right * (target.position - transform.position).normalized);
        rb.velocity = new Vector2(speed * (target.position - transform.position).normalized.x, rb.velocity.y);
    }

    /// <summary>
    /// Handles the Attack state.
    /// </summary>
    private void Attack()
    {
        //Debug.Log(gameObject.name + ": Attacking");
        // 35% chance of doing two attacks in a row
        if (Random.Range(0, 100) < 35)
            numOfAttacks = 2;
        // 65% chance of doing one attack
        else
            numOfAttacks = 1;
        animator.SetInteger("NumOfAttacks", numOfAttacks);
        animator.SetTrigger("Attack");
    }

    /// <summary>
    /// Checks if the player is detected by hitbox raycast.
    /// </summary>
    private bool TargetHit()
    {
        Collider2D col = Physics2D.OverlapBox(transform.position + (Vector3)hitbox.offset, hitbox.size, 0, playerMask);
        return col;
    }

    // Needs to be merged with the Cooldown timer utility method
    private IEnumerator AttackDamageCooldown()
    {
        hasHit = true;
        yield return new UnityEngine.WaitUntil(() => !TargetHit() && !pc.isHurt);
        hasHit = false;
    }

    #region Attack Animation Event Functions

    /// <summary>
    /// This should be called when the Attack animation begins.
    /// </summary>
    private void StartAttackAnimation()
    {
        canAttack = false;
        _isAttacking = true;
    }

    /// <summary>
    /// This should be called when the Attack animation ends.
    /// </summary>
    private void FinishAttackAnimation()
    {
        if (numOfAttacks == 2)
        {
            canAttack = true;
            animator.SetTrigger("Attack");
        }
        else
        {
            StartCoroutine(CooldownTimer(i => { canAttack = i; _isAttacking = !i; }, attackCooldown));
        }
        numOfAttacks -= 1;
    }

    #endregion

    /// <summary>
    /// Handles the Hurt state.
    /// </summary>
    private void Hurt()
    {
        //Debug.Log(gameObject.name + ": Hurt");
        animator.SetBool("IsMoving", false);
        _isAttacking = false;
    }

    /// <summary>
    /// Handles the Stunned state.
    /// </summary>
    private void Stunned()
    {
        //Debug.Log(gameObject.name + ": Stunned");
        animator.SetBool("IsMoving", false);
        _isAttacking = false;

        if (!begunStun)
        {
            StartCoroutine(CooldownTimer(i => {
                _isStunned = !i;
                animator.SetBool("IsStunned", _isStunned);
                if (!_isStunned) { begunStun = false; }
            }, stunDuration));

            begunStun = true;
        }

        // FUTURE IDEA: If player presses execution key, play execution death animation
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
}
