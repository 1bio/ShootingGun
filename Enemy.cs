using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Enemy : LivingEntity
{
    public enum state {Idle, Chasing, Attacking } // 열거형 - 같은 종류에 속한 여러 개 상수를 선언할 때 사용, 코드가 단순해지고 가독성이 좋아짐
    state currentState;

    public ParticleSystem deathEffect;

    NavMeshAgent pathfinder;
    Transform target;

    LivingEntity targetEntity;

    Material skinmaterial;
    Color orignialcolor;

    float AttackDistance = 0.5f; // 1.5f = 1.5m 
    float timeBetweenAttack = 1f;
    float damage = 1f;

    float myCollisionRadius; 
    float targetCollisionRadius;
    float nextAttackTime;

    bool hasTarget;

    protected override void Start()
    {
        base.Start();
        pathfinder = GetComponent<NavMeshAgent>();
        skinmaterial = GetComponent<Renderer>().material; // Enemy에 있는 mesh Renderer 컴포넌트의 material 호출
        orignialcolor = skinmaterial.color; // Enemy의 원래 컬러 


        if (GameObject.FindGameObjectWithTag("Player") != null) // 게임 오브젝트의 "Player" 태그가 null이 아니라면
        {
            currentState = state.Chasing;
            hasTarget = true; 

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius; // Enemy에 있는 CapsuleCollider 컴포넌트의 radius 호출
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius; // target(Player)에 있는 CapsuleCollider 컴포넌트의 radius 호출
        }

        StartCoroutine(UpdatePath());
    }


    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if(damage >= health) 
        {
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.main.startLifetime.constant);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }
    void OnTargetDeath() // 목표가 죽었을 때 
    {
        hasTarget = false;
        currentState = state.Idle;
    }


     void Update()
    {
        if (hasTarget) 
        {
            if (Time.time > nextAttackTime) // 현재 시간이 다음 공격 시간보다 클 때(다음 공격 시간이 되었을 때) 
            {
                float sqrDstTotarget = (target.position - transform.position).sqrMagnitude; // Vector3.sqrMagnitude - 계산된 거리의 제곱의 값을 리턴
                                                                                            // 정확한 거리를 몰라도 되고 단순 거리 비교만 할때 사용

                if (sqrDstTotarget < Mathf.Pow(AttackDistance + myCollisionRadius + targetCollisionRadius, 2)) // Mathf.Pow - x의 y승 
                {
                    nextAttackTime = Time.time + timeBetweenAttack; // 다음 공격 시간 = 현재 시간 + 다음 공격 시간의 텀


                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack()
    {
        currentState = state.Attacking;
        pathfinder.enabled = false;

        Vector3 originalPosition = transform.position; // orginalPosition -> targetPosition -> orginalPosition (공격 모션 순서) 
        Vector3 dirTotarget = (target.position - transform.position).normalized; // 방향벡터
        Vector3 attackPosition = target.position - dirTotarget * (myCollisionRadius); // 찌르기 애니메이션 들어가는 정도 
        

        float percent = 0; // 찌르기 애니메이션이 얼만큼 멀리갈 것인지에 대한 float
        float attackSpeed = 3f;

        skinmaterial.color = Color.red; // 공격하고 있을 때는 레드 컬러로 변환

        bool hasAppliedDamage = false; // 데미지를 적용하는 도중인가?

        while (percent <= 1) // 0 -> 1 -> 0 (공격 모션 순서)
        {
            if (percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4; // 보간 값 
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);  // 보간 값이 0 = originalPosition , 보간 값이 1 = targetPosition
                                                                                                 // Vector3.Lerp - 어떤 수치에서 어떤 수치로 값이 변경될 때 부드럽게 변경됌
            yield return null;
        }
        skinmaterial.color = orignialcolor;
        currentState = state.Chasing;
        pathfinder.enabled = true; // pathfinder의 기능 비활성화(setActive - 오브젝트 자체가 활성화 or 비활성화, enabled - 특정 컴포넌트만 비활성화) 
    }


    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f; // 1/4 = 1초에 4번            
        
        while(hasTarget) 
        {
            if (currentState == state.Chasing)
            {
                Vector3 dirTotaget = (target.position - transform.position).normalized; // 방향벡터
                Vector3 targetPosition = target.position - dirTotaget * (myCollisionRadius + targetCollisionRadius + AttackDistance / 2); // 목표 위치에서 일종의 적과 목표 사이의 방향벡터에 적과 목표의 충돌 범위의 
                                                                                                                                          // 반지름을 곱하여 뺸 값을 할당
                if (!dead) 
                {
                    pathfinder.SetDestination(targetPosition); // pathfinder는 NavmeshAgent의 레퍼런스임
                                                               // SetDestination - Enemy가 살아 있으면 계속 pathfinder를 갱신
                }
            }   
            yield return new WaitForSeconds(refreshRate);
        }

    }

}
