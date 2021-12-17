using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Projectile : MonoBehaviour
{
    public LayerMask collisionmask;
    public float speed = 10f;
    public float damage = 1f;

    float Lifetime = 3f;
    float skinWidth = 0.1f;

     void Start()
     {
        Destroy(gameObject, Lifetime);

        Collider[] initalCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionmask); // 발사체와 겹쳐있는 모든 충돌체들의 배열
       
        if(initalCollisions.Length > 0) // 총알을 생성했을 때 어떤 충돌 오브젝트와 이미 겹친 상태일 때
        {
            OnHitObject(initalCollisions[0], transform.position);
        }
     }
    public void Setspeed(float newSpeed)
    {
        speed = newSpeed; 
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        CheckCollision(moveDistance);
        transform.Translate(Vector3.forward * Time.deltaTime * speed);

    }

    void CheckCollision(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth , collisionmask, QueryTriggerInteraction.Collide)) 
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint) // 충돌한 첫번째 Enemy에게 데미지를 주고, 발사체를 파괴 
    {
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);
        }
        GameObject.Destroy(gameObject);
    }
    
}
