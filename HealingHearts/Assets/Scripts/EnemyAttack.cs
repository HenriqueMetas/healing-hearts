using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Enemy_Base thisEnemy;
    public Collider EnemyCollider;
    public GameObject hitAnimation;
    public Attack_Base currentAttack;

    // Start is called before the first frame update
    void Start()
    {
        thisEnemy = GetComponentInParent<Enemy_Base>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to an enemy
        if (other.CompareTag("Player"))
        {
            if (!other.gameObject.GetComponent<Player_Controller>().blocking)
            {
                Instantiate(hitAnimation, other.transform);

                other.gameObject.GetComponent<Player_Controller>().GetHit(thisEnemy, currentAttack.damage[thisEnemy.currentStrike], currentAttack.types[thisEnemy.currentStrike], currentAttack.force[thisEnemy.currentStrike]);
            }
            else
            {
                //Block attack thing
            }
        }
    }

    public void SetCurrentAttack(Attack_Base attack)
    {
        currentAttack = attack;
    }
}
