using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class Monster : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol,
        Trace,
        LookAround,
        Aiming,
        Attack,
        Assassinated,
        Hurt,
        Die
    }

    public State state = State.Idle;

    private Animator animator;
    private NavMeshAgent agent;
    public StateMachine stateMachine;

    private View monsterSight;

    private readonly int hashWalk = Animator.StringToHash("Move");
    private readonly int hashLookAround = Animator.StringToHash("LookAround");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashFind = Animator.StringToHash("isFind");
    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashIdle = Animator.StringToHash("Idle");
    private readonly int hashAmbushe = Animator.StringToHash("Ambushed");
    private readonly int hashBackWard = Animator.StringToHash("BackWard");
    private readonly int hashDie = Animator.StringToHash("Die");

    private float timer;

    private bool isAttack;
    public bool isAmbushed;
    private bool isHurt;
    private bool isDead;

    [Header("몬스터의 체력")]
    [SerializeField]
    private float HP = 100f;

    [Space(10)]
    [Header("사용할 총기")]
    [SerializeField]
    private Transform EquipedGun;

    [Space(10)]
    [Header("패트롤할 포인트")]
    [SerializeField]
    private Transform PatrolPointGroup;

    private List<Transform> PatrolPoint;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        monsterSight = GetComponent<View>();

        PatrolPoint = new List<Transform>();

        for (int i = 0; i < PatrolPointGroup.childCount; i++)
        {
            PatrolPoint.Add(PatrolPointGroup.GetChild(i));
        }

        stateMachine = gameObject.AddComponent<StateMachine>();
        stateMachine.AddState(State.Idle, new IdleState(this));
        stateMachine.AddState(State.Patrol, new PatrolState(this, PatrolPoint));
        stateMachine.AddState(State.Trace, new TraceState(this));
        stateMachine.AddState(State.LookAround, new LookAroundState(this));
        stateMachine.AddState(State.Aiming, new AimingState(this));
        stateMachine.AddState(State.Attack, new AttackState(this));
        stateMachine.AddState(State.Assassinated, new AssassinatedState(this));
        stateMachine.AddState(State.Hurt, new HurtState(this));
        stateMachine.AddState(State.Die, new DieState(this));

        stateMachine.InitState(State.Idle);

        isAmbushed = false;
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

            if (isHurt)
            {
                continue;
            }
            else if (isAmbushed)
            {
                stateMachine.ChangeState(State.Assassinated);
            }
            else if (isAttack)
            {
                stateMachine.ChangeState(State.Attack);
            }
            else if (monsterSight.isAttackAble)
            {
                stateMachine.ChangeState(State.Aiming);
            }
            else if (monsterSight.isFind)
            {
                stateMachine.ChangeState(State.Trace);
            }
            else if(state != State.LookAround)
            {
                stateMachine.ChangeState(State.Patrol);
            }
        }

        stateMachine.ChangeState(State.Die);
        yield break;
    }

    private void Update()
    {
        //플레이어를 바라보며 회전
        if (monsterSight.isAttackAble)
        {
            Quaternion newRotation = Quaternion.LookRotation((monsterSight.target.transform.position - transform.position).normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 2f * Time.deltaTime);
        }
    }

    //Attack Animation Method..
    private void AttackEnd()
    {
        isAttack = false;
    }

    //Hurt
    private void Hurt(float Damage)
    {
        this.HP -= Damage;
        if(HP <=0 || isAmbushed)
        {
            isDead = true;
        }
        else
        {
            stateMachine.ChangeState(State.Hurt);
        }
    }

    private void HurtEnd()
    {
        isHurt = false;
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
            owner.animator.SetBool(owner.hashIdle, true);
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
            owner.animator.SetBool(owner.hashIdle, true);
            owner.animator.SetBool(owner.hashWalk, true);
            owner.animator.SetBool(owner.hashFind, false);

            targetPos = PatrolPoint[patrolPointIndex].position;
        }

        private void ChangePatrolPoint()
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
            owner.animator.SetBool(owner.hashIdle, true);
            owner.animator.SetBool(owner.hashWalk, true);
            owner.animator.SetBool(owner.hashFind, true);
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

            if(owner.timer >= EndTime)
            {
                owner.stateMachine.ChangeState(State.Patrol);
            }

            Debug.Log("사주 경계 중...");
        }

        public override void Exit()
        {
            Debug.Log("사주 경계 종료");
            owner.animator.SetBool(owner.hashLookAround, false);
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
            owner.animator.SetBool(owner.hashAttack, true);
        }
    }

    private class AssassinatedState : BaseMonstgerState
    {
        public AssassinatedState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.state = State.Assassinated;
            owner.agent.isStopped = true;

            owner.animator.SetBool(owner.hashWalk, false);
            owner.animator.SetBool(owner.hashIdle, false);
            //공격 받는 애니메이션
            owner.animator.SetBool(owner.hashAmbushe, true);
        }
    }

    private class HurtState : BaseMonstgerState
    {
        public HurtState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.state = State.Hurt;
            owner.agent.isStopped = true;
            owner.isHurt = true;

            owner.animator.SetBool(owner.hashWalk, false);
            owner.animator.SetBool(owner.hashIdle, false);
            owner.animator.SetBool(owner.hashFind, false);
            owner.animator.SetBool(owner.hashAttack, false);
            //공격 받는 애니메이션
            owner.animator.SetTrigger(owner.hashHurt);
        }
    }

    private class DieState : BaseMonstgerState
    {
        public DieState(Monster owner) : base(owner) { }
        public override void Enter()
        {
            owner.agent.isStopped = true;
            owner.transform.tag = "Dead";

            owner.animator.SetBool(owner.hashWalk, false);
            owner.animator.SetBool(owner.hashIdle, false);
            //공격 받는 애니메이션
            owner.animator.SetTrigger(owner.hashDie);
        }
    }
}
