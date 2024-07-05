using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterLove.StateMachine;
using UnityEditor.Experimental.GraphView;
using TMPro;
using Unity.VisualScripting;
using System;
using static UnityEngine.EventSystems.EventTrigger;
using static Player_Controller;
using System.Reflection;

public class Enemy_Base : MonoBehaviour
{
    [Header("DEBUG")]
    public EnemyStates currentstate;
    [SerializeField] private List<Coroutine> runningCoroutines = new List<Coroutine>();

    [Header("GameObject and Component References")]
    [SerializeField] private CharacterController cc;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject player;

    [Header("Enemy Variables")]
    [SerializeField] public float maxHP;
    [SerializeField] public float currentHP;
    [SerializeField] public bool canBeHit;

    [Header("Movement Variables")]
    [SerializeField] public float velocity = 2f;
    [SerializeField] public float sprintAdittion = 3.5f;
    [SerializeField] public float gravity = 9.8f;
    [SerializeField] public float attackRange = 2f;
    [SerializeField] public bool animMove = false;
    [SerializeField] public bool upMove = false;

    public enum EnemyStates
    {
        Init,
        Idle,
        Follow,
        Attack,
        Break,
        Dead
    }


    StateMachine<EnemyStates,StateDriverUnity> sm;

    #region Global Methods

    public void TriggerChangeState(EnemyStates state)
    {
        sm.ChangeState(state);
    }

    private void Awake()
    {
        sm = new StateMachine<EnemyStates,StateDriverUnity>(this);
        sm.ChangeState(EnemyStates.Init);
    }




    private void FixedUpdate()
    {
        sm.Driver.FixedUpdate.Invoke();
    }
    private void Move(Vector3 direction)
    {
        Vector3 move = direction * velocity * Time.deltaTime;
        move.y -= gravity * Time.deltaTime;
        cc.Move(move);
    }

    public void RotateToPlayer()
    {
        Vector3 currentPlayerPosition = (player.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(currentPlayerPosition.x, 0, currentPlayerPosition.z));
        transform.rotation = targetRotation;
    }

