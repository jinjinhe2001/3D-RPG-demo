using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates
{
    GUARD,
    PATROL,
    CHASE,
    DEAD
}


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private NavMeshAgent agent;
    private EnemyStates enemyStates;

    private Animator anim;

    private Collider coll;

    protected CharacterStats characterStats;

    [Header("Basic Settings")]
    [Tooltip("视野范围")]
    public float sightRadius;
    [Tooltip("是否为守卫状态")]
    public bool isGuard;

    private float speed;

    protected GameObject attackTarget;

    [Tooltip("巡逻停留时间")]
    public float lookAtTime;
    private float remainLookAtTime;
    [Tooltip("耐心值时间")]
    public float FightWaitingTime;
    private float remainFightWaitingTime;

    //攻击间隔计时
    private float lastAttackTime;

    private Quaternion guardRotation;

    [Header("Patrol State")]
    [Tooltip("巡逻范围")]
    public float patrolRange;

    //巡逻时随机的点
    private Vector3 wayPoint;

    //守卫时的初始位置
    private Vector3 guardPos;

    //bool配合动画
    private bool isWalk;
    private bool isChase;
    private bool isFollow;
    private bool isDead;

    private bool playerDead = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
        remainFightWaitingTime = FightWaitingTime;
    }
    private void Start()
    {
        //characterStats.characterData.currHealth = characterStats.characterData.maxHealth;
        if(isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        //FIXME:场景切换后修改
        GameManager.Instance.AddObserver(this);
    }

    //切换场景时调用
    //private void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    private void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);   
    }

    private void Update()
    {
        if(characterStats.CurrHealth==0)
        {
            isDead = true;
        }
        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
            if (lastAttackTime >= 0) lastAttackTime -= Time.deltaTime;
        }
    }

    private void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Death", isDead);
        anim.SetBool("Critical", characterStats.isCritical);
    }

    private void SwitchStates()
    {
        if (isDead)
        {
            enemyStates = EnemyStates.DEAD;
        }
            //如果发现Player，切换到CHASE
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;
                if(transform.position!=guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    if(Vector3.SqrMagnitude(guardPos-transform.position)<=agent.stoppingDistance)
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;

                //判断是否走到了随机巡逻点
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else
                    {
                        GetNewWayPoint();
                    }
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }

                break;
            case EnemyStates.CHASE:
                ChaseTarget(attackTarget);
                break;
            case EnemyStates.DEAD:
                agent.radius = 0;
                coll.enabled = false;
                Destroy(gameObject,2f);
                break;
        }

    }

    /// <summary>
    /// 追击目标
    /// </summary>
    /// <param name="target"></param>
    private void ChaseTarget(GameObject target)
    {
        isWalk = false;
        isChase = true;
        agent.speed = speed;

        if (!FoundPlayer())
        {
            isFollow = false;
            agent.isStopped = false;
            //如果玩家脱离视野，则站桩直到耐心值降为零，回到守卫/巡逻状态
            if (remainFightWaitingTime > 0)
            {
                agent.destination = transform.position;
                remainFightWaitingTime -= Time.deltaTime;
            }
            else
            {
                if (isGuard)
                    enemyStates = EnemyStates.GUARD;
                else
                    enemyStates = EnemyStates.PATROL;
            }
        }
        else
        {
            //如果发现玩家，刷新耐心值
            if (remainFightWaitingTime != FightWaitingTime)
                remainFightWaitingTime = FightWaitingTime;

            isFollow = true;
            agent.isStopped = false;
            agent.destination = target.transform.position;
        }
        //在攻击范围内则攻击
        if (TargetInAttackRange() || TargetInSkillRange()) 
        {
            isFollow = false;
            agent.isStopped = true;

            if(characterStats.getHit)
            {
                lastAttackTime = characterStats.characterData.getHitWaitTime;
            }

            if (lastAttackTime <= 0) 
            {
                lastAttackTime = characterStats.attackData.coolDown;
                //暴击判断
                characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                //执行攻击
                Attack();
            }

        }
    }

    private void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if(TargetInAttackRange()&&!TargetInSkillRange())
        {
            //只在近战范围内，播放近身攻击动画
            anim.SetTrigger("Attack");
        }
        else if (TargetInSkillRange()&&!TargetInAttackRange())
        {
            //只在技能范围内，播放技能攻击动画
            anim.SetTrigger("Skill");
        }
        else if(TargetInSkillRange() && TargetInAttackRange())
        {
            //在近战和技能范围内， 随机播放两种动画
            int fightRandom = Random.Range(0, 2);
            if(fightRandom==0)
                anim.SetTrigger("Attack");
            else
                anim.SetTrigger("Skill");
        }
    }

    private bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach(var target in colliders)
        {
            if(target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }

    /// <summary>
    /// 判断目标是否在近战攻击范围内
    /// </summary>
    /// <returns></returns>
    private bool TargetInAttackRange()
    {
        if (attackTarget != null) 
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange+ attackTarget.GetComponent<NavMeshAgent>().radius;
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
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange + attackTarget.GetComponent<NavMeshAgent>().radius;
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
    protected bool TargetInWeaponRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.WeaponRange + attackTarget.GetComponent<NavMeshAgent>().radius;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 随机从巡逻范围中获取一个点
    /// </summary>
    private void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, guardPos.y, guardPos.z + randomZ);

        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    /// <summary>
    /// Animation Event
    /// </summary>
    private void Hit()
    {
        if(attackTarget!=null&& TargetInWeaponRange()&&transform.isFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.GetHurt(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        //获胜动画
        //停止所有移动
        //停止agent
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
        

    }
}
