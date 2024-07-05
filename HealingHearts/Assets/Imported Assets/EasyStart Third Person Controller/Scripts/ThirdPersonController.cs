//using System.Collections;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Runtime.CompilerServices;
//using Unity.VisualScripting;
//using UnityEditor.VersionControl;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.XR;
//using static Attack_Base;

///*
//    This file has a commented version with details about how each line works. 
//    The commented version contains code that is easier and simpler to read. This file is minified.
//*/


///// <summary>
///// Main script for third-person movement of the character in the game.
///// Make sure that the object that will receive this script (the player) 
///// has the Player tag and the Character Controller component.
///// </summary>
//public class ThirdPersonController : MonoBehaviour
//{
//    // Inputs
//    PlayerControls playerControls;
//    Vector2 movementInput;
//    float inputHorizontal;
//    float inputVertical;
//    bool inputJump;
//    bool inputCrouch;
//    bool inputSprint;
//    bool inputAttack;
//    bool inputRoll;
//    bool animMove = false;

//    //NEW REFACTORING
//    [Header("Necessary Components")]
//    [SerializeField] private GameObject weapon; //Will have logic about Attacks and animations and whatever
//    [SerializeField] private Weapon weaponLogic;

//    public int currentAttack = 1;
//    public bool enemyHit;
//    public float attackDistance = 0;
//    public List<Attack> attacks;

//    [SerializeField] private PlayerCameraController camera; //Will have logic to control camera and lock-on
//    [SerializeField] public Animator animator;
//    public CharacterController cc;

//    [Header("State Booleans")]
//    [SerializeField] bool isJumping = false;
//    [SerializeField] bool isSprinting = false;
//    //public bool isAttacking;
//    [SerializeField] bool rolling;
//    [SerializeField] public bool blocking;
//    [SerializeField] public bool isGrounded;

//    [Header("Variables")]
//    [Tooltip("Speed ??at which the character moves. It is not affected by gravity or jumping.")]
//    public float velocity = 5f;
//    [Tooltip("This value is added to the speed value while the character is sprinting.")]
//    public float sprintAdittion = 3.5f;
//    [Tooltip("The higher the value, the higher the character will jump.")]
//    public float jumpForce = 18f;
//    [Tooltip("Stay in the air. The higher the value, the longer the character floats before falling.")]
//    public float jumpTime = 0.85f;
//    [Space]
//    [Tooltip("Force that pulls the player down. Changing this value causes all movement, jumping and falling to be changed as well.")]
//    public float gravity = 9.8f;
//    public float rollSpeed = 0;
//    public float minDistance;

//    public float debugMultiplier;
//    public bool suffering;

//    float jumpElapsedTime = 0;

//    public bool stopGravityCoroutine = false;

//    private PlayerManager manager;


//    private void Awake()
//    {
//        playerControls = new PlayerControls();
//        InitiateInputs();
//        playerControls.Enable();
//        manager = GetComponent<PlayerManager>();
//    }

//    void Start()
//    {
//        cc = GetComponent<CharacterController>();
//        animator = GetComponent<Animator>();

//        // Message informing the user that they forgot to add an animator
//        if (animator == null)
//            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");
//    }


//    // Update is only being used here to identify keys and trigger animations
//    void Update()
//    {
//        if (!suffering)
//        {
//            stopGravityCoroutine = false;
//            if (!rolling && !blocking)
//            {

//                // Run and Crouch animation
//                // If dont have animator component, this block wont run
//                if (isGrounded && animator != null)
//                {

//                    // Run
//                    float minimumSpeed = 0.9f;
//                    //animator.SetBool("run", cc.velocity.magnitude > minimumSpeed);

//                    // Sprint
//                    isSprinting = cc.velocity.magnitude > minimumSpeed && inputSprint;
//                    //animator.SetBool("sprint", isSprinting);

//                }

//                // Jump animation
//                if (animator != null)
//                    animator.SetBool("air", !isGrounded);


//                // Handle can jump or not
//                if (inputJump && isGrounded)
//                {
//                    isJumping = true;
//                    animator.Play("Jump", manager.currentWeapon * 2);
//                    isGrounded = false;
//                    // Disable crounching when jumping
//                    //isCrouching = false; 
//                }

