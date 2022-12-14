using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    private CharacterStats characterStats;

    private GameObject attackTarget;
    private float lastAttackTime;

    private bool isDead;

    private float stopDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
    }

    private void OnEnable()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RegisterPlayer(characterStats);
    }

    private void Start()
    {
        SaveManager.Instance.LoadPlayerData();
        
    }

    private void OnDisable()
    {
        if (!MouseManager.IsInitialized) return;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }

    private void Update()
    {
        isDead = characterStats.CurrHealth == 0;
        if(isDead)
        {
            agent.isStopped = true;
            GameManager.Instance.NotifyObservers();
        }
        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }


    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDead) return;
        //agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (isDead) return;
        if (target != null)
        {
            attackTarget = target;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        transform.LookAt(attackTarget.transform);

        while (!TargetInAttackRange()) 
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        agent.isStopped = true;

        if (characterStats.getHit)
        {
            lastAttackTime = characterStats.characterData.getHitWaitTime;
        }

        if (lastAttackTime < 0) 
        {
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            //重置冷却时间
            lastAttackTime = characterStats.attackData.coolDown;
        }
    }

    /// <summary>
    /// 判断目标是否在近战攻击范围内
    /// </summary>
    /// <returns></returns>
    private bool TargetInAttackRange()
    {
        if (attackTarget != null)
        {
            if(attackTarget.GetComponent<NavMeshAgent>())
                return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange + attackTarget.GetComponent<NavMeshAgent>().radius;
            else
                return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 判断目标是否在远程攻击范围内
    /// </summary>
    /// <returns></returns>
    private bool TargetInSkillRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 武器到达距离
    /// </summary>
    /// <returns></returns>
    private bool TargetInWeaponRange()
    {
        if (attackTarget != null)
        {
            if (attackTarget.GetComponent<NavMeshAgent>())
                return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.WeaponRange + attackTarget.GetComponent<NavMeshAgent>().radius;
            else
                return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.WeaponRange;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Animation Event
    /// </summary>
    private void Hit()
    {
        if(attackTarget.CompareTag("Attackable"))
        {
            if(attackTarget.GetComponent<Rock>())
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 25, ForceMode.Impulse);
            }
        }

        else if(TargetInWeaponRange())
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.GetHurt(characterStats, targetStats);
        }
    }

}
