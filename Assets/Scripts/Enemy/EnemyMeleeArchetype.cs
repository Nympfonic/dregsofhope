using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMeleeArchetype : EnemyController
{
    private enum EnemyState
    {
        Idle,
        Patrolling,
        Moving,
        Attacking,
        Stunned
    }
    private EnemyState enemyState;
    private Transform pointA, pointB;

    private bool willPatrol = false;

    protected override void Start()
    {
        base.Start();

        health = 300;
        enemyState = EnemyState.Idle;
        sightRadius = 6.0f;

        if (willPatrol)
        {
            pointA = transform.Find("Point A");
            pointB = transform.Find("Point B");
        }
    }

    private void Update()
    {
        switch (enemyState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Moving:
                Movement();
                break;
            case EnemyState.Attacking:
                Attack();
                break;
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Stunned:
                Stunned();
                break;
        }
    }

    

    private void Idle()
    {
        Physics2D.CircleCast(transform.position, sightRadius, CurrentDirection(), );
    }

    private void Movement()
    {

    }

    private void Attack()
    {

    }

    private void Patrol()
    {

    }

    private void Stunned()
    {

    }
}
