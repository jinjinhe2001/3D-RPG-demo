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
    [Tooltip("��Ұ��Χ")]
    public float sightRadius;
    [Tooltip("�Ƿ�Ϊ����״̬")]
    public bool isGuard;

    private float speed;

    protected GameObject attackTarget;

    [Tooltip("Ѳ��ͣ��ʱ��")]
    public float lookAtTime;
    private float remainLookAtTime;
    [Tooltip("����ֵʱ��")]
    public float FightWaitingTime;
    private float remainFightWaitingTime;

    //���������ʱ
    private float lastAttackTime;

    private Quaternion guardRotation;

    [Header("Patrol State")]
    [Tooltip("Ѳ�߷�Χ")]
    public float patrolRange;

    //Ѳ��ʱ����ĵ�
    private Vector3 wayPoint;

    //����ʱ�ĳ�ʼλ��
    private Vector3 guardPos;

    //bool��϶���
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
        //FIXME:�����л����޸�
        GameManager.Instance.AddObserver(this);
    }

    //�л�����ʱ����
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
            //�������Player���л���CHASE
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

                //�ж��Ƿ��ߵ������Ѳ�ߵ�
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
    /// ׷��Ŀ��
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
            //������������Ұ����վ׮ֱ������ֵ��Ϊ�㣬�ص�����/Ѳ��״̬
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
            //���������ң�ˢ������ֵ
            if (remainFightWaitingTime != FightWaitingTime)
                remainFightWaitingTime = FightWaitingTime;

            isFollow = true;
            agent.isStopped = false;
            agent.destination = target.transform.position;
        }
        //�ڹ�����Χ���򹥻�
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
                //�����ж�
                characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                //ִ�й���
                Attack();
            }

        }
    }

    private void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if(TargetInAttackRange()&&!TargetInSkillRange())
        {
            //ֻ�ڽ�ս��Χ�ڣ����Ž���������
            anim.SetTrigger("Attack");
        }
        else if (TargetInSkillRange()&&!TargetInAttackRange())
        {
            //ֻ�ڼ��ܷ�Χ�ڣ����ż��ܹ�������
            anim.SetTrigger("Skill");
        }
        else if(TargetInSkillRange() && TargetInAttackRange())
        {
            //�ڽ�ս�ͼ��ܷ�Χ�ڣ� ����������ֶ���
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
    /// �ж�Ŀ���Ƿ��ڽ�ս������Χ��
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
    /// �ж�Ŀ���Ƿ���Զ�̹�����Χ��
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
    /// �����������
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
    /// �����Ѳ�߷�Χ�л�ȡһ����
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
        //��ʤ����
        //ֹͣ�����ƶ�
        //ֹͣagent
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
        

    }
}
