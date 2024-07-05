using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Attack_Base;
using static Enemy_Base;

public class Player_Controller : MonoBehaviour
{
    StateMachine<PlayerStates, StateDriverUnity> sm;

    public enum PlayerStates
    {
        Init,
        Basic,
        Attack,
        AirAttack,
        Rolling,
        Jumping,
        Break,
        Dead
    }

    [SerializeField] PlayerStates currentState;

    #region Global Methods

    [Header("Player Setup")]
    [SerializeField] PlayerControls playerControls;
    [SerializeField] PlayerManager manager;
    [SerializeField] public CharacterController cc;
    [SerializeField] public Animator animator;
    [SerializeField] private PlayerCameraController camera;

    [Header("Global Variables")]
    [SerializeField] bool stopGravityCoroutine;
    [SerializeField] public float minDistance;
    [SerializeField] public bool blocking;

    private void Awake()
    {
        sm = new StateMachine<PlayerStates, StateDriverUnity>(this);
        sm.ChangeState(PlayerStates.Init);
    }
    public void TriggerChangeState(PlayerStates state)
    {
        sm.ChangeState(state);
    }

    private void Update()
    {
        animator.SetBool("air", !isGrounded);
        animator.SetBool("grounded", isGrounded);
        sm.Driver.Update.Invoke();
    }
    private void FixedUpdate()
    {
        sm.Driver.FixedUpdate.Invoke();
        Vector3 verticalDirection = Vector3.up * -gravity * Time.deltaTime;
        cc.Move(verticalDirection);
    }

    private void InitiateInputs()
    {
        playerControls.PlayerController.PlayerMove.performed += OnPlayerMove;
        playerControls.PlayerController.PlayerMove.canceled += OnPlayerMove;
        playerControls.PlayerController.Camera.performed += OnCamera;
        playerControls.PlayerController.Camera.canceled += OnCamera;
        playerControls.PlayerController.Jump.performed += OnJump;
        playerControls.PlayerController.Jump.canceled += OnJump;
        playerControls.PlayerController.Attack.performed += OnAttack;
        playerControls.PlayerController.Attack.canceled += OnAttack;
        playerControls.PlayerController.BlockRoll.performed += OnBlockRoll;
        playerControls.PlayerController.BlockRoll.canceled += OnBlockRoll;
        playerControls.PlayerController.SwitchWeapon.performed += OnSwitchWeapon;
        playerControls.PlayerController.LockOn.performed += OnLockOn;
        //playerControls.PlayerController.SwitchWeapon.canceled += OnSwitchWeapon;
    }

