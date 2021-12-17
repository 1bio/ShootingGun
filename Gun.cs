using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform firePos; // 총알 생성 위치
    public Projectile projectile; // 발사체의 스크립트 내용을 가져옴
    public float msBetweenShot = 100; // 연사력
    public float projectileVelocity = 35; // 총알 발사 순간 속도 

    float nextShotTime;
    public void Shoot() // 총알 발사
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + msBetweenShot / 1000; // 100/1000 = 0.1(밀리세컨드를 세컨드로 바꿈) 
            Projectile newprojectile = Instantiate(projectile, firePos.position, firePos.rotation) as Projectile;
            newprojectile.Setspeed(projectileVelocity);
        }
    }
    
    
}
