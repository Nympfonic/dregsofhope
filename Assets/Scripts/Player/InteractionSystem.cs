using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    private const float detectionRadius = .7f;
    private LayerMask detectionLayer = 11;
    // Cache detected object
    private GameObject detectedInteractable;

    private void Update()
    {
        if (DetectInteractable())
        {
            if (InteractInput())
            {
                detectedInteractable.GetComponent<Interactable>().Interact();
            }
        }
    }

    private bool InteractInput()
    {
        return Input.GetButtonDown("Interact");
    }

    private bool DetectInteractable()
    {
        Collider2D obj = Physics2D.OverlapCircle(transform.position, detectionRadius, detectionLayer);
        if (obj == null)
        {
            detectedInteractable = null;
            return false;
        }
        else
        {
            detectedInteractable = obj.gameObject;
            return true;
        }
    }

    public void Interaction()
    {

    }

    public void PickUp()
    {

    }

    public void Examination()
    {

    }
}
