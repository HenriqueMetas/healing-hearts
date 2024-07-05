using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Outside Components")]
    [SerializeField] public Player_Controller controller;
    [SerializeField] public PlayerManager manager;
    [SerializeField] private Animator playerAnimator;
    [Header("Weapon Components")]
    [SerializeField] public Collider swordCollision;
    [SerializeField] public GameObject hitAnimation;

    [Header("List of Attacks")]
    [SerializeField] public List<Attack_Base> groundAttacks;
    [SerializeField] public List<Attack_Base> airAttacks;

    [Header("Logic")]
    [SerializeField] public int currentAttack = 0;
    [SerializeField] public Attack_Base ActualCurrentAttack;
    [SerializeField] public bool enemyHit;
    [SerializeField] public bool attacking;
    [SerializeField] public float maxDistanceToFocus = 10f;
    [SerializeField] public int currentStrike = 0;

    public GameObject currentEnemy;
    


    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<Player_Controller>();
        manager = FindObjectOfType<PlayerManager>();
        playerAnimator = controller.GetComponent<Animator>();
        swordCollision.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator WaitWeaponInput()
    {
        StartCoroutine(DoCurrentAttack());
        yield return new WaitForSeconds(5f);
        controller.animator.SetBool("WeaponOut", false);
        this.gameObject.SetActive(false);
    }

    private IEnumerator DoCurrentAttack()
    {
        currentStrike = 0;
        //TurnToEnemyLogic
        GameObject closestEnemy = GetClosestEnemy();
        Vector3 directionToLookAt = (closestEnemy.transform.position - controller.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToLookAt.x, 0, directionToLookAt.z));
        Vector3 initialPos = controller.transform.position;
        float heightDifference = GetClosestEnemy().transform.position.y - transform.position.y;
        float distanceToEnemy = Vector3.Distance(transform.position, closestEnemy.transform.position);

        if (currentAttack == 0 && distanceToEnemy > 6f && distanceToEnemy < maxDistanceToFocus)
        {
            currentAttack = 1;
        }

        float attackDistance = 0;

        //Choose Attack
        if (heightDifference > 2f || !controller.isGrounded) //Air Attack
        {
            controller.StopCoroutine("SetLayerWeightTo0");
            controller.gravity = 0f;
            //attackDistance = airAttacks[currentAttack].maxDistance;

            if (airAttacks[currentAttack].HitEffect != null)
            {
                Instantiate(airAttacks[currentAttack].HitEffect, controller.transform);
            }

            playerAnimator.SetLayerWeight(1, 1);
            playerAnimator.Play(airAttacks[currentAttack].animationName,1);
            ActualCurrentAttack = airAttacks[currentAttack];
        }
        else //Ground Attack
        {
            if (groundAttacks[currentAttack].HitEffect != null)
            {
                Instantiate(groundAttacks[currentAttack].HitEffect, controller.transform);
            }
            playerAnimator.Play(groundAttacks[currentAttack].animationName, manager.currentWeapon*2);
            ActualCurrentAttack = groundAttacks[currentAttack];
            //attackDistance = groundAttacks[currentAttack].maxDistance;
        }

        attacking = true;

        //Deal with Variables
        swordCollision.enabled = true;

        //Start Animation Logic
        yield return null;
        float time = playerAnimator.GetCurrentAnimatorStateInfo(0).length;
        float elapsedTime = 0f;
        Vector3 direction;
        if (distanceToEnemy < maxDistanceToFocus)
        {
            direction = (closestEnemy.transform.position - controller.transform.position).normalized;
            controller.transform.rotation = lookRotation;

        } else
        {
            direction = controller.transform.forward.normalized;
        }
        currentAttack++;
        if (currentAttack > 3) currentAttack = 0;
        
        float speed = attackDistance / (time * 0.75f);
        while (elapsedTime < time * 0.75)
        {
            if (Vector3.Distance(transform.position, initialPos) < attackDistance && !enemyHit && Vector3.Distance(transform.position, closestEnemy.transform.position) > 1f)
            {
                controller.cc.Move(direction * 10 * Time.deltaTime);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        controller.StartReturnGravity();

        enemyHit = false;
    }

    private GameObject GetClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        currentEnemy = null;
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

    private void OnTriggerEnter(Collider other)
    {
        //// Check if the collider belongs to an enemy
        //if (other.CompareTag("Enemy"))
        //{
        //    Instantiate(hitAnimation, other.transform);
        //    enemyHit = true;
        //    Rigidbody enemyRigidbody = other.GetComponentInParent<Rigidbody>();
        //    if (enemyRigidbody != null)
        //    {
        //        // Calculate the direction to apply the force
        //        Vector3 forceDirection = GameObject.FindGameObjectWithTag("Player").transform.forward;
        //        //forceDirection.y = 0; // Keep the force horizontal

        //        Enemy enemy = other.GetComponentInParent<Enemy>();
        //        // Apply the force to the enemy's Rigidbody
        //        enemy.ApplyKnockback(forceDirection, 1f, false);
        //    }
        //}
    }

    public void DoHit(Collider other)
    {
        Instantiate(hitAnimation, other.transform);
        enemyHit = true;
        Rigidbody enemyRigidbody = other.GetComponentInParent<Rigidbody>();
        CharacterController enemyController = other.GetComponent<CharacterController>();
        if (enemyController != null)
        {
            // Calculate the direction to apply the force
            Transform playerPlace = GameObject.FindGameObjectWithTag("Player").transform;
            //forceDirection.y = 0; // Keep the force horizontal

            //Enemy enemy = other.GetComponentInParent<Enemy>();
            // Apply the force to the enemy's Rigidbody
            //enemy.ApplyKnockback(forceDirection, 1f, false);

            Enemy_Base enemy = other.GetComponent<Enemy_Base>();

            enemy.GetHit(ActualCurrentAttack, currentStrike, playerPlace);
        }
    }

    
}
