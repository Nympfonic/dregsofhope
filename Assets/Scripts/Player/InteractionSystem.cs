using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    private const float detectionRadius = .7f;
    [SerializeField] private LayerMask detectionLayer;
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
        if (obj)
        {
            detectedInteractable = obj.gameObject;
            obj.GetComponent<Interactable>().ShowInteractionHint();
            return true;
        }
        else
        {
            detectedInteractable = null;
            return false;
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
