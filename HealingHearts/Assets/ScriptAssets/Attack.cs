using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/PlayerAttack")]
public class Attack : ScriptableObject
{
    public string animationName;
    public float damage;
    public float maxDistance;
    public GameObject HitEffect;
}
