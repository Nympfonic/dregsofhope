using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Interactable : MonoBehaviour
{
    private GameObject _interactionHint;

    private void Start()
    {
        _interactionHint = transform.GetChild(0).gameObject;
    }

    public enum InteractionType
    {
        NONE,
        Interact,
        PickUp,
        Examine
    }
    public InteractionType interactionType;

    public void Interact()
    {
        switch (interactionType)
        {
            case InteractionType.Interact:
                Interaction();
                break;
            case InteractionType.PickUp:
                PickUp();
                break;
            case InteractionType.Examine:
                Examination();
                break;
            case InteractionType.NONE:
                Debug.LogWarning("No interaction type given");
                break;
            default:
                Debug.LogError("Not a valid interactable");
                break;
        }
    }

    public void ShowInteractionHint()
    {
        _interactionHint.SetActive(true);
    }

    public void HideInteractionHint()
    {
        _interactionHint.SetActive(false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HideInteractionHint();
        }
    }

    protected virtual void Interaction()
    {
        FindObjectOfType<InteractionSystem>().Interaction();
        Debug.Log("Interacted with object");
    }

    private void PickUp()
    {
        FindObjectOfType<InteractionSystem>().PickUp();
        Destroy(gameObject);
        Debug.Log("Picked up object");
    }

    private void Examination()
    {
        FindObjectOfType<InteractionSystem>().Examination();
        Debug.Log("Examined object");
    }
}
