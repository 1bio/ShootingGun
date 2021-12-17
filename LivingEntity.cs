using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour,IDamageable 
{
    [SerializeField]
    protected float StartingHealth; 
    public event System.Action OnDeath;

    protected float health;
    protected bool dead; 

    protected virtual void Start()
    {
        health = StartingHealth;
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint ,Vector3 hitDirection)
    {   // 나중에 hit 변수 사용해서 다른 처리들 추가 예정
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            Die();
        }

    }

    [ContextMenu("Self Destruct")]
    protected void Die()
    {
        dead = true;
        if(OnDeath != null) // 적이 죽을때마다 OnDeath 호출 -> OnEnemyDeath()에 알림을 받음
        {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }

}
