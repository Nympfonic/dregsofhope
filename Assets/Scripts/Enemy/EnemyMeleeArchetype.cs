using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlanZucconi.AI.BT;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
public class EnemyMeleeArchetype : EnemyArchetype
{
    private Transform hitboxPoint;

    private enum State
    {
        Idle,
        Patrolling,
        Following,
        Attacking,
        Hurt,
        Stunned,
        Dead
    }
    private State curState, prevState;
    private Transform pointA, pointB;

    [SerializeField] private bool willPatrol = false;
    [SerializeField][Range(0.1f, 5.0f)] private float speed = 1.0f;

    private int attackDamage = 24;
    private bool canAttack = true;
    private bool hasHit = false;
    private float attackCooldown = 1.5f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float knockbackForce = 4f;

    [SerializeField][Range(.3f, 3.0f)] private float stunDuration = 1.0f;
    private float stunTimer = 0;

    protected override void Start()
    {
        base.Start();

        
        //hitboxPoint = transform.GetChild(0);

        sightDist = 4.0f;
        curState = State.Idle;

        // If a patrolling enemy, prepare patrol points
        if (willPatrol)
        {
            pointA = transform.parent.Find("PointA");
            pointB = transform.parent.Find("PointB");

            if (!pointA || !pointB)
            {
                willPatrol = false;
                Debug.Log("Cannot patrol without 2 points");
            }
        }
        // Else if a stationary enemy, save current position as fallback
        else
        {

        }
    }

    protected override Node CreateBehaviourTree()
    {
        return Action.Nothing;
    }

    protected virtual void Update()
    {
        if (isDead)
        {
            curState = State.Dead;
        }
        else
        {
            switch (curState)
            {
                case State.Idle:
                    IdleState();
                    break;
                case State.Following:
                    FollowState();
                    break;
                case State.Attacking:
                    AttackState();
                    break;
                case State.Patrolling:
                    PatrolState();
                    break;
                case State.Hurt:
                    HurtState();
                    break;
                case State.Stunned:
                    StunnedState();
                    break;
                case State.Dead:
                    Death();
                    break;
            }
        }
    }

    /// <summary>
    /// Handles the detection of the player and changing states.
    /// </summary>
    private void Detection()
    {
        Collider2D playerInRange = Physics2D.OverlapCircle(transform.position, sightDist, playerMask);

        // If player in range, and in the direction the entity is facing
        if (playerInRange
            && !pc.isDead
            && Vector2.Dot(CurrentDirection(), (target.position - transform.position).normalized) > 0)
        {
            int obstacleMask = LayerMask.GetMask("Player", "Ground");
            RaycastHit2D playerInSight = Physics2D.Linecast(transform.position + new Vector3(0, .3f),
                target.position, obstacleMask);

            // If player in sight, but outside attack range, move towards the player
            if (playerInSight.collider.CompareTag("Player")
                && curState != State.Following
                && Mathf.Abs(transform.position.x - target.position.x) > attackRange)
            {
                Debug.Log(gameObject.name + " State: Out of attack range; Following");
                animator.SetBool("IsMoving", true);
                curState = State.Following;
            }
            // Else if the player in sight, but within attack range, attack the player
            else if (playerInSight.collider.CompareTag("Player")
                && curState != State.Attacking
                && Mathf.Abs(transform.position.x - target.position.x) <= attackRange)
            {
                Debug.Log(gameObject.name + " State: In attack range; Attacking");
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("Attack");
                curState = State.Attacking;
            }
            // Else if player not in sight, return to idle state
            else if (!playerInSight
                && curState == State.Following)
            {
                Debug.Log(gameObject.name + " State: No player in LoS; Idling");
                animator.SetBool("IsMoving", false);
                curState = State.Idle;
            }
        }
        // Else if player not in range, return to idle state
        else if (!playerInRange
            && curState != State.Idle)
        {
            Debug.Log(gameObject.name + " State: No player in range; Idling");
            animator.SetBool("IsMoving", false);
            curState = State.Idle;
        }
        // Else if entity will patrol, and currently in idle state, return to patrolling state
        else if (willPatrol
            && curState == State.Idle)
        {
            Debug.Log(gameObject.name + " State: No player in range; Patrolling");
            animator.SetBool("IsMoving", true);
            curState = State.Patrolling;
        }
    }

