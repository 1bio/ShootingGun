using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float movespeed = 5;
    PlayerController controller; // PlayerController 타입 선언
    Camera viewcamera;
    GunController gunController; // GunController 타입 선언


     protected override void Start()
    {
        base.Start();
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewcamera = Camera.main; // 메인 카메라(카메라가 여러개면 첫번째 카메라를 비춤)

    } 

    
    void Update()
    {
        // 이동을 입력받는 곳 
        Vector3 inputmove = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")); 
        Vector3 movevelocity = inputmove.normalized * movespeed;
        controller.Move(movevelocity);


        // 바라보는 방향을 입력받는 곳 
        Ray ray = viewcamera.ScreenPointToRay(Input.mousePosition); // 스크린 공간에 있는 한 점으로 부터 레이 값을 반환
        Plane Groundplane = new Plane(Vector3.up, Vector3.zero); // Plane.Plane : 평면을 생성 
                                                                 // (inNormal 값(법선벡터), inPoint 값(원점에서의 거리) 여기서 inNormal 값은 정규화된 벡터 값만 가능 
                                                                 // Plane은 평면이기 때문에, Plane에 수직인 Vector.up = Vector(0,1,0) 값을 대입
        float rayDistance;
        if(Groundplane.Raycast(ray, out rayDistance)) // 조건문이 true = ray가 바닥과 교차함, 카메라에서 ray가 부딫힌 지점까지의 거리를 알 수 있음                                       
        {
            Vector3 point = ray.GetPoint(rayDistance); // ray의 rayDistance 값을 반환
         // Debug.DrawLine(ray.origin, point, Color.red); // 레이저 확인 
            controller.LookAt(point);
        }

        // 무기를 조작 입력을 받는 곳
        if (Input.GetMouseButton(0))
        {
            gunController.Shoot();
        }

    }
}
