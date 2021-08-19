using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AlanZucconi.AI.BT;

public class AIDirector : MonoBehaviour
{
    private static AIDirector _instance;
    public static AIDirector Instance { get { return _instance; } }

    private BehaviourTree tree;

    public delegate void EnterArea();
    public static event EnterArea OnEnterArea;

    EventZone[] eventZones;

    public AnimationCurve eventUtility;
    [SerializeField] private float eventTimeThreshold = 5.0f;
    [SerializeField] private float eventTimeMaxThreshold = 25.0f;
    private float lastEventTime = 0;
    private bool eventUtilityRunning = false;
    private bool eventTimerActive = true;
    private bool waitingForEvent = false;
    public static bool HasEventBeenFired = false;
    private static int _challengeDesire = 0;
    public static int ChallengeDesire { get { return _challengeDesire; } }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        Node root = CreateBehaviourTree();
        tree = new BehaviourTree(root);
    }

    private void Update()
    {
        SceneManager.sceneLoaded += OnSceneLoad;

        if (eventTimerActive)
        {
            lastEventTime += Time.deltaTime;
        }

        tree.Update();
    }

    /// <summary>
    /// When a new scene loads, find the scene's event zones, and store in a list.
    /// </summary>
    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        eventZones = FindObjectsOfType<EventZone>();
    }

    /// <summary>
    /// Utility method to map a value from an old range to a new range of values.
    /// </summary>
    /// <param name="oVal">The value to be mapped.</param>
    /// <param name="oMin">The old range minimum value.</param>
    /// <param name="oMax">The old range maximum value.</param>
    /// <param name="nMin">The new range minimum value.</param>
    /// <param name="nMax">The new range maximum value.</param>
    /// <returns></returns>
    private float Map(float oVal, float oMin, float oMax, float nMin, float nMax)
    {
        float oRange = oMax - oMin;
        float nRange = nMax - nMin;
        float value = (((oVal - oMin) * nRange) / oRange) + nMin;

        return value;
    }

    /// <summary>
    /// Evaluates the event probability curve.
    /// </summary>
    private float EventUtility()
    {
        return eventUtility.Evaluate(Mathf.Clamp01(lastEventTime / eventTimeMaxThreshold));
    }

    /// <summary>
    /// Normalises the challenge value.
    /// </summary>
    private float ChallengeDesireNormalized()
    {
        float desireNormalized = Map(_challengeDesire, -50, 50, 1, 0);

        return desireNormalized;
    }

    /// <summary>
    /// Checks if event is ready to be triggered.
    /// </summary>
    private IEnumerator EventUtilityCheck()
    {
        eventUtilityRunning = true;
        if (lastEventTime >= eventTimeMaxThreshold)
        {
            Debug.Log("Event waiting to be triggered");
            // if player enters any event trigger zone,
            // subscribe to begin event
            waitingForEvent = true;
            //eventTimerActive = false;
            //lastEventTime = 0;
        }
        else if (EventUtility() >= ChallengeDesireNormalized()) {
            
        }
        yield return new WaitForSeconds(5.0f);
        eventUtilityRunning = false;
    }

    /// <summary>
    /// Creates the behaviour tree.
    /// </summary>
    private Node CreateBehaviourTree()
    {
        return new Selector
        (
            new Filter
            (
                // Check time passed since last event
                () => lastEventTime >= eventTimeThreshold
                    && !eventUtilityRunning
                    && !waitingForEvent
                    && !HasEventBeenFired,
                // if greater than the threshold
                // then choose an event to carry out using utility function
                new Action
                (
                    () => {
                        StartCoroutine(EventUtilityCheck());
                    }
                )
            ),
            new Filter
            (
                () => waitingForEvent && !HasEventBeenFired,
                new Selector
                (
                    new Filter
                    (
                        () => Random.Range(0, 100) < 70,
                        new Action(() => AmbushEvent())
                    ),
                    new Action(() => PursuerEvent())
                )
            )
        );
    }

    public void EventActive()
    {

    }

    private void AmbushEvent()
    {

    }

    private void PursuerEvent()
    {

    }
}
