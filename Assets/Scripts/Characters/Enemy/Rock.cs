using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates
    {
        HitPlayer,
        HitEnemy,
        HitNothing
    }

    public RockStates rockStates;

    private Rigidbody rb;

    [Header("Basic Setting")]
    public float force;
    public int damage;

    public GameObject target;

    private Vector3 direction;

    public GameObject breakEffect;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }

    private void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude < 3f) 
        {
            rockStates = RockStates.HitNothing;
        }
    }

    public void FlyToTarget()
    {
        if(target==null)
        {
            target = FindObjectOfType<PlayerController>().gameObject;
        }
        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);

    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if(collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;

                    collision.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    collision.gameObject.GetComponent<CharacterStats>().GetHurt(damage, collision.gameObject.GetComponent<CharacterStats>());
                    rockStates = RockStates.HitNothing;
                }
                break;
            case RockStates.HitEnemy:
                if(collision.gameObject.GetComponent<Golem>())
                {
                    var collisionStats = collision.gameObject.GetComponent<CharacterStats>();
                    collisionStats.GetHurt(damage, collisionStats);
                    Instantiate(breakEffect, collision.contacts[0].point, Quaternion.identity);
                    
                    Destroy(gameObject);
                }
                break;
            case RockStates.HitNothing:
                break;
        }
    }
}