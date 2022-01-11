using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public enum ActiveGun { Primary, Secondary };
    ActiveGun activeGun;

    public Transform weaponHold;
    //public Gun[] availableGuns;
    public Gun primaryGun;
    public Gun secondaryGun;
    public Transform equippedMine;

    public Gun equippedGun;
    
   
    Coroutine fireCoroutine;
    //public void EquipGun(Gun gunToEquip)
    //{
    //    if (equippedGun != null)
    //    {
    //        Destroy(equippedGun.gameObject);
    //    }
    //    equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
    //    equippedGun.transform.parent = weaponHold;
    //}

    //public void EquipGun(int gunIndex)
    //{
    //    Gun gunToEquipFromList = availableGuns[gunIndex];
    //    EquipGun(gunToEquipFromList);
    //}

    public void PlaceMine()
    {
        Instantiate(equippedMine, new Vector3(weaponHold.position.x,0, weaponHold.position.z), weaponHold.rotation);
    }

    public void ChangeGun()
    {
        if(activeGun == ActiveGun.Secondary)
        {
            EquipPrimaryGun();
        } else
        {
            EquipSecondaryGun();
        }
    }

    public void EquipPrimaryGun()
    {
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(primaryGun, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;

        activeGun = ActiveGun.Primary;
    }

    public void EquipSecondaryGun()
    {
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(secondaryGun, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;

        activeGun = ActiveGun.Secondary;
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
