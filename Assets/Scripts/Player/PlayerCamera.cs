using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    private CinemachineFramingTransposer vcamFT;
    private float minY = .3f;
    private float maxY = .52f;

    private void Start()
    {
        vcamFT = FindObjectOfType<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    private void LateUpdate()
    {
        if (DownInput())
        {
            vcamFT.m_ScreenY = Mathf.Clamp(vcamFT.m_ScreenY - .005f, minY, maxY);
        }
        else
        {
            vcamFT.m_ScreenY = Mathf.Clamp(vcamFT.m_ScreenY + .005f, minY, maxY);
        }
    }

    private bool DownInput()
    {
        return Input.GetButton("Down");
    }
}
