using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.XR.Haptics;

public class EnemyBase : MonoBehaviour
{
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
//    [SerializeField] float actionTime;
//    [SerializeField] float attackDistance;
//    [SerializeField] float rotationSpeed;
//    [SerializeField] bool canBeInterrupted;
//    [SerializeField] int hitsBeforeRevenge;
//    [SerializeField] int currentHits;

//    [Header("ActionVariables")]
//    [SerializeField] public bool inAction = false; //Bool to check if AI is doing an action
//    [SerializeField] bool patrol;
//    [SerializeField] bool follow;
//    [SerializeField] bool attack;
//    [SerializeField] bool jump;
//    [SerializeField] float jumpElapsedTime;
//    [SerializeField] bool shouldLookAtPlayer;
//    [SerializeField] Vector3 patrolDirection;
//    [SerializeField] bool animMove;
//    [SerializeField] bool upMove;

//    [Header("Other components")]
//    [SerializeField] private CharacterController cc;
//    [SerializeField] private Animator animator;
//    [SerializeField] private GameObject player; //Reference to player


//    [Header("DebugStuff")]
//    [SerializeField] public float HP;
//    [SerializeField] bool interrupt;
//    [SerializeField] public bool DebugCanAct;
//    [SerializeField] int attackNumber;
//    [SerializeField] float forcePower;

//    [SerializeField] EnemyState currentState;

//    public enum EnemyState
//    {
//        Patrol,
//        Follow,
//        Attack,
//        Revenge,
//        Break
//    }

//    void Start()
//    {
//        player = GameObject.FindGameObjectWithTag("Player");
//        cc = GetComponent<CharacterController>();
//        animator = GetComponent<Animator>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        //Update is here to identify enemy behaviors and trigger animations
//        if (!inAction && DebugCanAct)
//        {
//            UpdateState();
//        }
//    }

//    public void UpdateState()
//    {
//        TransitionChange();
//        switch (currentState)
//        {
//            case EnemyState.Patrol:
//                StartCoroutine(Patrol());
//                break;
//            case EnemyState.Follow:
//                StartCoroutine(FollowPlayer());
//                break;
//            case EnemyState.Attack:
//                StartCoroutine(Attack());
//                break;
//        }
//    }

//    public void TransitionChange()
//    {
//        if(HP > 70)
//        {
//            // Behaviour 1: Idle or Patrol more often
//            currentState = SelectStateBasedOnProbability(new Dictionary<EnemyState, float> {
//                {EnemyState.Patrol, 35f },
//                {EnemyState.Follow, 35f },
//                {EnemyState.Attack, 30f }
//            });

//        }
//        else if(HP > 35)
//        {
//            currentState = SelectStateBasedOnProbability(new Dictionary<EnemyState, float> {
//                {EnemyState.Patrol, 25f },
//                {EnemyState.Follow, 25f },
//                {EnemyState.Attack, 50f }
//            });
//        }
//        else if(HP > 0)
//        {
//            currentState = SelectStateBasedOnProbability(new Dictionary<EnemyState, float> {
//                {EnemyState.Patrol, 10f },
//                {EnemyState.Follow, 10f },
//                {EnemyState.Attack, 80f }
//            });
//        }
//    }

//    protected EnemyState SelectStateBasedOnProbability(Dictionary<EnemyState, float> probabilities)
//    {
//        float total = 0;
//        foreach (var probability in probabilities.Values)
//        {
//            total += probability;
//        }

//        float randomPoint = Random.value * total;

//        foreach (var kvp in probabilities)
//        {
//            if (randomPoint < kvp.Value)
//            {
//                return kvp.Key;
//            }
//            else
//            {
//                randomPoint -= kvp.Value;
//            }
//        }

//        // In case something goes wrong, return Idle
//        return EnemyState.Patrol;
//    }

//    // FixedUpdate is responsible for applying movements and actions to the enemy
//    private void FixedUpdate()
//    {
//        Vector3 direction = Vector3.zero;
//        if (patrol)
//        {
//            direction = patrolDirection;
//        }
//        else if (follow)
//        {
//            Vector3 targetPosition = player.transform.position;
//            Vector3 directionToPlayer = (targetPosition - transform.position).normalized;
//            directionToPlayer.y = 0;
//            direction = directionToPlayer;
//        }
//        if (jump)
//        {
//            float directionY;
//            directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime);
//            jumpElapsedTime += Time.deltaTime;
//            if (jumpElapsedTime >= jumpTime)
//            {
//                jump = false;
//                jumpElapsedTime = 0;
//            }
//            Vector3 verticalDirection = Vector3.up * directionY;
//            direction += verticalDirection;
//        }
//        if(!inAction && attack)
//        {
//            inAction = true;
//            patrol = false;
//            follow = false;
//            animator.Play("Attack" + attackNumber);
//        }

//        if (interrupt) {
//            StopAllCoroutines();
//            StartCoroutine(Interrupt()); 
//        }
        
//        Move(direction);
//    }

//    private void Move(Vector3 direction)
//    {
//        Vector3 move = direction * velocity * Time.deltaTime;
//        move.y -= gravity * Time.deltaTime;
//        cc.Move(move);
//        transform.LookAt(new Vector3(transform.position.x + direction.x, transform.position.y, transform.position.z + direction.z));
//    }


//    public IEnumerator Patrol()
//    {
//        inAction = true;
//        float randomAngle = Random.Range(0f, 360f);
//        patrolDirection = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)).normalized;
//        patrol = true;
//        animator.Play("Walk");
//        yield return new WaitForSeconds(actionTime);

//        animator.Play("Idle");
//        patrol = false;
//        yield return new WaitForSeconds(0.3f);
//        inAction = false;
        
//    }

//    public IEnumerator FollowPlayer()
//    {
//        inAction = true;
//        follow = true;
//        animator.Play("Walk");
//        yield return new WaitForSeconds(actionTime);
//        follow = false;
//        animator.Play("Idle");
//        yield return new WaitForSeconds(0.5f);
//        inAction = false; 
//    }

//    public IEnumerator Attack()
//    {
//        if(HP > 0)
//        {
//            inAction = true;
//            //Walk towards player while distance to player > attackDistance, when reaches the player, do Random between Attack 1 and 2, if is walking for WalkSeconds, do thrust attack
//            float walkTime = 0f;
//            float walkSeconds = 3.0f; // Time after which thrust attack is performed
//            animator.Play("Run");
//            while (Vector3.Distance(transform.position, player.transform.position) > attackDistance)
//            {
//                // Move towards the player
//                Vector3 direction = (player.transform.position - transform.position).normalized;
//                direction.y = 0; // Keep movement in the horizontal plane
//                cc.Move(direction * (velocity + sprintAdittion) * Time.deltaTime);
//                transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

//                // Update walk time
//                walkTime += Time.deltaTime;

//                if (walkTime >= walkSeconds)
//                {
//                    // Perform thrust attack
//                    inAction = true;
//                    attack = true;
//                    patrol = false;
//                    follow = false;
//                    animator.Play("Attack3");
//                    yield break;
//                }

//                yield return null;
//            }
//            // Perform random attack when close to the player
//            int randomAttack = Random.Range(0, 2);
//            if (randomAttack == 0)
//            {
//                inAction = true;
//                attack = true;
//                patrol = false;
//                follow = false;
//                animator.Play("Attack1");
//            }
//            else
//            {
//                inAction = true;
//                attack = true;
//                patrol = false;
//                follow = false;
//                animator.Play("Attack2");
//            }
//        }
//    }

//    private IEnumerator ApplyEnemyForceOverTime(float force)
//    {
//        //weapon.GetComponent<Collider>().enabled = true;
//        animMove = true;
//        Debug.Log("Entrei na corotine do applyforce e o animan");
//        float elapsedTime = 0f;

//        while (animMove && attack)
//        {
//            RotateTowardsPlayer();
//            // Rotate the enemy to face the player

//            if (Vector3.Distance(transform.position, player.transform.position) >= attackDistance)
//            {
//                // Move the enemy towards the player
                
//                cc.Move(transform.forward * force * Time.deltaTime);
//            }
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//    }

//    private IEnumerator ApplyUpForceOverTime(float force)
//    {
//        Debug.Log("Vou po ar");
//        upMove = true;
//        RotateTowardsPlayer();
//        while (upMove && attack)
//        {
            
//            cc.Move(Vector3.up * force * Time.deltaTime);
//            yield return null;
//        }
        
//    }

//    private void RotateTowardsPlayer()
//    {
//        Vector3 currentPlayerPosition = (player.transform.position - transform.position).normalized;
//        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(currentPlayerPosition.x,0,currentPlayerPosition.z));
//        transform.rotation = targetRotation;
//    }

//    public void SetAnimationMove()
//    {
//        animMove = false;
//    }

//    public void SetUpMove()
//    {
//        upMove = false;
//    }

//    public void DebugStopAction()
//    {
//        inAction = false;
//    }

//    public void DebugStopAttack()
//    {
//        attack = false;
//    }
    
//    public void Gravity(float value)
//    {
//        gravity = value;
//    }

//    IEnumerator Interrupt()
//    {
//        animMove = false;
//        upMove = false;
//        attack = false;
//        patrol = false;
        
//        follow = false;
        
//        interrupt = false;
//        gravity = 0;
//        yield return new WaitForSeconds(1);
//        gravity = 9.8f;
//        inAction = false;
//        currentHits = 0;
        
//    }

//    private IEnumerator ApplyForceOverTime(Vector3 forceDirection, float duration)
//    {
//        float elapsedTime = 0f;

//        while (elapsedTime < duration)
//        {
//            cc.Move(forceDirection * Time.deltaTime);
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//    }

//    public void TakeHit(Vector3 forcedirection)
//    {
//        if (canBeInterrupted)
//        {
//            currentHits++;
//            StopAllCoroutines();
//            if (currentHits == hitsBeforeRevenge) {
//                inAction = true;
//                patrol = false;
//                follow = false;
//                attack = true;
//                animator.Play("RevengeValue");
//            }
//            else
//            {
//                StartCoroutine(Interrupt());
//                StartCoroutine(ApplyForceOverTime(forcedirection * forcePower, 0.5f)); //This forcepower needs to come from the attack
//                animator.Play("Hit1"); //Hit number should be based on forcepower
//            }
//        }
//    }
//    //########################################################################################################################
//    [Header("Values")]
   
    
   
//    [SerializeField] float maxHP;
//    [SerializeField] float currentHP;
//    [SerializeField] bool canBeDamaged;
//    [SerializeField] float KnockbackDuration;
//    [SerializeField] float knockbackTime;
   
 

    

   


   

   

//    //public void TakeDamage(float damage, Vector3 direction, float force)
//    //{
//    //    if (canBeDamaged)
//    //    {
//    //        currentHits++;
//    //        if (currentHits < hitsBeforeRevenge)
//    //        {
//    //            StopAllCoroutines();
//    //            currentHP -= damage;
//    //            animator.Play("Hit1");
//    //            StartCoroutine(ApplyKnockback(direction, force)); 
//    //        }
//    //        else
//    //        {
//    //            StopAllCoroutines();
//    //            inAction = true;
//    //            canBeDamaged = false;
//    //            animator.Play("RevengeValue");
//    //        }
//    //    }
//    //}

//    //IEnumerator ApplyKnockback(Vector3 direction, float force)
//    //{
//    //    Vector3 knockbackDirection = direction * force;
//    //    knockbackTime = KnockbackDuration;
//    //    while (knockbackTime > 0)
//    //    {
//    //        knockbackTime -= Time.deltaTime;
//    //        rb.MovePosition(transform.position + knockbackDirection * Time.deltaTime);
//    //        yield return null;
//    //    }
//    //    inAction = false;
//    //    currentHits = 0;

//    //    //RESET MAN
//    //}

//    //public void Jump(int direction)
//    //{
//    //    Vector3 dir = Vector3.zero;
//    //    if(direction == 0)
//    //    {
//    //        dir = transform.up;
//    //    }
//    //    if (direction == 1)
//    //    {
//    //        dir = -transform.up;
//    //    }
//    //    Vector3 forceDirection = dir * 10;
//    //    rb.AddForce(forceDirection, ForceMode.Impulse);
//    //}
   

//    //private void setAnimMove()
//    //{
//    //    animMove = false;
//    //}
}
