using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableObject : LivingEntity
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);

        //TODO Play death effect

        base.TakeHit(damage, hitPoint, hitDirection);
    }
}
