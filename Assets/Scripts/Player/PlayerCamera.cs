using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PlayerCamera : MonoBehaviour
{
    private BoxCollider2D mapBounds;
    private Transform player;
    private Camera cam;

    private float camOrthsize;
    private float camRatio;
    private float xMin, xMax, yMin, yMax;
    private float camX, camY;

    private void Start()
    {
        mapBounds = GameObject.Find("MapBounds").GetComponent<BoxCollider2D>();
        player = GameObject.Find("Player").transform;

        xMin = mapBounds.bounds.min.x;
        xMax = mapBounds.bounds.max.x;
        yMin = mapBounds.bounds.min.y;
        yMax = mapBounds.bounds.max.y;

        cam = GetComponent<Camera>();
        camOrthsize = cam.orthographicSize;
        camRatio = cam.aspect * camOrthsize;
    }

    private void LateUpdate()
    {
        if (player)
        {
            camY = Mathf.Clamp(player.position.y, yMin + camOrthsize, yMax - camOrthsize);
            camX = Mathf.Clamp(player.position.x, xMin + camRatio, xMax - camRatio);

            transform.position = new Vector3(camX, camY, transform.position.z);
        }
    }
}
