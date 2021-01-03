using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public BoxCollider2D mapBounds;

    private Transform player;
    private Camera cam;
    private float camOrthsize;
    private float camRatio;
    private float xMin, xMax, yMin, yMax;
    private float camX, camY;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        xMin = mapBounds.bounds.min.x;
        xMax = mapBounds.bounds.max.x;
        yMin = mapBounds.bounds.min.y;
        yMax = mapBounds.bounds.max.y;

        cam = GetComponent<Camera>();
        camOrthsize = cam.orthographicSize;
        camRatio = (xMax + camOrthsize) / 2.0f;
    }

    private void FixedUpdate()
    {
        camY = Mathf.Clamp(player.position.y, yMin + camOrthsize, yMax - camOrthsize);
        camX = Mathf.Clamp(player.position.x, xMin + camRatio, xMax - camRatio);
        this.transform.position = new Vector3(camX, camY, this.transform.position.z);
    }
}
