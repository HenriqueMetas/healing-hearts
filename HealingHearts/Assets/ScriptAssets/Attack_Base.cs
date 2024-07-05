using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Attack_Base")]
public class Attack_Base : ScriptableObject
{
    public string animationName;
    public float[] damage;
    public float[] force;
    public AttackType[] types;
    public GameObject HitEffect;

    public enum AttackType
    {
        Normal,
        Finisher,
        Thrust,
        Launcher
    }
}
