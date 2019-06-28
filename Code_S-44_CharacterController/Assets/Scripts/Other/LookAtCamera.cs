using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make the object face the camera.
/// </summary>
public class LookAtCamera : MonoBehaviour
{
    private Camera m_Camera;

    private void Awake()
    {
        m_Camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward, m_Camera.transform.rotation * Vector3.up);
    }
}
