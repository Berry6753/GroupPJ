using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public enum PlayerStateName
{
    IDLE, WALK, ASSASING
}

public enum PlayerAttackType
{
    NOMAL,ASSASING,AIMMING
}


public class PlayerController : Singleton<PlayerController>
{
    public float moveSpeed;
    public bool isAssasing;
    public bool isGround = true;
    public CinemachineVirtualCamera overView;
    public CinemachineVirtualCamera aimView;
    public GameObject aim;

    public GameObject sss;

    [HideInInspector]
    public GameObject target;
    [HideInInspector]
    public Animator playerAnimator;
    public PlayerStateName playerState = PlayerStateName.IDLE;
    public GameObject assasingPos;
    protected CharacterController characterController;

    [SerializeField]
    private GameObject leftHandAttackPos;
    [SerializeField]
    private GameObject rightHandAttackPos;

    private StateMachine playerStateMachine;
    private Vector3 moveDirection;
    private Vector3 jumpDirection = new Vector3(0, 0, 0);
    private PlayerAttackType attackType = PlayerAttackType.NOMAL;
    private Transform aaa;

    private float jumpForce = 3.0f;
    private float gravtyScale = -0.02f;
    private float attackTime;
    private float maxComboInputTime = 0.5f;
    private float attakingTime = 3.0f;
    private float rootSpeed = 3.0f;
    private float yRotate;
    private float xRoatte;
    private int comboCount = 0;
    private bool isCrouching = false;
    private bool isJump = false;
    private bool isAttack = false;
    private bool isAimming = false;
    private bool isAssasingAttack = false;



    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
        playerAnimator = GetComponent<Animator>();
        playerStateMachine = gameObject.AddComponent<StateMachine>();
        overView.Priority = 10;
        aimView.Priority = 0;
        playerStateMachine.AddState(PlayerStateName.IDLE, new IdleState(this));
        playerStateMachine.AddState(PlayerStateName.WALK, new WalkState(this));
        playerStateMachine.InitState(PlayerStateName.IDLE);
        aaa = playerAnimator.GetBoneTransform(HumanBodyBones.Spine);
        leftHandAttackPos.SetActive(false);
        rightHandAttackPos.SetActive(false);
        jumpDirection.y = jumpForce;
    }

    private void FixedUpdate()
    {

    }

    private void LateUpdate()
    {
        if (isAimming)
        {
            aaa.rotation = Quaternion.Euler(0f, 0, yRotate * rootSpeed);

        }
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
        MoveAssasingTarget();
        PlayerRotate();
        aimView.LookAt = aim.transform;
    }

    private void OnAimming()
    {
        if (isAimming)
        {
            overView.gameObject.SetActive(true);
            aimView.gameObject.SetActive(false);
            isAimming = false;
        }
        else
        {
            aimView.gameObject.SetActive(true);
            overView.gameObject.SetActive(false);
            isAimming = true;
        }
    }
    private void CamaraChange()
    {

    }


    private void PlayerRotate()
    {
        transform.Rotate(0f, Input.GetAxis("Mouse X") * rootSpeed, 0f);
        yRotate += Input.GetAxis("Mouse Y") * rootSpeed;
        yRotate = Mathf.Clamp(yRotate, 50, 100);
        


    }

    private void PlayerYRoatate()
    {

    }

    void OnCrouching()
    {
        if (!isAssasingAttack)
        {
            if (!isCrouching)
            {
                isCrouching = true;
                moveSpeed = 1.0f;
                //애니세팅
                playerAnimator.SetBool("IsCrouching", true);
                //앉기일 경우 0, 0.49 ,0
                characterController.center = new Vector3(0, 0.49f, 0);
                //높이 1
                characterController.height = 1;

            }
            else
            {
                isCrouching = false;
                moveSpeed = 1.0f;
                //애니세팅
                playerAnimator.SetBool("IsCrouching", false);
                //캐릭터 컨트롤러 콜리전 세팅 센터값 0 , 0.99 ,0
                characterController.center = new Vector3(0, 0.99f, 0);
                //높이 1.8
                characterController.height = 1.8f;
            }

        }



    }

    private void OnAssasing()
    {
        if (isAssasing&&!isAttack)
        {
            attackType = PlayerAttackType.ASSASING;
            isCrouching = false;
            moveSpeed = 1.0f;
            //애니세팅
            //playerAnimator.SetLayerWeight(1, 1);
            playerAnimator.SetBool("IsCrouching", false);
            playerAnimator.SetTrigger("AssasingAttackStart");
            //캐릭터 컨트롤러 콜리전 세팅 센터값 0 , 0.99 ,0
            characterController.center = new Vector3(0, 0.99f, 0);
            //높이 1.8
            characterController.height = 1.8f;
            target.gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
        }
        
    }

    private void OnAttack()
    {

        switch (attackType)
        {
            case PlayerAttackType.NOMAL:
                NomalAttack();
                break;
            case PlayerAttackType.ASSASING:
                AssaingAttack();
                break;
            case PlayerAttackType.AIMMING:
                break;
        }

    }

    public void SetAssingAttackingAni()
    {
        playerAnimator.SetTrigger("AssasinAttacking");
    }
    private void MoveAssasingTarget()
    {
        if (attackType == PlayerAttackType.ASSASING)
        {
            target.transform.position = assasingPos.transform.position;
        }
    }

    private void AssaingAttack()
    {
        playerAnimator.SetTrigger("AssasingAttackFinish");
        attackType = PlayerAttackType.NOMAL;
    }

    private void NomalAttack()
    {
        isAttack = true;
       // playerAnimator.SetLayerWeight(1, 1);
        playerAnimator.SetBool("IsAttack", true);
        attackTime = Time.time;

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

    public void LeftAttack()
    {
        if (leftHandAttackPos.activeSelf == false)
        {
            leftHandAttackPos.SetActive(true);
        }
        else
        {
            leftHandAttackPos.SetActive(false);
        }
    }

    public void RightAttack()
    {
        if (rightHandAttackPos.activeSelf == false)
        {
            rightHandAttackPos.SetActive(true);
        }
        else
        {
            rightHandAttackPos.SetActive(false);
        }
    }

    private void AttackingTimeCheck()
    {
        if (Time.time - attackTime > attakingTime)
        {
            isAttack = false;
            comboCount = 0;
            playerAnimator.SetBool("IsAttack", false);
            //playerAnimator.SetLayerWeight(1, 0);
        }

    }

    private void OnJump()
    {
        if (isGround && !isAssasing)
        {
            isGround = false;
            playerAnimator.SetBool("IsGround", isGround);
        }

    }

    private void OnMove(InputValue input)
    {
        if(!isAssasingAttack)
        {
            Vector2 moveVector = input.Get<Vector2>();
            moveDirection = new Vector3(moveVector.x, 0, moveVector.y);
            playerStateMachine.ChangeState(PlayerStateName.WALK);
        }
        
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
            player.characterController.Move(player.transform.TransformDirection( player.moveDirection) * player.moveSpeed * Time.deltaTime);
            if (player.isGround)
            {
                player.SetAnimatorFloat(player.moveDirection.x * player.moveSpeed, player.moveDirection.z * player.moveSpeed);

            }


            if (player.characterController.velocity.magnitude == 0)
            {
                player.playerStateMachine.ChangeState(PlayerStateName.IDLE);
            }
        }
    }

}
