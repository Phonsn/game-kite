using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun[] allGuns;
    public Transform equippedMine;

    public Gun equippedGun;
   
    Coroutine fireCoroutine;
    public void EquipGun(Gun gunToEquip)
    {
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;
    }

    public void EquipGun(int gunIndex)
    {
        Gun gunToEquipFromList = allGuns[gunIndex];
        EquipGun(gunToEquipFromList);
    }

    public void PlaceMine()
    {
        Instantiate(equippedMine, new Vector3(weaponHold.position.x,0, weaponHold.position.z), weaponHold.rotation);
    }

    public void OnTriggerHold()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerHold();
            fireCoroutine = StartCoroutine(equippedGun.RapidFire());
        }
    }

    public void OnTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
            }
        }
    }

    public void Reload()
    {
        if (equippedGun != null)
        {
            equippedGun.Reload();
        }
    }
}
