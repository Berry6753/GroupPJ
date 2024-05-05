using PolyPerfect;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol,
        LookAround,
        Run,
        Assassinated,
        Hurt,
        Die
    }

    public State state = State.Idle;

    private NavMeshAgent agent;
    private Animator animator;
    private StateMachine stateMachine;

    private View BossSight;

    private readonly int hashWalk = Animator.StringToHash("Move");
    private readonly int hashLookAround = Animator.StringToHash("LookAround");
    private readonly int hashFind = Animator.StringToHash("isFind");
    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashAmbushe = Animator.StringToHash("Ambushed");
    private readonly int hashDie = Animator.StringToHash("Die");

    private float timer = 0;

    private bool isAmbushed;
    private bool isHurt;
    private bool isDead;

    [Header("보스의 체력")]
    [SerializeField]
    private float HP = 100f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        BossSight = GetComponent<View>();

        stateMachine = gameObject.AddComponent<StateMachine>();
        stateMachine.AddState(State.Idle, new IdleState(this));
        stateMachine.AddState(State.Patrol, new PatrolState(this));
        stateMachine.AddState(State.LookAround, new LookAroundState(this));
        stateMachine.AddState(State.Run, new RunState(this));
        stateMachine.AddState(State.Assassinated, new AssassinatedState(this));
        stateMachine.AddState(State.Hurt, new HurtState(this));
        stateMachine.AddState(State.Die, new DieState(this));

        stateMachine.InitState(State.Idle);
    }

    private void Start()
    {
        StartCoroutine(CheckState());
    }

    private IEnumerator CheckState()
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
            else if (!BossSight.RunSuccess)
            {
                stateMachine.ChangeState(State.Run);
            }
            else if (state != State.LookAround)
            {
                stateMachine.ChangeState(State.Patrol);
            }
        }

        stateMachine.ChangeState(State.Die);
        yield break;
    }

    //private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    //{
    //    Vector3 randPoint = center + Random.insideUnitSphere * range;
    //    NavMeshHit hit;
    //    if(NavMesh.SamplePosition(randPoint, out hit, 1.0f, NavMesh.AllAreas))
    //    {
    //        result = hit.position;
    //        return true;
    //    }

    //    result = Vector3.zero;
    //    return false;
    //}

    private void Hurt(float Damage)
    {
        this.HP -= Damage;
        if (HP <= 0 || isAmbushed)
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
        protected Boss owner;
        public BaseMonstgerState(Boss owner) { this.owner = owner; }
    }

    private class IdleState : BaseMonstgerState
    {
        public IdleState(Boss owner) : base(owner) { }

        public override void Enter()
        {
            owner.state = State.Idle;
            owner.agent.isStopped = true;
            owner.animator.SetBool(owner.hashWalk, false);

            owner.agent.speed = 3.5f;
            owner.agent.angularSpeed = 130f;
        }
    }

    private class PatrolState : BaseMonstgerState
    {

        public PatrolState(Boss owner) : base(owner) { }

        public override void Enter()
        {
            owner.state = State.Patrol;
            owner.agent.isStopped = false;
            owner.animator.SetBool(owner.hashWalk, true);
            owner.animator.SetBool(owner.hashFind, false);

            owner.agent.speed = 3.5f;
            owner.agent.angularSpeed = 130f;
        }

        private bool RandomPoint(Vector3 center, float range, out Vector3 result)
        {
            Vector3 randPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }

            result = Vector3.zero;
            return false;
        }

        public override void FixedUpdate()
        {
            if(owner.agent.remainingDistance <= owner.agent.stoppingDistance)
            {
                Vector3 point;
                if(RandomPoint(owner.transform.position, 5f, out point))
                {
                    owner.agent.SetDestination(point);
                }
            }

            //Debug.Log("패트롤 중...");
        }
    }

    private class RunState : BaseMonstgerState
    {
        Vector3 RunDir;
        public RunState(Boss owner) : base(owner) { }
        public override void Enter()
        {
            owner.state = State.Run;
            owner.agent.isStopped = false;
            owner.animator.SetBool(owner.hashWalk, true);
            owner.animator.SetBool(owner.hashFind, true);

            owner.agent.speed = 8f;
            owner.agent.angularSpeed = 200f;
        }

        public override void Update()
        {
            RunDir = new Vector3(owner.BossSight.target.position.x - owner.transform.position.x, 0, owner.BossSight.target.position.z - owner.transform.position.z).normalized;
            owner.agent.SetDestination(owner.transform.position - RunDir * 5f);
        }
    }

    private class LookAroundState : BaseMonstgerState
    {
        float EndTime;
        public LookAroundState(Boss owner) : base(owner) { }
        public override void Enter()
        {
            EndTime = Random.Range(3f, 6f);

            owner.state = State.LookAround;
            owner.agent.isStopped = true;
            owner.animator.SetBool(owner.hashLookAround, true);

        }

        public override void Update()
        {
            owner.timer += Time.deltaTime;
            Debug.Log("주위 감지 중...");

            if (owner.timer >= EndTime)
            {
                owner.stateMachine.ChangeState(State.Patrol);
            }
        }

        public override void Exit()
        {
            owner.animator.SetBool(owner.hashLookAround, false);
            Debug.Log($"주위 감지 종료");
            owner.timer = 0;
        }
    }

    private class AssassinatedState : BaseMonstgerState
    {
        public AssassinatedState(Boss owner) : base(owner) { }
        public override void Enter()
        {
            owner.state = State.Assassinated;
            owner.agent.isStopped = true;

            owner.animator.SetBool(owner.hashWalk, false);
            //공격 받는 애니메이션
            owner.animator.SetBool(owner.hashAmbushe, true);
        }
    }

    private class HurtState : BaseMonstgerState
    {
        public HurtState(Boss owner) : base(owner) { }
        public override void Enter()
        {
            owner.state = State.Hurt;
            owner.agent.isStopped = true;
            owner.isHurt = true;

            owner.animator.SetBool(owner.hashWalk, false);
            owner.animator.SetBool(owner.hashFind, false);
            //공격 받는 애니메이션
            owner.animator.SetTrigger(owner.hashHurt);

            Debug.Log("공격받다.");
        }
    }

    private class DieState : BaseMonstgerState
    {
        public DieState(Boss owner) : base(owner) { }
        public override void Enter()
        {
            owner.agent.isStopped = true;
            owner.transform.tag = "Dead";

            owner.animator.SetBool(owner.hashWalk, false);
            //공격 받는 애니메이션
            owner.animator.SetTrigger(owner.hashDie);

            Debug.Log("죽다.");
        }
    }
}
