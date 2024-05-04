using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerStateName
{
    IDLE, WALK, RUN, CROUCHING, CROUCHINGWALK,
}

public class PlayerController : Singleton<PlayerController>
{
    public float moveSpeed;
    public bool isGround = true;

    public PlayerStateName playerState = PlayerStateName.IDLE;
    protected CharacterController characterController;

    [SerializeField]
    private GameObject leftHandAttackPos;
    [SerializeField]
    private GameObject rightHandAttackPos;

    private StateMachine playerStateMachine;
    private Vector3 moveDirection;
    private Vector3 jumpDirection = new Vector3(0, 0, 0);
    private Animator playerAnimator;


    private float jumpForce = 3.0f;
    private float gravtyScale = -0.02f;
    private float attackTime;
    private float maxComboInputTime = 0.5f;
    private float attakingTime = 3.0f;
    private int comboCount = 0;
    private bool isCrouching = false;
    private bool isJump = false;
    private bool isAttack = false;
    private bool isAimming = false;


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerAnimator = GetComponent<Animator>();
        playerStateMachine = gameObject.AddComponent<StateMachine>();
        playerStateMachine.AddState(PlayerStateName.IDLE, new IdleState(this));
        playerStateMachine.AddState(PlayerStateName.WALK, new WalkState(this));
        playerStateMachine.InitState(PlayerStateName.IDLE);

        jumpDirection.y = jumpForce;
    }

    private void FixedUpdate()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isGround)
        {
            jumpDirection.y += gravtyScale;
            characterController.Move(jumpDirection * Time.deltaTime);
        }
        else
        {
            jumpDirection.y = jumpForce;
        }

        AttackingTimeCheck();
    }

    private void OnAttack()
    {
        isAttack = true;
        playerAnimator.SetLayerWeight(1, 1);
        playerAnimator.SetBool("IsAttack", true);
        attackTime = Time.time;
        if (!isAimming)
        {
            if (Time.time - attackTime <= maxComboInputTime)
            {
                if (comboCount == 0)
                {
                    playerAnimator.SetTrigger("LeftAttack");
                    comboCount++;
                }
                else if (comboCount == 1)
                {
                    playerAnimator.SetTrigger("RightAttack");
                    comboCount--;
                }
            }
        }



    }

    private void AttackingTimeCheck()
    {
        if (Time.time - attackTime > attakingTime)
        {
            isAttack = false;
            comboCount = 0;
            playerAnimator.SetBool("IsAttack", false);
            playerAnimator.SetLayerWeight(1, 0);
        }

    }

    private void OnJump()
    {
        if (isGround)
        {
            isGround = false;
        }

    }

    private void OnMove(InputValue input)
    {
        Vector2 moveVector = input.Get<Vector2>();
        moveDirection = new Vector3(moveVector.x, 0, moveVector.y);
        playerStateMachine.ChangeState(PlayerStateName.WALK);
    }

    private void SetAnimatorFloat(float Xvalue, float Zvalue)
    {
        playerAnimator.SetFloat("Xspeed", Xvalue);
        playerAnimator.SetFloat("Zspeed", Zvalue);
    }


    private class PlayerBaseState : BaseState
    {
        protected PlayerController player;

        public PlayerBaseState(PlayerController player)
        {
            this.player = player;
        }
    }



    private class IdleState : PlayerBaseState
    {
        public IdleState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            player.playerState = PlayerStateName.IDLE;
            //애니메이션 세팅
            player.SetAnimatorFloat(0, 0);
            //캐릭터 컨트롤러 콜리전 세팅 센터값 0 , 0.99 ,0
            //높이 1.8
            //앉기일 경우 0, 0.49 ,0
            //높이 1
        }

        public override void Update()
        {
        }
    }

    private class WalkState : PlayerBaseState
    {
        public WalkState(PlayerController player) : base(player) { }

        public override void Enter()
        {
        }

        public override void FixedUpdate()
        {
        }

        public override void Update()
        {
            player.characterController.Move(player.moveDirection * player.moveSpeed * Time.deltaTime);
            player.SetAnimatorFloat(player.moveDirection.x * player.moveSpeed, player.moveDirection.z * player.moveSpeed);


            if (player.characterController.velocity.magnitude == 0)
            {
                player.playerStateMachine.ChangeState(PlayerStateName.IDLE);
            }
        }
    }

}