//                if (inputAttack && !weaponLogic.attacking)
//                {
//                    weaponLogic.StopCoroutine("WaitWeaponInput");
//                    stopGravityCoroutine = true;
//                    weapon.SetActive(true);
//                    //animator.SetBool("WeaponOut", true);
//                    jumpElapsedTime = jumpTime;
//                    weaponLogic.StartCoroutine("WaitWeaponInput");
//                }

//                if (inputRoll && isGrounded)
//                {

//                    bool hasSignificantInput = Mathf.Abs(inputHorizontal) > 0.5f || Mathf.Abs(inputVertical) > 0.5f;

//                    if (hasSignificantInput)
//                        animator.Play("Dodge");
//                    else
//                        StartCoroutine("DoBlock");
//                }

//                HeadHittingDetect();
//            }
//            animator.SetBool("grounded", isGrounded);
//        }
//    }

//    #region Input Handlers
//    private void InitiateInputs()
//    {
//        playerControls.PlayerController.PlayerMove.performed += OnPlayerMove;
//        playerControls.PlayerController.PlayerMove.canceled += OnPlayerMove;
//        playerControls.PlayerController.Camera.performed += OnCamera;
//        playerControls.PlayerController.Camera.canceled += OnCamera;
//        playerControls.PlayerController.Jump.performed += OnJump;
//        playerControls.PlayerController.Jump.canceled += OnJump;
//        playerControls.PlayerController.Attack.performed += OnAttack;
//        playerControls.PlayerController.Attack.canceled += OnAttack;
//        playerControls.PlayerController.BlockRoll.performed += OnBlockRoll;
//        playerControls.PlayerController.BlockRoll.canceled += OnBlockRoll;
//        playerControls.PlayerController.SwitchWeapon.performed += OnSwitchWeapon;
//        playerControls.PlayerController.LockOn.performed += OnLockOn;
//        //playerControls.PlayerController.SwitchWeapon.canceled += OnSwitchWeapon;
//    }
//    private void OnPlayerMove(InputAction.CallbackContext context)
//    {
//        inputHorizontal = context.ReadValue<Vector2>().x;
//        inputVertical = context.ReadValue<Vector2>().y;
//    }
//    private void OnJump(InputAction.CallbackContext context)
//    {
//        inputJump = context.ReadValueAsButton();
//    }
//    private void OnAttack(InputAction.CallbackContext context)
//    {
//        inputAttack = context.ReadValueAsButton();
//    }
//    private void OnBlockRoll(InputAction.CallbackContext context)
//    {
//        inputRoll = context.ReadValueAsButton();
//    }

//    private void OnCamera(InputAction.CallbackContext context)
//    {
//        camera.DealWithCamera(context.ReadValue<Vector2>());
//    }

//    private void OnLockOn(InputAction.CallbackContext context)
//    {
//        camera.LockOn();
//    }

//    private void OnSwitchWeapon(InputAction.CallbackContext context)
//    {
//        GetComponent<PlayerManager>().SwitchWeapon();
//    }
//    #endregion


//    // With the inputs and animations defined, FixedUpdate is responsible for applying movements and actions to the player
//    private void FixedUpdate()
//    {
//        if (!suffering)
//        {
//            if (!weaponLogic.attacking && !rolling && !blocking) // Only rotate if not attacking or rolling
//            {

//                if (inputHorizontal != 0f || inputVertical != 0f)
//                {
//                    //animator.SetBool("Moving", true);
//                }
//                else
//                {
//                    //animator.SetBool("Moving", false);
//                }

//                float movementValue = Mathf.Max(Mathf.Abs(inputHorizontal), Mathf.Abs(inputVertical));

//                animator.SetFloat("Movement", Mathf.Abs(movementValue));
//                // Sprinting velocity boost or crounching desacelerate
//                float velocityAdittion = 0;
//                if (isSprinting)
//                    velocityAdittion = sprintAdittion;

