using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    protected bool isDead = false;
    public int health = 100;
    [SerializeField]
    protected bool facingRight = true;

    protected Transform target;
    [SerializeField]
    protected float sightDist = 4.0f;
    protected int playerMask;

    protected virtual void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        playerMask = LayerMask.GetMask("Player");
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

    protected void FlipSprite()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
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
