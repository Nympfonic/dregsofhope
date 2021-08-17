using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get { return _instance; }
    }

    public GameObject pauseMenu;
    private PlayerController pc;
    private PlayerSpawnLocation[] psl;

    public static bool gamePaused = false;

    /* Save data variables */
    // scene where last interacted checkpoint was located
    private static int _savedScene;
    public static int SavedScene
    {
        get { return _savedScene; }
        set { _savedScene = SavedScene; }
    }

    // last interacted checkpoint
    private static PlayerSpawnLocation _savedSpawnLocation;
    public static PlayerSpawnLocation SavedSpawnLocation
    {
        get { return _savedSpawnLocation; } 
        set { _savedSpawnLocation = SavedSpawnLocation; }
    }

    // last safe death position
    private static Vector3 _savedDeathLocation;
    public static Vector3 SavedDeathLocation
    {
        get { return _savedDeathLocation; }
        set { _savedDeathLocation = SavedDeathLocation; }
    }

    // check if player has reached first checkpoint
    private static bool _hasReachedFirstCheckpoint = false;
    public static bool HasReachedFirstCheckpoint
    {
        get { return _hasReachedFirstCheckpoint; }
        set { _hasReachedFirstCheckpoint = HasReachedFirstCheckpoint; }
    }

    // previous scene
    private static int _previousScene;
    public static int PreviousScene
    {
        get { return _previousScene; }
        set { _previousScene = PreviousScene; }
    }

    // current player health
    private static int _curPlayerHealth;
    public static int CurrentPlayerHealth
    {
        get { return _curPlayerHealth; }
        set { _curPlayerHealth = CurrentPlayerHealth; }
    }

    // maximum player health
    private static int _maxPlayerHealth;
    public static int MaxPlayerHealth
    {
        get { return _maxPlayerHealth; }
        set { _maxPlayerHealth = MaxPlayerHealth; }
    }

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        pc = FindObjectOfType<PlayerController>();
        psl = FindObjectsOfType<PlayerSpawnLocation>();

        SceneManager.sceneLoaded += OnSceneLoad;

        // Set player spawn position
        /* If player hasn't reached first checkpoint,
         * set player to spawn at the start */
        if (!_hasReachedFirstCheckpoint)
        {
            foreach (PlayerSpawnLocation spawnPoint in psl)
            {
                if (spawnPoint.SpawnType == PlayerSpawnLocation._SpawnType.Start)
                {
                    pc.transform.position = (Vector2)spawnPoint.transform.position;
                    break;
                }
            }
        }
        /* Else if player has reached first checkpoint already,
         * set player to spawn at the last interacted checkpoint */
        else if (_hasReachedFirstCheckpoint)
        {
            pc.transform.position = (Vector2)_savedSpawnLocation.transform.position;
        }
    }

    private void Update()
    {
        if (PauseGameInput())
        {
            if (gamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private bool PauseGameInput()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        //psl = FindObjectsOfType<PlayerSpawnLocation>();
        //foreach (PlayerSpawnLocation spawnPoint in psl)
        //{

        //}

        SceneManager.UnloadSceneAsync(_previousScene);

        AIDirector.HasEventBeenFired = false; // Reset on every scene load
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
        gamePaused = false;
    }

    private void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        gamePaused = true;
    }

    public static void SaveGame()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath
            + "/DregsOfHope_SaveData.dat");
        SaveData data = new SaveData
        {
            savedScene = _savedScene,
            savedSpawnLocation = _savedSpawnLocation,
            savedDeathLocation = _savedDeathLocation,
            hasReachedFirstCheckpoint = _hasReachedFirstCheckpoint
        };
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Game data saved");
    }

    public static void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/DregsOfHope_SaveData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath
                + "/DregsOfHope_SaveData.dat", FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();

            _savedScene = data.savedScene;
            _savedSpawnLocation = data.savedSpawnLocation;
            _savedDeathLocation = data.savedDeathLocation;
            _hasReachedFirstCheckpoint = data.hasReachedFirstCheckpoint;
            Debug.Log("Game data loaded");
        }
        else
        {
            Debug.LogError("Unable to find save data");
        }
    }

    public static void ResetData()
    {
        if (File.Exists(Application.persistentDataPath + "/DregsOfHope_SaveData.dat"))
        {
            File.Delete(Application.persistentDataPath + "/DregsOfHope_SaveData.dat");

            // Reset to fresh state
            _savedScene = 1;
            _savedSpawnLocation = null;
            _savedDeathLocation = Vector3.zero;
            _hasReachedFirstCheckpoint = false;
            Debug.Log("Save data reset complete");
        }
        else
        {
            Debug.LogError("No save data to delete");
        }
    }
}

[Serializable]
class SaveData
{
    public int savedScene;
    public PlayerSpawnLocation savedSpawnLocation;
    public Vector3 savedDeathLocation;
    public bool hasReachedFirstCheckpoint;
}
