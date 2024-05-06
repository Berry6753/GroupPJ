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

    private readonly int hashWalk = Animator.StringToHash("Move");
    private readonly int hashLookAround = Animator.StringToHash("LookAround");

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
            owner.animator.SetBool(owner.hashWalk, false);
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
            owner.animator.SetBool(owner.hashWalk, true);
            
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
                           
        }

        public override void FixedUpdate()
        {

            Debug.Log("패트롤 중...");

            if (Vector3.Distance(owner.transform.position, targetPos) <= 0.2f)
            {
                ChangePatrolPoint();
                //주위 둘러보는 State로 변경
                owner.stateMachine.ChangeState(State.LookAround);
            }
            else
            {
                owner.agent.SetDestination(targetPos);
            }            
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
            owner.animator.SetBool(owner.hashWalk, true);
        }

        public override void Update()
        {

            //Debug.Log("플레이어 쫓기");
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
            owner.animator.SetBool(owner.hashWalk, false);
            owner.animator.SetBool(owner.hashLookAround, true);
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
            owner.animator.SetBool(owner.hashLookAround, false);
            Debug.Log($"주위 감지 종료, 지난 시간 : {owner.timer}, 종료 지정 시간 : {EndTime}");
            owner.timer = 0;
        }
    }

    private class AimingState : BaseMonstgerState
    {
        float AttackDelay = 1.5f;

        public AimingState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.state = State.Aiming;

            owner.agent.isStopped = true;
            owner.animator.SetBool(owner.hashIdle, false);
            owner.animator.SetBool(owner.hashWalk, false);
            owner.animator.SetBool(owner.hashFind, true);
            owner.animator.SetBool(owner.hashAttack, false);

            //Debug.Log("플레이어 조준...");
        }

        public override void Update()
        {
            owner.timer += Time.deltaTime;

            if (owner.timer >= AttackDelay)
            {
                owner.isAttack = true;
            }
        }       
    }
    private class AttackState : BaseMonstgerState
    {
        public AttackState(Monster owner) : base(owner) { }
        public override void Enter()
        {

            //Debug.Log("공격!!");
            owner.timer = 0;
            owner.state = State.Attack;
            //owner.animator.SetBool(owner.hashAttack, true);
        }
    }
}
