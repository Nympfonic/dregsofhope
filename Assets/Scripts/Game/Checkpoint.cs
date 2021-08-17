using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerSpawnLocation))]
public class Checkpoint : Interactable
{
    protected override void Interaction()
    {
        // Despawn active enemies and respawn them in original positions
        ResetEnemies();
        // Save game progress
        SaveGame();

        Debug.Log("Interacted with checkpoint");
    }

    private void ResetEnemies()
    {

    }

    private void SaveGame()
    {
        GameManager.SaveGame();
    }
}
