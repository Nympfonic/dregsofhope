using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlanZucconi.AI.BT;

public class AIDirector : Singleton
{
    private BehaviourTree tree;

    public delegate void OnEnterArea();
    public static event OnEnterArea onEnterArea;

    public AnimationCurve eventUtility;
    [SerializeField]
    private float eventTimeThreshold = 10.0f;
    [SerializeField]
    private float eventTimeMaxThreshold = 25.0f;
    private float lastEventTime = 0;
    private bool eventUtilityRunning = false;
    [HideInInspector]
    public int challengeDesire = 0;

    private void Start()
    {
        Node root = CreateBehaviourTree();
        tree = new BehaviourTree(root);
    }

    private void Update()
    {
        lastEventTime += Time.deltaTime;

        tree.Update();
    }

    private void SubscribeEvents()
    {

    }

    private float Map(float oVal, float oMin, float oMax, float nMin, float nMax)
    {
        float oRange = oMax - oMin;
        float nRange = nMax - nMin;
        float value = (((oVal - oMin) * nRange) / oRange) + nMin;

        return value;
    }

    private float EventUtility()
    {
        return eventUtility.Evaluate(Mathf.Clamp01(lastEventTime / eventTimeMaxThreshold));
    }

    private float ChallengeDesire()
    {
        float desireNormalized = Map(challengeDesire, -50, 50, 0, 1);

        return desireNormalized;
    }

    private IEnumerator EventUtilityCheck()
    {
        eventUtilityRunning = true;
        if (EventUtility() <= ChallengeDesire())
        {
            // Subscribe to event
        }
        yield return new WaitForSeconds(5.0f);
        eventUtilityRunning = false;
    }

    public Node CreateBehaviourTree()
    {
        return new Selector
        (
            new Filter
            (
                // Check time passed since last event
                () => lastEventTime >= eventTimeThreshold && !eventUtilityRunning,
                // if greater than 10 secs (prototype demonstration)
                // then choose an event to carry out using utility function
                new Action
                (
                    () => {
                        StartCoroutine(EventUtilityCheck());
                    }
                )
            )
        );
    }
}
