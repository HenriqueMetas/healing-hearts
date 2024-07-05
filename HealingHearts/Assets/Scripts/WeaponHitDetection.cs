using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitDetection : MonoBehaviour
{

    private PlayerManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to an enemy
        if (other.CompareTag("Enemy"))
        {
            manager.DealWithHit(other);
        }
    }
}
