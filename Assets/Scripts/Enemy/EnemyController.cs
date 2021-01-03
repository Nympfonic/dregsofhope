using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    protected bool isDead = false;
    public int health = 100;
    protected bool facingRight = true;

    protected Transform target;
    protected float sightRadius = 5.0f;

    protected virtual void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
    }

    public virtual Vector2 CurrentDirection()
    {
        Vector2 dir = Vector2.zero;
        if (facingRight)
            dir = Vector2.right;
        else
            dir = Vector2.left;
        return dir;
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

    protected void Death()
    {
        isDead = true;
    }
}
