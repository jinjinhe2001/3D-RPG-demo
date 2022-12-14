using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FreeLook : MonoBehaviour
{
    private CinemachineFreeLook freeLook;

    public float camDepthSmooth = 1000000000000000f;

    private void Awake()
    {
        freeLook = GetComponent<CinemachineFreeLook>();

}

    private void Update()
    {
        OnClick();
        GetScrollWheel();
    }

    private void OnClick()
    {
        if(Input.GetMouseButton(1))
        {
            freeLook.m_XAxis.m_InputAxisName = "Mouse X";
            freeLook.m_YAxis.m_InputAxisName = "Mouse Y";
        }
        else
        {
            freeLook.m_XAxis.m_InputAxisValue = 0;
            freeLook.m_YAxis.m_InputAxisValue = 0;
            freeLook.m_XAxis.m_InputAxisName = null;
            freeLook.m_YAxis.m_InputAxisName = null;
        }
    }

    private void GetScrollWheel()
    {
        if ((Input.mouseScrollDelta.y < 0 && freeLook.m_Orbits[0].m_Radius < 16f) || (Input.mouseScrollDelta.y > 0 && freeLook.m_Orbits[0].m_Radius > 6f)) 
        {
            freeLook.m_Orbits[0].m_Radius -= Input.mouseScrollDelta.y * Time.deltaTime * camDepthSmooth * camDepthSmooth* camDepthSmooth;
            freeLook.m_Orbits[0].m_Height -= Input.mouseScrollDelta.y * Time.deltaTime * camDepthSmooth * camDepthSmooth* camDepthSmooth;
            freeLook.m_Orbits[1].m_Radius -= Input.mouseScrollDelta.y * Time.deltaTime * camDepthSmooth * camDepthSmooth* camDepthSmooth;
            freeLook.m_Orbits[1].m_Height -= Input.mouseScrollDelta.y * Time.deltaTime * camDepthSmooth * camDepthSmooth* camDepthSmooth;
            freeLook.m_Orbits[2].m_Radius -= Input.mouseScrollDelta.y * Time.deltaTime * camDepthSmooth * camDepthSmooth* camDepthSmooth;
            freeLook.m_Orbits[2].m_Height -= Input.mouseScrollDelta.y * Time.deltaTime * camDepthSmooth * camDepthSmooth* camDepthSmooth;
        }
    }
}
