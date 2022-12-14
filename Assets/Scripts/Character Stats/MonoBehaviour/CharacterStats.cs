using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack;

    [Tooltip("模板数据")]
    public CharacterData_SO templateData;

    [Tooltip("角色数据")]
    public CharacterData_SO characterData;

    public AttackData_SO attackData;

    [HideInInspector]
    public bool isCritical;

    public bool getHit = false;

    private void Awake()
    {
        if(templateData!=null)
        {
            characterData = Instantiate(templateData);
        }
    }

    #region Read from Data_SO
    public int MaxHealth
    {
        get
        {
            if (characterData != null)
                return characterData.maxHealth;
            else
                return 0;
        }
        set
        {
            characterData.maxHealth = value;
        }
    }

    public int CurrHealth
    {
        get
        {
            if (characterData != null)
                return characterData.currHealth;
            else
                return 0;
        }
        set
        {
            characterData.currHealth = value;
        }
    }
    public int BaseDefence
    {
        get
        {
            if (characterData != null)
                return characterData.baseDefence;
            else
                return 0;
        }
        set
        {
            characterData.baseDefence = value;
        }
    }
    public int CurrDefence
    {
        get
        {
            if (characterData != null)
                return characterData.currDefence;
            else
                return 0;
        }
        set
        {
            characterData.currDefence = value;
        }
    }
    #endregion

    #region Character Combat

    public void GetHurt(CharacterStats attacker,CharacterStats defener)
    {
        int damage = Mathf.Max(attacker.CurrDamage() - defener.CurrDefence, 0);
        CurrHealth = Mathf.Max(CurrHealth-damage, 0);

        if(attacker.isCritical)
        {
            defener.GetComponent<Animator>().SetTrigger("Hit");
        }
        //Update UI
        UpdateHealthBarOnAttack?.Invoke(CurrHealth, MaxHealth);
        //经验Update
        if (CurrHealth <= 0) 
        {
            attacker.characterData.UpdateExp(characterData.killPoint);
        }
    }

    public void GetHurt(int damage,CharacterStats defender)
    {
        int currdamage = Mathf.Max(damage - defender.CurrDefence, 0);
        CurrHealth = Mathf.Max(CurrHealth - currdamage, 0);

        UpdateHealthBarOnAttack?.Invoke(CurrHealth, MaxHealth);

        if (CurrHealth <= 0)
        {
            GameManager.Instance.playerStats.characterData.UpdateExp(characterData.killPoint);
        }
    }

    private int CurrDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);

        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
        }
        return (int)coreDamage;
    }



    #endregion
}