//                // Direction movement
//                float directionX = inputHorizontal * (velocity + velocityAdittion) * Time.deltaTime;
//                float directionZ = inputVertical * (velocity + velocityAdittion) * Time.deltaTime;
//                float directionY = 0;

//                // Jump handler
//                if (isJumping)
//                {

//                    // Apply inertia and smoothness when climbing the jump
//                    // It is not necessary when descending, as gravity itself will gradually pulls
//                    directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Time.deltaTime;

//                    // Jump timer
//                    jumpElapsedTime += Time.deltaTime;
//                    if (jumpElapsedTime >= jumpTime)
//                    {
//                        isJumping = false;
//                        jumpElapsedTime = 0;
//                    }
//                }

//                // Add gravity to Y axis
//                directionY = directionY - gravity * Time.deltaTime;

//                Vector3 forward = Camera.main.transform.forward;
//                Vector3 right = Camera.main.transform.right;
//                // --- Character rotation --- 


//                forward.y = 0;
//                right.y = 0;

//                forward.Normalize();
//                right.Normalize();

//                // Relate the front with the Z direction (depth) and right with X (lateral movement)
//                forward = forward * directionZ;
//                right = right * directionX;

//                if (directionX != 0 || directionZ != 0)
//                {
//                    float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
//                    Quaternion rotation = Quaternion.Euler(0, angle, 0);
//                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.5f);
//                }


//                // --- End rotation ---


//                Vector3 verticalDirection = Vector3.up * directionY;
//                Vector3 horizontalDirection = forward + right;

//                Vector3 movement = verticalDirection + horizontalDirection;
//                cc.Move(movement);
//            }
//        }
//    }


//    //This function makes the character end his jump if he hits his head on something
//    void HeadHittingDetect()
//    {
//        float headHitDistance = 1.1f;
//        Vector3 ccCenter = transform.TransformPoint(cc.center);
//        float hitCalc = cc.height / 2f * headHitDistance;

//        // Uncomment this line to see the Ray drawed in your characters head
//        // Debug.DrawRay(ccCenter, Vector3.up * headHeight, Color.red);

//        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc, 0))
//        {
//            jumpElapsedTime = 0;
//            isJumping = false;
//        }
//    }

//    IEnumerator DoRoll()
//    {
//        rolling = true;

//        Vector3 direction = transform.forward;

//        while (rolling)
//        {
//            cc.Move(direction * rollSpeed * Time.deltaTime);
//            yield return null;
//        }
//    }

//    public void StopRoll()
//    {
//        rolling = false;
//    }

//    IEnumerator DoBlock()
//    {
//        blocking = true;
//        animator.Play("Block");
//        yield return null;
//        float time = animator.GetCurrentAnimatorStateInfo(0).length;
//        float elapsedTime = 0f;
//        while (elapsedTime < time * 0.75)
//        {
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//        blocking = false;
//    }


//    public int hitCounter = 0;
//    public void GetHit(Enemy_Base enemy, float damage, AttackType[] types, float force)
//    {
//        StopCoroutine("RecoverFromHit");
//        if (hitCounter >= types.Length)
//        {
//            hitCounter = 0;
//        }

//        //HP = HP - DAMAGE
//        Vector3 direction = Vector3.zero;
//        if (types[hitCounter] == AttackType.Normal)
//        {
//            direction = enemy.transform.forward * force;
//        }
//        else if (types[hitCounter] == AttackType.Finisher)
//        {
//            direction = (enemy.transform.forward + (transform.up / 2)) * force;
//        }
//        else if (types[hitCounter] == AttackType.Thrust)
//        {
//            direction = enemy.transform.forward;
//        }
//        else if (types[hitCounter] == AttackType.Launcher)
//        {
//            direction = transform.up * force;
//        }
//        Vector3 lookDirection = enemy.transform.position - transform.position;
//        lookDirection.y = 0; // Keep the rotation in the horizontal plane
//        transform.rotation = Quaternion.LookRotation(lookDirection);
//        StartCoroutine(RecoverFromHit(direction));
//    }

