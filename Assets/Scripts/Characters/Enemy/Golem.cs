using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{
    [Header("Skill")]
    public float kickForce = 25;

    public GameObject rockPrefab;
    public Transform handPos;

    /// <summary>
    /// Animation Event
    /// </summary>
    public void KickOff()
    {
        if (attackTarget != null && TargetInWeaponRange() && transform.isFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            Vector3 direction = (attackTarget.transform.position - transform.position).normalized;
            //direction.Normalize();

            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");

            targetStats.GetHurt(characterStats, targetStats);
        }
    }

    /// <summary>
    /// Animation Event
    /// </summary>
    private void ThrowRock()
    {
        if(attackTarget!=null)
        {
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;
        }
    }

}
