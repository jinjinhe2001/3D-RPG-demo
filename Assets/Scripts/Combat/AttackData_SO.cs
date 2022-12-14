using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewData", menuName = "Attack/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    [Tooltip("¹¥»÷¾àÀë")]
    public float attackRange;
    [Tooltip("Ô¶³Ì¾àÀë")]
    public float skillRange;
    [Tooltip("ÎäÆ÷¾àÀë(ÓÃÒÔÅĞ¶ÏÊÇ·ñ»÷ÖĞ)")]
    public float WeaponRange;

    public float coolDown;

    public int minDamage;
    public int maxDamage;

    [Tooltip("±©»÷ÉËº¦°Ù·Ö±È")]
    public float criticalMultiplier;
    [Tooltip("±©»÷ÂÊ")]
    public float criticalChance;
}
