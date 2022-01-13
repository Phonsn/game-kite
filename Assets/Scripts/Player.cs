using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TwinStickController))]
[RequireComponent(typeof(GunController))]

public class Player : LivingEntity
{

    TwinStickController playerController;
    GunController gunController;
    TrailRenderer trailRenderer;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        playerController = GetComponent<TwinStickController>();
        gunController = GetComponent<GunController>();
        trailRenderer = GetComponent<TrailRenderer>();
        gunController.EquipPrimaryGun();
        ControlTrailRenderer(false);
    }

    public void ControlTrailRenderer(bool status)
    {
        trailRenderer.enabled = status;
    }

    public override void TakeDamage(float damage)
    {
        Debug.Log("Take Damage: " + damage);
        StartCoroutine("FlashRed");
        base.TakeDamage(damage);
    }

    private IEnumerator FlashRed()
    {
        Transform flashObject = transform.Find("Body/PlayerDummy");

        Color originalColor = flashObject.GetComponent<Renderer>().material.color;
        flashObject.GetComponent<Renderer>().material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        flashObject.GetComponent<Renderer>().material.color = originalColor;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
