using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewData", menuName = "Attack/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    [Tooltip("��������")]
    public float attackRange;
    [Tooltip("Զ�̾���")]
    public float skillRange;
    [Tooltip("��������(�����ж��Ƿ����)")]
    public float WeaponRange;

    public float coolDown;

    public int minDamage;
    public int maxDamage;

    [Tooltip("�����˺��ٷֱ�")]
    public float criticalMultiplier;
    [Tooltip("������")]
    public float criticalChance;
}