    public float currentYForce;
    public Vector3 CalculateMovement()
    {
        float movementValue = Mathf.Max(Mathf.Abs(inputHorizontal), Mathf.Abs(inputVertical));
        animator.SetFloat("Movement", Mathf.Abs(movementValue));

        // Direction movement
        float directionX = inputHorizontal * (velocity) * Time.deltaTime;
        float directionZ = inputVertical * (velocity) * Time.deltaTime;
        float directionY = 0;

        if (isJumping)
        {
            if ((inputJump || jumpElapsedTime < jumpTime / 4) && jumpElapsedTime < jumpTime)
            {
                // Apply inertia and smoothness when climbing the jump
                directionY = Mathf.SmoothStep(jumpForce, 0, jumpElapsedTime / jumpTime) * Time.deltaTime;
                currentYForce = Mathf.SmoothStep(jumpForce, 0, jumpElapsedTime / jumpTime);
            }
            else
            {
                // Apply smooth descent transition
                float remainingTime = jumpTime - jumpElapsedTime;
                if (remainingTime > 0)
                {
                    directionY = Mathf.SmoothStep(currentYForce/2, 0, jumpElapsedTime / jumpTime) * Time.deltaTime;
                }
                else
                {
                    directionY = 0;
                }
            }

            // Jump timer
            jumpElapsedTime += Time.deltaTime;
            if (jumpElapsedTime >= jumpTime)
            {
                isJumping = false;
                jumpElapsedTime = 0;
            }
        }


        //directionY = directionY - gravity * Time.deltaTime;

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        // --- Character rotation --- 
        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        // Relate the front with the Z direction (depth) and right with X (lateral movement)
        forward = forward * directionZ;
        right = right * directionX;

        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.5f);
        }

        // --- End rotation ---


        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;

        Vector3 movement = verticalDirection + horizontalDirection;

        return movement;
    }

    public void ChangeWeapons(GameObject weapon)
    {
        this.weapon = weapon;
        this.weaponLogic = this.weapon.GetComponent<Weapon>();
    }

    #endregion

    #region Input Stuff

    [Header("Inputs")]
    [SerializeField] float inputHorizontal;
    [SerializeField] float inputVertical;
    [SerializeField] bool inputJump;
    [SerializeField] bool inputCrouch;
    [SerializeField] bool inputSprint;
    [SerializeField] bool inputAttack;
    [SerializeField] bool inputRoll;

    private void OnPlayerMove(InputAction.CallbackContext context)
    {
        inputHorizontal = context.ReadValue<Vector2>().x;
        inputVertical = context.ReadValue<Vector2>().y;
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        inputJump = context.ReadValueAsButton();
    }
    private void OnAttack(InputAction.CallbackContext context)
    {
        inputAttack = context.ReadValueAsButton();
    }
    private void OnBlockRoll(InputAction.CallbackContext context)
    {
        inputRoll = context.ReadValueAsButton();
    }
    private void OnCamera(InputAction.CallbackContext context)
    {
        camera.DealWithCamera(context.ReadValue<Vector2>());
    }
    private void OnLockOn(InputAction.CallbackContext context)
    {
        camera.LockOn();
    }

    private void OnSwitchWeapon(InputAction.CallbackContext context)
    {
        GetComponent<PlayerManager>().SwitchWeapon();
    }
    #endregion

    #region INIT STATE ------
    void Init_Enter() 
    {
        
        playerControls = new PlayerControls();
        InitiateInputs();
        playerControls.Enable();
        manager = GetComponent<PlayerManager>();
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        sm.ChangeState(PlayerStates.Basic);
    }
    #endregion

    #region BASIC STATE ------

    [Header("Locomotion Variables")]
    public float velocity = 5f;
    public float gravity = 9.8f;
    void Basic_Enter()
    {
        currentState = PlayerStates.Basic;
        isJumping = false;
    }

    void Basic_Update()
    {
        if (inputJump)
        {
            isJumping = true;
            jumpElapsedTime = 0;
            animator.Play("Jump", manager.currentWeapon * 2);
            sm.ChangeState(PlayerStates.Jumping);
        }
        else if (inputAttack)
        {
            sm.ChangeState(PlayerStates.Attack);
        }
        else if(inputRoll)
        {
            bool hasSignificantInput = Mathf.Abs(inputHorizontal) > 0.5f || Mathf.Abs(inputVertical) > 0.5f;
            if (hasSignificantInput)
                sm.ChangeState(PlayerStates.Rolling);
            else
                Debug.Log("This is a block");
        }
    }

    void Basic_FixedUpdate()
    {
        Vector3 movement = CalculateMovement();
        cc.Move(movement);
    }

    #endregion

    #region JUMP STATE -------
    [Header("Jump Variables")]
    [SerializeField] float jumpForce = 18f;
    [SerializeField] float jumpTime = 0.85f;
    [SerializeField] float jumpElapsedTime = 0;
    [SerializeField] public bool isGrounded;
    [SerializeField] public bool isJumping = false;
    
    void Jumping_Enter()
    {
        currentState = PlayerStates.Jumping;
        isGrounded = false;
    }

    void Jumping_Update()
    {
        if (isGrounded)
        {
            sm.ChangeState(PlayerStates.Basic);
        }
        else if (inputAttack)
        {
            isJumping = false;
            sm.ChangeState(PlayerStates.AirAttack);
        }
    }

    void Jumping_FixedUpdate()
    {
        Vector3 movement = CalculateMovement();
        cc.Move(movement);
        HeadHittingDetect();
    }

    void HeadHittingDetect() 
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;

        // Uncomment this line to see the Ray drawed in your characters head
        // Debug.DrawRay(ccCenter, Vector3.up * headHeight, Color.red);

        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc, 0))
        {
            isJumping = false;
            jumpElapsedTime = 0;
        }
    }
    #endregion

    #region ATTACK STATE --------
    [Header("Attack State Components")]
    [SerializeField] private GameObject weapon;
    [SerializeField] private Weapon weaponLogic;
    void Attack_Enter()
    {
        currentState = PlayerStates.Attack;
        weaponLogic.StopCoroutine("WaitWeaponInput");
        StartReturnGravity();
        weapon.SetActive(true);
        jumpElapsedTime = jumpTime;
        weaponLogic.StartCoroutine("WaitWeaponInput");
    }

    public void LeaveAttackState()
    {
        if(isGrounded)
        {
            sm.ChangeState(PlayerStates.Basic);
        }
        else
        {
            sm.ChangeState(PlayerStates.Jumping);
        }
    }
    #endregion

    #region AIR ATTACK STATE --------
    void AirAttack_Enter()
    {
        currentState = PlayerStates.AirAttack;
        weaponLogic.StopCoroutine("WaitWeaponInput");
        gravity = 0;
        weapon.SetActive(true);
        jumpElapsedTime = jumpTime;
        weaponLogic.StartCoroutine("WaitWeaponInput");
    }

    void AirAttack_Exit()
    {
        StartReturnGravity();
    }
    #endregion

    #region ROLLING STATE ----------
    [Header("Rolling Variables")]
    [SerializeField] bool rolling;
    [SerializeField] float rollSpeed;

    void Rolling_Enter()
    {
        animator.Play("Dodge", -1, 0f);
        currentState = PlayerStates.Rolling;
    }

    public IEnumerator DoRoll()
    {
        rolling = true;

        Vector3 direction = transform.forward;

        while (rolling && currentState == PlayerStates.Rolling)
        {
            cc.Move(direction * rollSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void StopRoll()
    {
        rolling = false;
        sm.ChangeState(PlayerStates.Basic);
    }

    #endregion

    #region BREAK STATE ----------
    [Header("Break Variables")]
    public int hitCounter = 0;
    public bool suffering;
    public float debugMultiplier;
    public float breakElapsedTime;

    void Break_Enter()
    {
        SetBoolsToFalse();
        currentState = PlayerStates.Break;
        manager.DisableColliders();
    }

    public void GetHit(Enemy_Base enemy, float damage, AttackType type, float force)
    {
        sm.ChangeState(PlayerStates.Break);
        //StopCoroutine("RecoverFromHit");

        manager.ReduceHP(damage);
        Vector3 direction = Vector3.zero;
        if (type == AttackType.Normal)
        {
            direction = enemy.transform.forward * force;
        }
        else if (type == AttackType.Finisher)
        {
            direction = (enemy.transform.forward + (transform.up / 2)) * force;
        }
        else if (type == AttackType.Thrust)
        {
            direction = enemy.transform.forward;
        }
        else if (type == AttackType.Launcher)
        {
            direction = transform.up * force;
        }
        Vector3 lookDirection = enemy.transform.position - transform.position;
        lookDirection.y = 0; // Keep the rotation in the horizontal plane
        transform.rotation = Quaternion.LookRotation(lookDirection);
        StartCoroutine(RecoverFromHit(direction));
    }

    
    private IEnumerator RecoverFromHit(Vector3 direction)
    {
        hitCounter++;
        suffering = true;
        animator.Play("Hit");
        StartReturnGravity();
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float fadeoutFactor = Mathf.Lerp(debugMultiplier, 0f, elapsedTime / duration);

            Vector3 moveDirection = direction * fadeoutFactor * Time.deltaTime;
            //moveDirection -= new Vector3(0, gravity, 0) * Time.deltaTime;
            // Apply the direction force with the fadeout factor
            cc.Move(moveDirection);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
         
        suffering = false;

        if (isGrounded)
        {
            sm.ChangeState(PlayerStates.Basic);
        }
        else
        {
            sm.ChangeState(PlayerStates.Jumping);
        }
    }
    #endregion



    #region Animation Methods
    [Header("Gravity Stuff")]
    public float gravityCoroutineTimeElapsed;
    public bool gravityCoroutineRunning = false;
    public float startingGravity;
    public float gravityCoroutineTransitionDuration;

    [Header("Animation Variables")]
    [SerializeField] public bool animMove;
    public IEnumerator ReturnGravity()
    {
        gravityCoroutineTransitionDuration = 1f; // Duration of gravity transition
        gravityCoroutineRunning = true;

        while (gravityCoroutineTimeElapsed < gravityCoroutineTransitionDuration)
        {
            gravityCoroutineTimeElapsed += Time.deltaTime;
            gravity = Mathf.Lerp(0f, startingGravity, gravityCoroutineTimeElapsed / gravityCoroutineTransitionDuration);
            if (gravityCoroutineTimeElapsed > 0.1f && (isGrounded))
            {
                gravityCoroutineTimeElapsed = gravityCoroutineTransitionDuration;
            }
            yield return null; // Wait for the next frame
        }

        gravityCoroutineRunning = false;
        gravity = startingGravity;
        // Ensure the gravity is set to the target value at the end
    }

    public void StartReturnGravity()
    {
        gravityCoroutineTimeElapsed = 0;
        gravity = 0;
        if (!gravityCoroutineRunning)
        {
            StartCoroutine(ReturnGravity());
        }
    }

    public void ApplyAnimationForce(float force)
    {

        StartCoroutine(ApplyForceOverTime(force));
    }

    private IEnumerator ApplyForceOverTime(float force)
    {
        weapon.GetComponent<Collider>().enabled = true;
        animMove = true;
        float elapsedTime = 0f;
        Vector3 forceDirection = transform.forward * force;

        while (animMove && (currentState == PlayerStates.Attack || currentState == PlayerStates.AirAttack))
        {
            if (Vector3.Distance(weaponLogic.currentEnemy.transform.position, transform.position) >= minDistance)
            {
                cc.Move(forceDirection * Time.deltaTime);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ApplyAirForceOverTime(float force)
    {
        Debug.Log("Entrei na corotine do applyforce no ar");
        weapon.GetComponent<Collider>().enabled = true;
        animMove = true;
        float elapsedTime = 0f;
        GameObject enemy = GetClosestEnemy();
        Vector3 directionToLookAt = (enemy.transform.position - transform.position).normalized;
        Vector3 forceDirection = (transform.forward + new Vector3(0, directionToLookAt.y, 0)) * force;
        while (animMove && (currentState == PlayerStates.Attack || currentState == PlayerStates.AirAttack))
        {
            if (Vector3.Distance(weaponLogic.currentEnemy.transform.position, transform.position) >= minDistance)
            {
                cc.Move(forceDirection * Time.deltaTime);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void SetAnimationMove()
    {
        weapon.GetComponent<Collider>().enabled = false;
        animMove = false;
    }

    public IEnumerator SetLayerWeightTo0()
    {
        float duration = 0.2f;
        float elapsedTime = 0f;
        float initialWeight = animator.GetLayerWeight(1);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newWeight = Mathf.Lerp(initialWeight, 0f, elapsedTime / duration);
            animator.SetLayerWeight(1, newWeight);
            yield return null;
        }

        // Ensure the weight is set to 0 at the end
        animator.SetLayerWeight(1, 0f);
    }

    private GameObject GetClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject currentEnemy = null;
        float distance = 0;
        foreach (var enemy in enemies)
        {
            if (currentEnemy == null)
            {
                currentEnemy = enemy;

            }
            else
            {
                if (Vector3.Distance(transform.position, enemy.transform.position) < distance)
                {
                    currentEnemy = enemy;
                }
            }
            distance = Vector3.Distance(transform.position, currentEnemy.transform.position);
        }

        return currentEnemy;
    }

    private void SetBoolsToFalse()
    {
        animMove = false;
        isJumping = false;
        rolling = false;
    }

    #endregion
}