//    private IEnumerator RecoverFromHit(Vector3 direction)
//    {
//        hitCounter++;
//        suffering = true;
//        animator.Play("Hit");
//        gravity = 0;
//        StartCoroutine(ReturnGravity());
//        float duration = 1f;
//        float elapsedTime = 0f;

//        while (elapsedTime < duration)
//        {
//            float fadeoutFactor = Mathf.Lerp(debugMultiplier, 0f, elapsedTime / duration);

//            Vector3 moveDirection = direction * fadeoutFactor * Time.deltaTime;
//            moveDirection -= new Vector3(0, gravity, 0) * Time.deltaTime;
//            // Apply the direction force with the fadeout factor
//            cc.Move(moveDirection);
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//        suffering = false;
//    }




//    public IEnumerator ReturnGravity()
//    {
//        float elapsedTime = 0f;
//        float transitionDuration = 1f; // Duration of gravity transition
//        float startingGravity = gravity;

//        while (elapsedTime < transitionDuration && !stopGravityCoroutine)
//        {
//            elapsedTime += Time.deltaTime;
//            gravity = Mathf.Lerp(startingGravity, 13f, elapsedTime / transitionDuration);
//            yield return null; // Wait for the next frame
//        }

//        // Ensure the gravity is set to the target value at the end
//    }

//    public void ApplyAnimationForce(float force)
//    {

//        StartCoroutine(ApplyForceOverTime(force));
//    }

//    private IEnumerator ApplyForceOverTime(float force)
//    {
//        weapon.GetComponent<Collider>().enabled = true;
//        animMove = true;
//        Debug.Log("Entrei na corotine do applyforce e o ani");
//        float elapsedTime = 0f;
//        Vector3 forceDirection = transform.forward * force;

//        while (animMove)
//        {
//            if (Vector3.Distance(weaponLogic.currentEnemy.transform.position, transform.position) >= minDistance)
//            {
//                cc.Move(forceDirection * Time.deltaTime);
//            }
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//    }

//    private IEnumerator ApplyAirForceOverTime(float force)
//    {
//        Debug.Log("Entrei na corotine do applyforce no ar");
//        weapon.GetComponent<Collider>().enabled = true;
//        animMove = true;
//        float elapsedTime = 0f;
//        GameObject enemy = GetClosestEnemy();
//        Vector3 directionToLookAt = (enemy.transform.position - transform.position).normalized;
//        Vector3 forceDirection = (transform.forward + new Vector3(0, directionToLookAt.y, 0)) * force;
//        while (animMove)
//        {
//            if (Vector3.Distance(weaponLogic.currentEnemy.transform.position, transform.position) >= minDistance)
//            {
//                cc.Move(forceDirection * Time.deltaTime);
//            }
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//    }

//    public void SetAnimationMove()
//    {
//        weapon.GetComponent<Collider>().enabled = false;
//        animMove = false;
//    }

//    public IEnumerator SetLayerWeightTo0()
//    {
//        float duration = 0.2f;
//        float elapsedTime = 0f;
//        float initialWeight = animator.GetLayerWeight(1);

//        while (elapsedTime < duration)
//        {
//            elapsedTime += Time.deltaTime;
//            float newWeight = Mathf.Lerp(initialWeight, 0f, elapsedTime / duration);
//            animator.SetLayerWeight(1, newWeight);
//            yield return null;
//        }

//        // Ensure the weight is set to 0 at the end
//        animator.SetLayerWeight(1, 0f);
//    }

//    private GameObject GetClosestEnemy()
//    {
//        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
//        GameObject currentEnemy = null;
//        float distance = 0;
//        foreach (var enemy in enemies)
//        {
//            if (currentEnemy == null)
//            {
//                currentEnemy = enemy;

//            }
//            else
//            {
//                if (Vector3.Distance(transform.position, enemy.transform.position) < distance)
//                {
//                    currentEnemy = enemy;
//                }
//            }
//            distance = Vector3.Distance(transform.position, currentEnemy.transform.position);
//        }

//        return currentEnemy;
//    }

//    public void ChangeWeapons(GameObject weapon)
//    {
//        this.weapon = weapon;
//        this.weaponLogic = this.weapon.GetComponent<Weapon>();
//    }



//}