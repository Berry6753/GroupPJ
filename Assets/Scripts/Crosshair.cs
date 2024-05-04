using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private float gunAccuracy;

    [SerializeField] private GameObject crosshair;

    public void FireAnimation()
    {
        anim.SetTrigger("fire");
    }

    public float GetAccuracy()
    {
        gunAccuracy = 0.02f;
        return gunAccuracy;
    }
}
