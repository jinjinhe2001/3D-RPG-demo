using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewData",menuName ="Character Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;

    public int currHealth;

    public int baseDefence;

    public int currDefence;

    [Tooltip("受击僵直时间")]
    public float getHitWaitTime;

    [Header("Kill")]
    public int killPoint;

    [Header("Level")]
    public int currLevel;

    public int maxLevel;

    public int baseExp;

    public int currExp;

    public float levelBuff;

    public float LevelMultiplier
    {
        get
        {
            return 1 + (currLevel - 1) * levelBuff;
        }
    }

    public void UpdateExp(int point)
    {
        currExp += point;
        if (currExp >= baseExp)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// 升级
    /// </summary>
    private void LevelUp()
    {
        currLevel = Mathf.Clamp(currLevel + 1, 0, maxLevel);
        baseExp += (int)(baseExp * LevelMultiplier);
        maxHealth = (int)(maxHealth * LevelMultiplier);
        currHealth = maxHealth;

        Debug.Log("LevelUp" + currLevel + "Health:" + currHealth);
    }
}
