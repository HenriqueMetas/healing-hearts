using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    [Header("HP Stuff")]
    [SerializeField] public float maxHP;
    [SerializeField] public float currentHP;
    [Header("Weapon Stuff")]
    [SerializeField] GameObject[] weapons;
    [SerializeField] Collider[] weaponColliders;
    [SerializeField] public int currentWeapon = 0;


    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        GetComponent<Player_Controller>().ChangeWeapons(weapons[currentWeapon]);
        currentHP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchWeapon()
    {
        if (!weapons[currentWeapon].GetComponent<Weapon>().attacking)
        {
            int weapontochange = currentWeapon + 1;
            if (weapontochange == weapons.Length)
            {
                Debug.Log("Vou trocar o weapontochange para 0");
                weapontochange = 0;
            }
            Debug.Log("Current: " + currentWeapon + " | ToChange: " + weapontochange);
            weapons[currentWeapon].SetActive(false);
            weaponColliders[currentWeapon].gameObject.SetActive(false);
            weapons[weapontochange].SetActive(true);
            weaponColliders[weapontochange].gameObject.SetActive(true);
            //In case of weapon = 0 -> All layers to 0
            if (weapontochange == 0)
            {
                int layers = anim.layerCount;
                for (int i = 1; i < layers; i++)
                {
                    anim.SetLayerWeight(i, 0);
                }

            }
            //In case of Shield
            else if (weapontochange == 1)
            {
                anim.SetLayerWeight(weapontochange * 2, 1);

            }
            currentWeapon = weapontochange;
            GetComponent<Player_Controller>().ChangeWeapons(weapons[currentWeapon]);
        }
    }

    public void ReduceHP(float hp)
    {
        currentHP -= hp;
    }

    public void SetAttacking()
    {
        weapons[currentWeapon].GetComponent<Weapon>().attacking = false;
    }

    public void SwitchWeaponCollider()
    {
        weaponColliders[currentWeapon].enabled = !weaponColliders[currentWeapon].enabled;
    }

    public void DealWithHit(Collider col)
    {
        weapons[currentWeapon].GetComponent<Weapon>().DoHit(col);
    }

    public void IterateWeaponStrike()
    {
        weapons[currentWeapon].GetComponent<Weapon>().currentStrike++;
    }

    public void DisableColliders()
    {
        foreach(var collider in weaponColliders)
        {
            if (collider.enabled) collider.enabled = false;
        }
    }
}
