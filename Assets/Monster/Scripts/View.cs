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

    public Transform target {  get; private set; }

    public bool isFind {  get; private set; }
    public bool isAttackAble { get; private set; }
    public bool RunSuccess { get; private set; }

    private NavMeshAgent agent;

    [SerializeField]
    private float AttackDistance;
    
    private void Awake()
    {
        viewAngle = 120.0f;
        viewDistance = 10.0f;
        isFind = false;

        agent = GetComponent<NavMeshAgent>();
        RunSuccess = true;
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

        Debug.DrawRay(Eyes.position, leftBoundary, Color.red);
        Debug.DrawRay(Eyes.position, rightBoundary, Color.red);

        Collider[] target = Physics.OverlapSphere(Eyes.position, viewDistance, targetMask);

        if(target.Length > 0 )
        {
            Transform targetTF = target[0].gameObject.transform;
            Vector3 direction = (targetTF.position - transform.position).normalized;
            float angle = Vector3.Angle(direction, transform.forward);

            if (angle <= viewAngle * 0.5f)
            {
                Debug.DrawRay(Eyes.position, direction, Color.blue);
                if (Physics.Raycast(Eyes.transform.position, direction, out RaycastHit hit, viewDistance))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        isFind = true;
                        RunSuccess = false;
                        this.target = hit.transform;
                        //agent.destination = hit.transform.position;
                        if(Vector3.Distance(transform.position, hit.transform.position) <= AttackDistance)
                        {
                            isAttackAble = true;
                        }
                        else
                        {
                            isAttackAble = false;
                        }
                    }
                }
                else
                {
                    MissTarget();
                }
            }
            else
            {
                MissTarget();
            }
        }
        else
        {
            MissTarget();
            RunSuccess = true;
        }
    }

    private void MissTarget()
    {
        isFind = false;
        isAttackAble = false;
    }
}
