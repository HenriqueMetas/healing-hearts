using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public Transform playerCharacter;
    public float speed = 5f;
    public float distanceToAttack = 2f;
    public Rigidbody rb;
    private Vector3 knockbackDirection;
    private float knockbackTime;
    private float knockbackDuration = 0.5f; // Duration of the knockback effect
    private float maxDistance = 10f;
    public bool isAttacking = false;
    private bool isDizzy = false;

    public bool debugCanAttack = false;

    public Animator enemyAnimator;

    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        playerCharacter = GameObject.FindGameObjectWithTag("Player").transform;
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing on the enemy.");
        }
    }

    void Update()
    {
        if (!isAttacking && !isDizzy)
        {
            FollowPlayer();
        }
        if (knockbackTime > 0)
        {
            knockbackTime -= Time.deltaTime;
            rb.MovePosition(transform.position + knockbackDirection * speed * Time.deltaTime);
        }
    }

    void FollowPlayer()
    {
        if (debugCanAttack)
        {
            if (Vector3.Distance(transform.position, playerCharacter.position) < distanceToAttack)
            {
                enemyAnimator.SetBool("Moving", false);
                StartCoroutine("DoAttack");
            }
            else if (Vector3.Distance(transform.position, playerCharacter.position) < maxDistance)
            {
                enemyAnimator.SetBool("Moving", true);
                Vector3 direction = (playerCharacter.position - transform.position).normalized;
                rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
            else
            {
                enemyAnimator.SetBool("Moving", false);
            }
        }
    }

    public void ApplyKnockback(Vector3 direction, float force, bool wasBlocked)
    {
        if (!isAttacking || wasBlocked)
        {
            enemyAnimator.Play("GetHit");
            knockbackDirection = direction * force;
            knockbackTime = knockbackDuration;
        }
    }

    IEnumerator DoAttack()
    {
        isAttacking = true;
        enemyAnimator.Play("IddleBattle");
        yield return new WaitForSeconds(0.2f);
        enemyAnimator.Play("Attack02");
        yield return null;
        yield return new WaitForSeconds(enemyAnimator.GetCurrentAnimatorClipInfo(0).Length);
        RecoverFromAttack();
    }

    public void RecoverFromAttack()
    {
        StartCoroutine("RecoverFromAttackCoroutine");
    }

    IEnumerator RecoverFromAttackCoroutine()
    {
        isAttacking = false;
        isDizzy = true;
        enemyAnimator.SetBool("isDizzy", true);
        yield return new WaitForSeconds(2f);
        isDizzy = false;
        enemyAnimator.SetBool("isDizzy", false);
    }
}
