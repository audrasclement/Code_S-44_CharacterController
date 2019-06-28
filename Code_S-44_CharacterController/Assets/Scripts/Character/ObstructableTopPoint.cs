using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstructableTopPoint : MonoBehaviour
{

    public bool _isObstructed;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            _isObstructed = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            _isObstructed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            _isObstructed = false;
        }
    }
}
