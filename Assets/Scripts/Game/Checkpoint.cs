using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerSpawnLocation))]
public class Checkpoint : Interactable
{
    public Text gameSavedText;

    private bool _canInteract = true;

    protected override void Interaction()
    {
        if (_canInteract)
        {
            // Despawn active enemies and respawn them in original positions
            ResetEnemies();
            // Save game progress
            GameManager.SaveGame();

            Debug.Log("Interacted with checkpoint");

            StartCoroutine(InteractionCooldown());
            StartCoroutine(GameSavedPopup());
        }
    }

    private void ResetEnemies()
    {

    }

    private IEnumerator GameSavedPopup()
    {
        gameSavedText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        gameSavedText.gameObject.SetActive(false);
    }

    private IEnumerator InteractionCooldown()
    {
        _canInteract = false;
        yield return new WaitForSeconds(1f);
        _canInteract = true;
    }
}
