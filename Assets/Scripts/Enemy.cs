using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State { Idle, Chasing, Attacking };
    State currentState;

    UnityEngine.AI.NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;

    Color originalColor;

    float AttackDistanceThreshold = 1.5f;
    float timeBetweenAttacks = 1;
    float damage = 1;

    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

    protected override void Start()
    {
        base.Start();
        pathfinder = GetComponent<UnityEngine.AI.NavMeshAgent>();
        skinMaterial = GetComponent<Renderer>().material;
        originalColor = skinMaterial.color;

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

            
            StartCoroutine(UpdatePath());
        }
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    void Update()
    {   
        if(hasTarget){
            if(Time.time > nextAttackTime){
                float sqrtDistanceToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrtDistanceToTarget < Mathf.Pow(AttackDistanceThreshold + targetCollisionRadius, 2))
                {
                    nextAttackTime = timeBetweenAttacks + Time.time;
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack() {

		currentState = State.Attacking;
		pathfinder.enabled = false;

		Vector3 originalPosition = transform.position;
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

		float attackSpeed = 3;
		float percent = 0;

		skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

		while (percent <= 1) {
            if(percent >= 0.5f && !hasAppliedDamage){
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
			percent += Time.deltaTime * attackSpeed;
			float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
			transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

			yield return null;
		}

		skinMaterial.color = originalColor;
		currentState = State.Chasing;
		pathfinder.enabled = true;
	}

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;
        while (hasTarget)
        {   
            if(currentState == State.Chasing){
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + AttackDistanceThreshold/2);
                if(!dead){
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
