using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class View : MonoBehaviour
{
    [SerializeField] private float viewAngle;
    [SerializeField] private float viewDistance;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private Transform Eyes;

    public bool isFind {  get; private set; }

    private NavMeshAgent agent;
    
    private void Awake()
    {
        viewAngle = 120.0f;
        viewDistance = 10.0f;
        isFind = false;

        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Sight();
    }

    private Vector3 BoundarAngle(float _angle)
    {
        _angle += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(_angle * Mathf.Deg2Rad), 0, Mathf.Cos(_angle * Mathf.Deg2Rad));
    }

    private void Sight()
    {
        Vector3 leftBoundary = BoundarAngle(-viewAngle * 0.5f);
        Vector3 rightBoundary = BoundarAngle(viewAngle * 0.5f);

        Debug.DrawRay(transform.position, leftBoundary, Color.red);
        Debug.DrawRay(transform.position, rightBoundary, Color.red);

        Collider[] target = Physics.OverlapSphere(Eyes.position, viewDistance, targetMask);

        if(target.Length > 0 )
        {
            Transform targetTF = target[0].gameObject.transform;
            Vector3 direction = (targetTF.position - transform.position).normalized;
            float angle = Vector3.Angle(direction, transform.forward);

            if (angle <= viewAngle * 0.5f)
            {
                Debug.DrawRay(transform.position + Eyes.forward, direction, Color.blue);
                if (Physics.Raycast(Eyes.transform.position, direction, out RaycastHit hit, viewDistance))
                {
                    Debug.Log(hit.transform.name);
                    if (hit.transform.CompareTag("Player"))
                    {
                        isFind = true;
                        agent.destination = hit.transform.position;
                    }
                }
                else
                {
                    isFind = false;
                }
            }
        }
    }
}
