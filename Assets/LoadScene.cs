using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class LoadScene : MonoBehaviour
{
    [SerializeField] private int scene;
    [SerializeField] private PlayerSpawnLocation playerSpawnLocation;

    private Animator crossfade;
    [SerializeField] float crossfadeTime = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            GameManager.SavedSpawnLocation = playerSpawnLocation;
            GameManager.PreviousScene = SceneManager.GetActiveScene().buildIndex;
            GameManager.CurrentPlayerHealth = collision.GetComponent<PlayerController>().CurrentHealth;
            GameManager.MaxPlayerHealth = collision.GetComponent<PlayerController>().MaxHealth;
            StartCoroutine(SceneLoad(scene));
        }
    }

    private IEnumerator SceneLoad(int scene)
    {
        crossfade.SetTrigger("Crossfade");

        yield return new WaitForSeconds(crossfadeTime);

        SceneManager.LoadScene(scene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(scene));
    }
}
