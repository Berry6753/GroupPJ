using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol,
        Run,
        Assassinated,
        Hurt,
        Die
    }

    public State state = State.Idle;


    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
