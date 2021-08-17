using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnLocation : MonoBehaviour
{
    public enum _SpawnType
    {
        Start,
        Checkpoint,
        Spawn
    }
    public _SpawnType SpawnType;
}
