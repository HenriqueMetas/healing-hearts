using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

public class ObstaclePush : MonoBehaviour
{
    [SerializeField]
    public Transform sphereCenter;
    public Transform boxCenter;
    public float pushDuration = 0.2f;
    public Collider[] hitColliders;


    public float pushForce = 3f;  // Adjust this value for the push strength
    public float pushRadius = 2f; // Radius to check for enemies
    public LayerMask enemyLayer;  // Layer mask to identify enemies

    [SerializeField] float x = 1, y = 1, z = 1;

    private CharacterController characterController;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        PushNearbyEnemies();
    }

    private void PushNearbyEnemies()
    {
        //hitColliders = Physics.OverlapSphere(sphereCenter.position, pushRadius);
        hitColliders = Physics.OverlapBox(boxCenter.position, new Vector3(x, y, z), transform.rotation);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                Rigidbody rb = hitCollider.attachedRigidbody;

                CharacterController enemyController = hitCollider.GetComponent<CharacterController>();


                if (rb != null)
                {
                       
                        Vector3 pushDirection = hitCollider.gameObject.transform.position - transform.position;
                        pushDirection.y = 0;
                        pushDirection.Normalize();

                        rb.AddForceAtPosition(pushDirection * pushForce, transform.position, ForceMode.Impulse);
                        //Vector3 newEnemyPosition = hitCollider.transform.position + pushDirection * pushForce * Time.deltaTime;
                        //hitCollider.transform.position = newEnemyPosition;

                }
                
                else if (enemyController != null)
                {

                    Vector3 pushDirection = hitCollider.transform.position - transform.position;
                    pushDirection.y = 0;
                    pushDirection.Normalize();
                    StartCoroutine(SmoothPush(enemyController, pushDirection));
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(sphereCenter.position, pushRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(boxCenter.position, new Vector3(x, y, z));
    }


    private IEnumerator PushEnemy(GameObject enemy, Vector3 pushDirection)
    {
        float elapsedTime = 0f;

        while (elapsedTime < pushDuration)
        {
            if (enemy != null)
            {
                // Apply the push force to the enemy's position
                enemy.transform.position += pushDirection * pushForce * Time.deltaTime;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator SmoothPush(CharacterController enemyController, Vector3 pushDirection)
    {
        float elapsedTime = 0f;

        while (elapsedTime < pushDuration)
        {
            Vector3 newEnemyPosition = pushDirection * pushForce * Time.deltaTime;
            enemyController.Move(newEnemyPosition);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