    /// <summary>
    /// Handles the Idle state.
    /// </summary>
    private void IdleState()
    {
        if (!_isHurt || curState != State.Stunned)
        {
            Detection();
        }
    }

    /// <summary>
    /// Handles the Follow state.
    /// </summary>
    private void FollowState()
    {
        if (!_isHurt || curState != State.Stunned)
        {
            Detection();
            if ((facingRight && transform.position.x > target.position.x)
                || (!facingRight && transform.position.x < target.position.x))
            {
                FlipSprite();
            }
            /*transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(target.position.x, transform.position.y, transform.position.z),
                speed * Time.deltaTime);*/
            //rb.AddForce(speed * Vector2.right * (target.position - transform.position).normalized);
            rb.velocity = new Vector2(speed * (target.position - transform.position).normalized.x, 0);
        }
    }

    /// <summary>
    /// Handles the Attack state.
    /// </summary>
    private void AttackState()
    {
        //Collider2D hitTarget = Physics2D.OverlapBox(transform.position + (Vector3)hitbox.offset, hitbox.size, 0, playerMask);
        if (TargetHit() && !hasHit)
        {
            //hasHit = true;
            if (!pc.isHurt && !pc.isDead)
            {
                pc.TakeDamage(attackDamage, (target.transform.position - transform.position).normalized, knockbackForce);
                StartCoroutine(AttackDamageCooldown());
            }
            // Attack chain moves if previous hit connects
            //attackNum = (attackNum + 1) % 3;
            //animator.SetInteger("AttackNum", attackNum);
        }
    }

    private bool TargetHit()
    {
        Collider2D col = Physics2D.OverlapBox(transform.position + (Vector3)hitbox.offset, hitbox.size, 0, playerMask);
        return col;
    }

    // Needs to be merged with the Cooldown timer utility method
    private IEnumerator AttackDamageCooldown()
    {
        hasHit = true;
        yield return new UnityEngine.WaitUntil(() => !TargetHit() && !target.GetComponent<PlayerController>().isHurt);
        hasHit = false;
    }

    #region Attack Animation Event Functions

    /// <summary>
    /// This should be called when the Attack animation begins.
    /// </summary>
    private void StartAttackAnimation()
    {
        canAttack = false;
    }

    /// <summary>
    /// This should be called when the Attack animation ends.
    /// </summary>
    private void FinishAttackAnimation()
    {
        StartCoroutine(CooldownTimer(i => { canAttack = i; }, attackCooldown));
        curState = State.Idle;
    }

    #endregion

    /// <summary>
    /// Handles the Hurt state.
    /// </summary>
    private void HurtState()
    {
        animator.SetBool("IsMoving", false);
    }

    #region Hurt Animation Event Functions

    /// <summary>
    /// Should be called when the Hurt animation begins.
    /// </summary>
    protected override void StartHurtAnimation()
    {
        base.StartHurtAnimation();

        curState = State.Hurt;
    }

    /// <summary>
    /// Should be called when the Hurt animation ends.
    /// </summary>
    protected override void FinishHurtAnimation()
    {
        base.FinishHurtAnimation();

        curState = State.Idle;
    }

    #endregion

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
    /// Handles the Patrol state.
    /// </summary>
    private void PatrolState()
    {
        if (!_isHurt || curState != State.Stunned)
        {
            Detection();

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
    }

    /// <summary>
    /// Handles the Stunned state.
    /// </summary>
    private void StunnedState()
    {
        stunTimer += Time.deltaTime;

        if (stunTimer > stunDuration)
        {
            stunTimer = 0;
            curState = State.Idle;
        }
    }
}
