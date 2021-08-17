using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlanZucconi.AI.BT;

public abstract class EnemyArchetype : MonoBehaviour
{
    protected PlayerController pc;
    protected SpriteRenderer sr;
    protected Rigidbody2D rb;
    protected BoxCollider2D hitbox;
    protected Animator animator;
    protected BehaviourTree tree;
    protected Transform target;

    protected bool isDead = false;
    protected bool hasPlayedDeathAnim = false;
    protected int _health = 100;
    public int Health
    {
        get { return _health; }
        set { _health = Health; }
    }
    [SerializeField] protected bool facingRight;
    [SerializeField] protected float sightDist = 4.0f;
    protected int playerMask;

    protected bool _isHurt = false;
    public bool IsHurt { get { return _isHurt; } }

    protected virtual void Start()
    {
        pc = FindObjectOfType<PlayerController>();
        target = pc.transform;
        playerMask = LayerMask.GetMask("Player");
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        Node root = CreateBehaviourTree();
        tree = new BehaviourTree(root);
    }

    protected virtual Node CreateBehaviourTree()
    {
        return Action.Nothing;
    }

    /// <summary>
    /// Returns the current direction that the entity is facing as a Vector2.
    /// </summary>
    public Vector2 CurrentDirection()
    {
        if (facingRight)
            return Vector2.right;
        else
            return Vector2.left;
    }

    /// <summary>
    /// Handles flipping the entity's sprite.
    /// </summary>
    protected void FlipSprite()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    /// <summary>
    /// Handles the damage the entity takes. This method should be called by other entities' scripts if they want to inflict damage on this entity.
    /// </summary>
    /// <param name="damage">The amount of damage dealt to its health.</param>
    public void TakeDamage(int damage)
    {
        if (_health > 0 && _health - damage > 0 && !_isHurt)
        {
            _health -= damage;
            animator.SetTrigger("TakeDamage");
        }
        else if (_health > 0 && _health - damage <= 0 && !_isHurt)
        {
            _health = 0;
            Death();
        }
    }

    /// <summary>
    /// Overloaded TakeDamage method to add a knockback force in addition to damage.
    /// </summary>
    /// <param name="damage">The amount of damage dealt to its health.</param>
    /// <param name="knockbackDir">The direction vector of the knockback force.</param>
    /// <param name="knockbackForce">The amount of knockback force applied.</param>
    public void TakeDamage(int damage, Vector2 knockbackDir, float knockbackForce)
    {
        TakeDamage(damage);
        if (_health > 0 && _health - damage > 0 && !_isHurt)
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }

    #region Hurt Animation Events

    /// <summary>
    /// This should be called when the Hurt animation begins.
    /// </summary>
    protected virtual void StartHurtAnimation()
    {
        _isHurt = true;
    }

    /// <summary>
    /// This should be called when the Hurt animation ends.
    /// </summary>
    protected virtual void FinishHurtAnimation()
    {
        _isHurt = false;
    }

    #endregion

    /// <summary>
    /// Handles the death of the entity.
    /// </summary>
    protected void Death()
    {
        if (!hasPlayedDeathAnim)
        {
            StartCoroutine(DeathAnimation());
            hasPlayedDeathAnim = true;
        }
    }

    /// <summary>
    /// Plays the death animation and waits for it to finish before removing the entity from the scene.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeathAnimation()
    {
        // Trigger death animation
        animator.SetTrigger("Death");
        // Wait until death animation finishes
        yield return new UnityEngine.WaitUntil(() => isDead);
        // Destroy game object
        Destroy(gameObject);
    }

    #region Death Animation Events

    /// <summary>
    /// This should be called when the Death animation ends.
    /// </summary>
    private void FinishDeathAnimation()
    {
        isDead = true;
    }

    #endregion
}
