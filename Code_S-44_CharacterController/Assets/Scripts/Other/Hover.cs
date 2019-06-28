using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make the object hover.
/// </summary>
public class Hover : MonoBehaviour
{

    // User Inputs
    public bool hoverOnX;
    public bool hoverOnZ;
    //public float degreesPerSecond = 15.0f;
    public float amplitude = 0.15f;
    public float frequency = 0.25f;

    // Position Storage Variables
    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    // Use this for initialization
    void Start()
    {
        // Store the starting position & rotation of the object
        posOffset = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Spin object around Y-Axis
        //transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);

        // Float up/down with a Sin()
        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

        if (hoverOnX)
        {
            tempPos.x += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        }

        if (hoverOnZ)
        {
            tempPos.z += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        }

        transform.position = tempPos;
    }
}
