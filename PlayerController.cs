using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Vector3 velocity;
    Rigidbody myrigidbody;

    
    void Start()
    {
        myrigidbody = GetComponent<Rigidbody>();
    }

   
    public void Move(Vector3 _velocity) // Vector3를 volocity 객체에 넣음
    {
        velocity = _velocity;
    }

    public void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z); 
        transform.LookAt(heightCorrectPoint);
        
    }

    private void FixedUpdate()
    {
        myrigidbody.MovePosition(myrigidbody.position + velocity * Time.fixedDeltaTime);
    }    
}
