using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun startingGun;
    Gun equipedGun;


    void Start()
    {
        if(startingGun != null)
        {
            EquipGun(startingGun);
        }
    }
    public void EquipGun(Gun gunToEquip)
    {
        if (gunToEquip != null)
        {
            equipedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
            equipedGun.transform.parent = weaponHold;
        }
    }
    public void Shoot()
    {
        if(equipedGun != null)
        {
            equipedGun.Shoot();
        }
    }

    
}
