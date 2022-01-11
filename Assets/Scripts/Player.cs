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

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