    public IEnumerator ApplyEnemyForceOverTime(float force)
    {
        //weapon.GetComponent<Collider>().enabled = true;
        animMove = true;
        Debug.Log("Entrei na corotine do applyforce e o animan");
        float elapsedTime = 0f;

        while (animMove)
        {
            RotateToPlayer();
            // Rotate the enemy to face the player
            Vector3 enemy = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 playerman = new Vector3(player.transform.position.x, 0, player.transform.position.z);
            if (Vector3.Distance(enemy, playerman) >= attackRange)
            {
                // Move the enemy towards the player

                cc.Move(transform.forward * force * Time.deltaTime);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void SetAnimationMove()
    {
        animMove = false;
    }

    public IEnumerator ApplyUpForceOverTime(float force)
    {
        upMove = true;
        RotateToPlayer();
        while (upMove)
        {

            cc.Move(Vector3.up * force * Time.deltaTime);
            yield return null;
        }
    }

    public void Gravity(float value)
    {
        gravity = value;
    }

    public void SetUpMove()
    {
        upMove = false;
    }

    public IEnumerator ReturnGravity()
    {
        float elapsedTime = 0f;
        float transitionDuration = 1f; // Duration of gravity transition
        float startingGravity = gravity;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            gravity = Mathf.Lerp(startingGravity, 9.8f, elapsedTime / transitionDuration);
            yield return null; // Wait for the next frame
        }

        // Ensure the gravity is set to the target value at the end
    }


    #endregion

    #region Init State
    public bool DEBUGCANACT;
    void Init_Enter() //Equivale ao Start do state
    {
        Debug.Log("Entrei no Init da SM");
        currentHP = maxHP;
        currentstate = EnemyStates.Init;
        player = GameObject.FindGameObjectWithTag("Player");
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (DEBUGCANACT)
        {
            sm.ChangeState(EnemyStates.Idle);
        }
        //Play an initialize animation or whatever
    }

    #endregion

    #region IDLE STATE -------

    [Header("IdleState Variables")]
    [SerializeField] private float minimumTime = 8f;
    [SerializeField] private float maximumTime = 10f;
    [SerializeField] private bool moveRight;

    //This is the base state for the enemy to choose what he does, it has a base method that makes him walk 
    void Idle_Enter()
    {
        Debug.Log("Entrei no Idle da SM");
        currentstate = EnemyStates.Idle;
        moveRight = UnityEngine.Random.Range(0, 2) == 0;
        StartCoroutine(StayInIdleState(UnityEngine.Random.Range(minimumTime, maximumTime)));
        if (moveRight)
        {
            animator.Play("RightWalk");
        }
        else
        {
            animator.Play("LeftWalk");
        }
    }

    void Idle_FixedUpdate()
    {
        RotateToPlayer();
        Vector3 direction;
        if (moveRight) direction = transform.right;
        else direction = -transform.right;
        Move(direction);
    }

    IEnumerator StayInIdleState(float time)
    {
        Debug.Log("Vou ficar no idle durante " + time + "segundos");
        yield return new WaitForSeconds(time);

        IdlePickPossibleState();
    }

    void IdlePickPossibleState()
    {
        EnemyStates[] possibleStates = new EnemyStates[]
        {
            //EnemyStates.Idle,
            //EnemyStates.Follow,
            EnemyStates.Attack
        };
        //Pick one of the possibleStates at random
        int randomIndex = UnityEngine.Random.Range(0, possibleStates.Length);
        EnemyStates pickedState = possibleStates[randomIndex];

        // Set the current state to the picked state
        if (pickedState == EnemyStates.Idle)
        {
            Idle_Enter();
        }
        else
        {
            sm.ChangeState(pickedState);
        }
    }

    #endregion

    #region Follow State
    [Header("IdleState Variables")]
    [SerializeField] private float followMinimumTime = 8f;
    [SerializeField] private float followMaximumTime = 10f;

    void Follow_Enter()
    {
        Debug.Log("Entrei no Follow da SM");
        currentstate = EnemyStates.Follow;
        animator.Play("Walk");
        StartCoroutine(StayInFollowState(UnityEngine.Random.Range(followMinimumTime, followMaximumTime)));
    }

    //In this state the enemy follows the player
    void Follow_FixedUpdate()
    {
        RotateToPlayer();
        Vector3 direction = transform.forward;
        Move(direction);
    }

    IEnumerator StayInFollowState(float time)
    {
        Debug.Log("Vou ficar no follow durante " + time + "segundos");

        yield return new WaitForSeconds(time);

        FollowPickPossibleState();
    }

    void FollowPickPossibleState()
    {
        EnemyStates[] possibleStates = new EnemyStates[]
        {
            //EnemyStates.Idle,
            //EnemyStates.Follow,
            EnemyStates.Attack
        };
        //Pick one of the possibleStates at random
        int randomIndex = UnityEngine.Random.Range(0, possibleStates.Length);
        EnemyStates pickedState = possibleStates[randomIndex];

        if (pickedState == EnemyStates.Follow)
        {
            Follow_Enter();
        }
        // Set the current state to the picked state
        else
        {
            sm.ChangeState(pickedState);
        }
    }

    #endregion

    #region Attack State

    [Header("AttackState Variables")]
    [SerializeField] Attack_Base[] attacks;
    [SerializeField] Attack_Base thrustAttack;
    [SerializeField] Attack_Base revengeAttack;
    [SerializeField] int numberOfAttacks;
    [SerializeField] bool attacking;
    [SerializeField] float minimumCloseTime;
    [SerializeField] float maximumCloseTime;
    [SerializeField] EnemyAttack enemyWeapon;

    [SerializeField] float currentAttackStrikes;
    [SerializeField] public int currentStrike;


    void Attack_Enter()
    {
        Debug.Log("Entrei no Attack da SM");
        currentstate = EnemyStates.Attack;
        currentStrike = 0;
        if (enemyWeapon == null)
        {
            enemyWeapon = GetComponentInChildren<EnemyAttack>();
        }

        if (attacksTaken == revengeValue)
        {
            Debug.Log("Vou fazer revenge attack");
            enemyWeapon.SetCurrentAttack(revengeAttack);
            animator.Play(revengeAttack.animationName);
            attacksTaken = 0;
            attacking = true;
        }
        else
        {
            Attack_Base attack = attacks[UnityEngine.Random.Range(0, numberOfAttacks)];
            float followtime = UnityEngine.Random.Range(minimumCloseTime, maximumCloseTime);
            StartCoroutine(DoAttack(attack,
                                    followtime));
        }

        attacksTaken = 0;
    }

    IEnumerator DoAttack(Attack_Base attack,
                         float followtime)
    {
        
        float walkTime = 0f;
        animator.Play("Run");
        Vector2 enemyposition = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerposition = new Vector2(player.transform.position.x, player.transform.position.z);
        while (Vector2.Distance(enemyposition, playerposition) > attackRange)
        {
            // Move towards the player
            Vector3 direction = (player.transform.position - transform.position).normalized;
            direction.y -= gravity * Time.deltaTime; // Keep movement in the horizontal plane
            cc.Move(direction * (velocity + sprintAdittion) * Time.deltaTime);
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

            // Update walk time
            walkTime += Time.deltaTime;

            if (walkTime >= followtime)
            {
                enemyWeapon.SetCurrentAttack(thrustAttack);
                animator.Play(thrustAttack.animationName);
                attacking = true;
                yield break;
            }
            enemyposition = new Vector2(transform.position.x, transform.position.z);
            playerposition = new Vector2(player.transform.position.x, player.transform.position.z);
            yield return null;
        }

        enemyWeapon.SetCurrentAttack(attack);
        Debug.Log("Vou fazer o ataque: " + attack.animationName);
        animator.Play(attack.animationName);
        attacking = true;
    }

    public void IterateStrike()
    {
        currentStrike++;
    }

    void AttackPickPossibleState()
    {
        attacking = false;
        EnemyStates[] possibleStates = new EnemyStates[]
       {
            EnemyStates.Idle,
            EnemyStates.Follow,
            EnemyStates.Attack
       };
        //Pick one of the possibleStates at random
        int randomIndex = UnityEngine.Random.Range(0, possibleStates.Length);
        EnemyStates pickedState = possibleStates[randomIndex];

        if (pickedState == EnemyStates.Attack)
        {
            Attack_Enter();
        }
        else
        {
            // Set the current state to the picked state
            sm.ChangeState(pickedState);
        }
    }

    #endregion

    #region Break State

    [Header("BreakState Variables")] 
    //[SerializeField] public Attack_Base breakAttack;
    [SerializeField] public int attacksTaken;
    [SerializeField] public int revengeValue;


    public void GetHit(Attack_Base attack, int index, Transform playertransform)
    {
        if (canBeHit && !isDead)
        {
            Interrupt(); //1º Para coroutines e entra no Break
            DealWithHit(attack, index, playertransform);
        }
    }

    public void Interrupt()
    {
        if (!isDead)
        {
            Debug.Log("OH NO FUI INTERROMPIDO");
            StopAllCoroutines();
            if (currentstate != EnemyStates.Break)
            {
                sm.ChangeState(EnemyStates.Break);
            }
            else
            {
                Break_Enter();
            }
        }
    }

    void Break_Enter()
    {
        SetBoolsToFalse();
        Debug.Log("Entrei no Break da SM");
        currentstate = EnemyStates.Break;
        gravity = 9.8f;

    }

    public void DealWithHit(Attack_Base attack, int index, Transform playertransform)
    {
        currentHP -= attack.damage[index];
        if (currentHP <= 0)
        {
            animator.SetBool("Dead", true);
            isDead = true;
        }
        else
        {
            attacksTaken++;
        }

        if (attacksTaken != revengeValue)
        {
            Vector3 direction = Vector3.zero;

            if (attack.types[index] == Attack_Base.AttackType.Normal)
            {
                direction = playertransform.forward * attack.force[index];
                animator.Play("Hit1", -1, 0);
            }
            else if (attack.types[index] == Attack_Base.AttackType.Finisher)
            {
                direction = (playertransform.forward + (playertransform.up / 2)) * attack.force[index];
                animator.Play("Hit2", -1, 0);
            }
            else if (attack.types[index] == Attack_Base.AttackType.Thrust)
            {
                direction = playertransform.forward * attack.force[index];
                animator.Play("Hit3", -1, 0);
            }
            else if (attack.types[index] == Attack_Base.AttackType.Launcher)
            {
                direction = transform.up * attack.force[index];
                animator.Play("Hit4", -1, 0);
            }

            Vector3 lookDirection = playertransform.position - transform.position;
            lookDirection.y = 0; // Keep the rotation in the horizontal plane
            transform.rotation = Quaternion.LookRotation(lookDirection);
            StartCoroutine(RecoverFromHit(direction));
        }
        else
        {
            if (!isDead)
                sm.ChangeState(EnemyStates.Attack);
        }
    }

    public float debugMultiplier = 1f;
    private IEnumerator RecoverFromHit(Vector3 direction)
    {
        Debug.Log("Vou recuperar do hit");
        gravity = 0;
        StartCoroutine(ReturnGravity());
        float duration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float fadeoutFactor = Mathf.Lerp(debugMultiplier, 0f, elapsedTime / duration);

            Vector3 moveDirection = direction * fadeoutFactor * Time.deltaTime;
            moveDirection -= new Vector3(0, gravity, 0) * Time.deltaTime;
            // Apply the direction force with the fadeout factor
            cc.Move(moveDirection);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        BreakPickPossibleState();
    }

    void BreakPickPossibleState()
    {
        if (currentHP > 0)
        {
            EnemyStates[] possibleStates = new EnemyStates[]
            {
            EnemyStates.Idle,
            EnemyStates.Follow,
            EnemyStates.Attack
            };
            //Pick one of the possibleStates at random
            int randomIndex = UnityEngine.Random.Range(0, possibleStates.Length);
            EnemyStates pickedState = possibleStates[randomIndex];

            // Set the current state to the picked state
            sm.ChangeState(pickedState);
        }
        else
        {
            sm.ChangeState(EnemyStates.Dead);
        }
    }

    public void setCanBeHit()
    {
        canBeHit = true;
    }

    public void unsetCanBeHit()
    {
        canBeHit = false;
    }
    

    #endregion

    #region Dead State
    public bool isDead = false;
    void Dead_Enter()
    {
        currentstate = EnemyStates.Dead;
        isDead = true;
        cc.enabled = false;
    }

    #endregion

    private void SetBoolsToFalse()
    {
        animMove = false;
        upMove = false;
        attacking = false;
    }
}
