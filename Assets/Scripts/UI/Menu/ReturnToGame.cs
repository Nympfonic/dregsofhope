using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToGame : MonoBehaviour
{
    public void ExitMenu()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().ResumeGame();
    }
}
