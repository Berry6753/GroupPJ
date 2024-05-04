using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol,
        Trace,
        LookAround,
        Attack,
        Die
    }

    public State state = State.Idle;

    private Animator animator;
    private NavMeshAgent agent;
    private StateMachine stateMachine;

    [SerializeField]
    private List<Transform> PatrolPoint;

    private View monsterSight;

    private readonly int hashTrace = Animator.StringToHash("Move");
    //private readonly int hashAttack = Animator.StringToHash("isAttack");

    private float timer;

    private bool isDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        monsterSight = GetComponent<View>();

        stateMachine = gameObject.AddComponent<StateMachine>();
        stateMachine.AddState(State.Idle, new IdleState(this));
        stateMachine.AddState(State.Patrol, new PatrolState(this, PatrolPoint));
        stateMachine.AddState(State.Trace, new TraceState(this));
        stateMachine.AddState(State.LookAround, new LookAroundState(this));
        stateMachine.AddState(State.Attack, new AttackState(this));

        stateMachine.InitState(State.Idle);
    }

    private void Start()
    {
        StartCoroutine(CheckMonsterState());
    }

    private IEnumerator CheckMonsterState()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(0.3f);

            if (state == State.Die)
            {
                stateMachine.ChangeState(State.Die);
                yield break;
            }

            if (monsterSight.isAttackAble)
            {
                stateMachine.ChangeState(State.Attack);
            }
            else if (monsterSight.isFind)
            {
                stateMachine.ChangeState(State.Trace);
            }
            //else if(state == State.LookAround)
            //{
            //    stateMachine.ChangeState(State.LookAround);
            //}
            else if(state != State.LookAround)
            {
                stateMachine.ChangeState(State.Patrol);
            }
        }
    }

    private class BaseMonstgerState : BaseState
    {
        protected Monster owner;
        public BaseMonstgerState(Monster owner) { this.owner = owner; }
    }

    private class IdleState : BaseMonstgerState
    {
        public IdleState(Monster owner) : base(owner) { }

        public override void Enter()
        {
            owner.state = State.Idle;
            owner.agent.isStopped = true;
            owner.animator.SetBool(owner.hashTrace, false);
        }
    }

    private class PatrolState : BaseMonstgerState
    {
        List<Transform> PatrolPoint;
        Vector3 targetPos;

        int patrolPointIndex = 0;
        int NextPatrolPointIndex = 1;

        public PatrolState(Monster owner, List<Transform> PatrolPoint) : base(owner) 
        {
            this.PatrolPoint = PatrolPoint;
        }

        public override void Enter()
        {
            owner.state = State.Patrol;
            owner.agent.isStopped = false;
            owner.animator.SetBool(owner.hashTrace, true);
            
            targetPos = PatrolPoint[patrolPointIndex].position;
        }

        private void ChangePatrolPoint()
        {
            if(Vector3.Distance(owner.transform.position, targetPos) < 0.1f)
            {
                if(patrolPointIndex >= PatrolPoint.Count -1)
                {
                    NextPatrolPointIndex = -1;
                }
                else if(patrolPointIndex <= 0)
                {
                    NextPatrolPointIndex = 1;
                }

                patrolPointIndex += NextPatrolPointIndex;
                targetPos = PatrolPoint[patrolPointIndex].position;

                //주위 둘러보는 State로 변경
                owner.stateMachine.ChangeState(State.LookAround);
            }
        }

        public override void FixedUpdate()
        {
            ChangePatrolPoint();
            owner.agent.SetDestination(targetPos);

            Debug.Log("패트롤 중...");
        }
    }

    private class TraceState : BaseMonstgerState
    {
        Vector3 targetPos;
        public TraceState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.state = State.Trace;
            targetPos = owner.monsterSight.target.position;
            owner.agent.isStopped = false;
            owner.animator.SetBool(owner.hashTrace, true);
        }

        public override void Update()
        {
            owner.agent.SetDestination(targetPos);
        }
    }

    private class LookAroundState : BaseMonstgerState
    {
        float EndTime;
        public LookAroundState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            EndTime = Random.Range(3f, 6f);

            owner.state = State.LookAround;
            owner.agent.isStopped = true;
            owner.animator.SetBool(owner.hashTrace, false);
            //owner.animator.SetBool(owner.hashAttack, true);
        }

        public override void Update()
        {
            owner.timer += Time.deltaTime;
            Debug.Log("주위 감지 중...");

            if(owner.timer >= EndTime)
            {
                owner.stateMachine.ChangeState(State.Patrol);
            }
        }

        public override void Exit()
        {            
            Debug.Log($"주위 감지 종료, 지난 시간 : {owner.timer}, 종료 지정 시간 : {EndTime}");
            owner.timer = 0;
        }
    }

    private class AttackState : BaseMonstgerState
    {
        public AttackState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.state = State.Attack;
            //owner.animator.SetBool(owner.hashAttack, true);
        }
    }
}
