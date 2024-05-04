using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGround : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("c");
        if (other.CompareTag("Ground"))
        {
            PlayerController.Instance.isGround = true;
            Debug.Log("a");
        }
    }

    //public void OnTriggerExit(Collider other)
    //{
    //    Debug.Log("d");
    //    if (other.CompareTag("Ground"))
    //    {
    //        PlayerController.Instance.isGround = false;
    //        Debug.Log("b");
    //    }
    //}
}
