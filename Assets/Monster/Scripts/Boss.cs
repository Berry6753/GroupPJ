using PolyPerfect;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

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
    private bool isRunner;
    private bool isDead;

    [Header("보스의 체력")]
    [SerializeField]
    private float HP = 100f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
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

    private class BaseMonsterState : BaseState
    {
        protected Boss owner;

        public BaseMonsterState(Boss owner) { this.owner = owner; }
    }

    private class IdleState : BaseMonsterState

    {
        public IdleState(Boss owner) : base(owner) { }

        public override void Enter()
        {
            owner.state = State.Idle;
            owner.agent.isStopped = true;

            owner.animator.SetBool(owner.hashWalk, false);

            owner.agent.speed = 3.5f;
            owner.agent.angularSpeed = 130f;

            owner.stateMachine.ChangeState(State.Patrol);
        }
    }

    private class PatrolState : BaseMonsterState
    {
        Vector3 point;
        Vector3 MovePoint; 

        public PatrolState(Boss owner) : base(owner) { }

        public override void Enter()
        {
            owner.state = State.Patrol;
            owner.agent.isStopped = false;
            owner.animator.SetBool(owner.hashWalk, true);
            owner.animator.SetBool(owner.hashFind, false);

            owner.agent.speed = 3.5f;
            owner.agent.angularSpeed = 130f;

            MovePoint = owner.transform.position;
            //point = MovePoint;

            RandomPoint(owner.transform.position, 15f, out point);
        }

        private void RandomPoint(Vector3 center, float range, out Vector3 result)
        {  
            Vector3 randPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                MovePoint = hit.position;
                result = MovePoint;
            }

            result = MovePoint;
        }

        public override void FixedUpdate()
        {
            Debug.Log("패트롤 중...");           

            Debug.Log($"위치 : {owner.transform.position}, 이동 포인트 : {point}");
            Debug.Log($"남은 거리 : {owner.agent.remainingDistance}");

            if (owner.agent.remainingDistance <= 0.5f)
            {
                //RandomPoint(owner.transform.position, 5f, out point);
                owner.stateMachine.ChangeState(State.LookAround);
            }
            else
            {
                owner.agent.SetDestination(point);
            }
        }
    }

    private class RunState : BaseMonsterState
    {
        Vector3 RunDir;
        public RunState(Boss owner) : base(owner) { }
        public override void Enter()
        {
            owner.state = State.Run;
            owner.agent.isStopped = false;
            owner.isRunner = true;
            owner.animator.SetBool(owner.hashWalk, true);
            owner.animator.SetBool(owner.hashFind, true);

            owner.agent.speed = 8f;
            owner.agent.angularSpeed = 200f;

            owner.isRunner = true;
        }

        public override void FixedUpdate()
        {
            Debug.Log("도망 중...");
            RunDir = new Vector3(owner.BossSight.target.position.x - owner.transform.position.x, 0, owner.BossSight.target.position.z - owner.transform.position.z).normalized;
            owner.agent.SetDestination(owner.transform.position - RunDir * 5f);

            if (owner.BossSight.RunSuccess)
            {
                owner.stateMachine.ChangeState(State.LookAround);
            }
        }

    }

    private class LookAroundState : BaseMonsterState
    {
        float EndTime = 2f;

        Quaternion newRotation;
        public LookAroundState(Boss owner) : base(owner) { }
        public override void Enter()
        {
            owner.state = State.LookAround;
            owner.agent.isStopped = true;

            owner.animator.SetBool(owner.hashWalk, false);
            owner.animator.SetBool(owner.hashFind, false);
            owner.animator.SetBool(owner.hashLookAround, true);

            if (owner.isRunner)
            {
                newRotation = Quaternion.LookRotation((owner.BossSight.target.transform.position - owner.transform.position).normalized);
            }
        }

        public override void Update()
        {
            if (owner.isRunner)
            {
                owner.transform.rotation = Quaternion.Lerp(owner.transform.rotation, this.newRotation, 2f * Time.deltaTime);
            }            

            owner.timer += Time.deltaTime;
            Debug.Log("주위 감지 중...");

            if (owner.timer >= EndTime)
            {
                owner.isRunner = false;
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

    private class AssassinatedState : BaseMonsterState
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

    private class HurtState : BaseMonsterState
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


    private class DieState : BaseMonsterState
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
            owner.state = State.Die;
        }
    }
}
