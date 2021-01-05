using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMeleeArchetype : EnemyController
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

    private float attackRange = 1.4f;

    [SerializeField][Range(.3f, 3.0f)]
    private float stunDuration = 1.0f;
    private float stunTimer = 0;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();

        sightDist = 4.0f;
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
                break;
        }
    }

    private void Detection()
    {
        Collider2D playerInRange = Physics2D.OverlapCircle(transform.position, sightDist, playerMask);

        if (playerInRange
            && Vector3.Dot(CurrentDirection(), target.position) > 0)
        {
            int obstacleMask = LayerMask.GetMask("Player", "Ground");
            RaycastHit2D playerInSight = Physics2D.Linecast(transform.position + new Vector3(0, .3f),
                target.position, obstacleMask);

            if (playerInSight.collider.CompareTag("Player")
                && curState != State.Following
                && Mathf.Abs(transform.position.x - target.position.x) > attackRange)
            {
                Debug.Log("State: Following");
                prevState = curState;
                curState = State.Following;
            }
            else if (playerInSight.collider.CompareTag("Player")
                && curState != State.Attacking
                && Mathf.Abs(transform.position.x - target.position.x) <= attackRange)
            {
                Debug.Log("State: Attacking");
                prevState = curState;
                curState = State.Attacking;
            }
            else if (!playerInSight
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
            Debug.Log("State: Patrolling");
            prevState = curState;
            curState = State.Patrolling;
        }
    }

    private void Idle()
    {
        Detection();
    }

    private void Follow()
    {
        Detection();

        transform.position = Vector3.MoveTowards(transform.position,
            new Vector3(target.position.x, transform.position.y, transform.position.z),
            speed * Time.deltaTime);
    }

    private void Attack()
    {
        // Animation trigger for attacking

        // Return to Idle state after animation finishes
    }

    private void Patrol()
    {
        Detection();

        if (facingRight && transform.position.x < pointB.position.x)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(pointB.position.x, transform.position.y, transform.position.z),
                speed * Time.deltaTime);
        }
        else if (!facingRight && transform.position.x > pointA.position.x)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(pointA.position.x, transform.position.y, transform.position.z),
                speed * Time.deltaTime);
        }
        else if (transform.position.x > pointB.position.x)
        {
            if (facingRight)
            {
                FlipSprite();
            }
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(pointB.position.x, transform.position.y, transform.position.z),
                speed * Time.deltaTime);
        }
        else if (transform.position.x < pointA.position.x)
        {
            if (!facingRight)
            {
                FlipSprite();
            }
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(pointA.position.x, transform.position.y, transform.position.z),
                speed * Time.deltaTime);
        }

        if ((!facingRight && transform.position.x == pointA.position.x)
            || (facingRight && transform.position.x == pointB.position.x))
        {
            FlipSprite();
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
