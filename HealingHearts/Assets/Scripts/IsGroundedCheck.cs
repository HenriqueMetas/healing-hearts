using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGroundedCheck : MonoBehaviour
{
    [SerializeField] Player_Controller controller;

    [SerializeField] Transform boxCenter;
    [SerializeField] float x=1,y=1,z=1;
    

    private void Start()
    {
        controller = GetComponentInParent<Player_Controller>();
        boxCenter = this.transform;
    }

    private void Update()
    {
        if (!controller.isGrounded)
        {
            Collider[] hitColliders = Physics.OverlapBox(boxCenter.position, new Vector3(x,y,z));
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Ground"))
                {
                    controller.isGrounded = true;
                    Debug.Log("I'm grounded now.");
                }
            }
        }
        else
        {
            Collider[] hitColliders = Physics.OverlapBox(boxCenter.position, new Vector3(x, y, z));
            bool foundGround = false;
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Ground"))
                {
                    foundGround = true;
                    break;
                }
            }
            controller.isGrounded = foundGround;
            
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(boxCenter.position, new Vector3(x, y, z));
    }


}
