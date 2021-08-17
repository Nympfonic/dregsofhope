using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitfallDeath : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().TakeDamage(9999);
        }
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyArchetype>().TakeDamage(9999);
        }
    }
}
