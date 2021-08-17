using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventZone : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If player enters event zone
        if (collision.CompareTag("Player"))
        {
            AIDirector.OnEnterArea += AIDirector.WaitingForEvent;
        }
    }
}
