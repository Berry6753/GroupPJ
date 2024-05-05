using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckAssing : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            PlayerController.Instance.isAssasing = true;
            PlayerController.Instance.target = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            PlayerController.Instance.isAssasing = false;
            PlayerController.Instance.target = null;
        }
    }
}
