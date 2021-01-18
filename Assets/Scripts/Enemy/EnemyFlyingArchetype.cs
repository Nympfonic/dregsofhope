using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFlyingArchetype : EnemyController
{
    private Rigidbody2D rb;

    private enum State
    {
        Idle,
        Patrolling,
        Following,
        Attacking,
        Stunned,
        Dead
    }
    private State curState, prevState;
    private Transform pointA, pointB;

    [SerializeField]
    private bool willPatrol = false;
    [SerializeField][Range(2.0f, 6.0f)]
    private float speed = 4.0f;

    [SerializeField]
    private int attackDamage = 15;
    private float attackRange = 3.0f;
    private bool canAttack = true;
    private bool hasHit = false;
    [SerializeField]
    private float attackCooldown = 3.0f;
    private Vector3 lastTargetLocation = Vector3.positiveInfinity;

    [SerializeField][Range(.3f, 3.0f)]
    private float stunDuration = 1.0f;
    private float stunTimer = 0;

    protected override void Start()
    {
        base.Start();

        sightDist = 6.0f;
        curState = State.Idle;

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
    }

    private void Update()
    {
        if (isDead)
        {
            curState = State.Dead;
        }
        else
        {
            Collider2D hitTarget = Physics2D.OverlapBox(transform.position, transform.localScale, 0, playerMask);
            if (hitTarget)
            {
                PlayerController pc = target.GetComponent<PlayerController>();
                if (pc)
                {
                    if (!pc.isHurt && !hasHit)
                    {
                        pc.TakeDamage(attackDamage);
                        StartCoroutine(DamageCooldown());
                    }
                }
            }
        }

        switch (curState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Following:
                Follow();
                break;
            case State.Attacking:
                Attack();
                break;
            case State.Patrolling:
                Patrol();
                break;
            case State.Stunned:
                Stunned();
                break;
            case State.Dead:
                Destroy(gameObject);
                break;
        }
    }

    private bool TargetHit()
    {
        Collider2D col = Physics2D.OverlapBox(transform.position, transform.localScale, 0, playerMask);

        return col;
    }

    private void Detection()
    {
        Collider2D playerInRange = Physics2D.OverlapCircle(transform.position, sightDist, playerMask);

        if (playerInRange
            && Vector2.Dot(CurrentDirection(), (target.position - transform.position).normalized) > 0)
        {
            
            if (playerInRange.gameObject.CompareTag("Player")
                && curState != State.Following
                && (target.position - transform.position).sqrMagnitude > Mathf.Pow(attackRange, 2))
            {
                //Debug.Log("State: Following");
                prevState = curState;
                curState = State.Following;
            }
            else if (playerInRange.gameObject.CompareTag("Player")
                && curState != State.Attacking
                && canAttack
                && Vector2.SqrMagnitude(target.position - transform.position) <= Mathf.Pow(attackRange, 2))
            {
                //Debug.Log("State: Attacking");
                prevState = curState;
                lastTargetLocation = playerInRange.transform.position;
                curState = State.Attacking;
            }
            else if (!playerInRange
                && curState == State.Following)
            {
                curState = State.Idle;
            }
        }
        else if (!playerInRange
            && curState == State.Following)
        {
            curState = State.Idle;
        }
        else if (willPatrol
            && curState == State.Idle)
        {
            //Debug.Log("State: Patrolling");
            prevState = curState;
            curState = State.Patrolling;
        }
    }

    private void Idle()
    {
        if (!isHurt || curState != State.Stunned)
        {
            Detection();
        }
    }

    private void Follow()
    {
        if (!isHurt || curState != State.Stunned)
        {
            Detection();

            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(target.position.x, target.position.y, transform.position.z),
                speed * Time.deltaTime);
        }
    }

    private void Attack()
    {
        // Move in the direction towards the player
        Vector2 dir = (lastTargetLocation - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position,
            new Vector3(lastTargetLocation.x + dir.x * 3.0f,
                lastTargetLocation.y + dir.y * 3.0f,
                transform.position.z),
            speed * Time.deltaTime);
        // Return to previous state after animation finishes
        if ((new Vector3(lastTargetLocation.x + dir.x * 3.0f,
            lastTargetLocation.y + dir.y * 3.0f,
            transform.position.z) - transform.position).sqrMagnitude <= .0025f)
        {
            lastTargetLocation = Vector3.positiveInfinity;
            StartCoroutine(CooldownTimer(i => { canAttack = i; }, attackCooldown));
            curState = prevState;
        }
    }

    private IEnumerator DamageCooldown()
    {
        hasHit = true;
        yield return new WaitWhile(TargetHit);
        hasHit = false;
    }

    private IEnumerator CooldownTimer(System.Action<bool> toggleVar, float time)
    {
        toggleVar(false);
        yield return new WaitForSeconds(time);
        toggleVar(true);
    }

    private void Patrol()
    {
        if (!isHurt || curState != State.Stunned)
        {
            Detection();

            if ((facingRight && transform.position.x < pointB.position.x)
                || transform.position.x > pointB.position.x)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                    new Vector3(pointB.position.x, pointB.position.y, transform.position.z),
                    speed * Time.deltaTime);
            }
            else if ((!facingRight && transform.position.x > pointA.position.x)
                || transform.position.x < pointA.position.x)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                    new Vector3(pointA.position.x, pointA.position.y, transform.position.z),
                    speed * Time.deltaTime);
            }

            if ((!facingRight && transform.position.x <= pointA.position.x)
                || (facingRight && transform.position.x >= pointB.position.x))
            {
                FlipSprite();
            }
        }
    }

    private void Stunned()
    {
        stunTimer += Time.deltaTime;

        if (stunTimer > stunDuration)
        {
            stunTimer = 0;
            curState = State.Idle;
        }
    }
}
