using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Grenade : Explosives
{
    [Header("Grenade Settings")]
    //Assignables
    public Rigidbody rb;

    //Lifetime
    public int maxCollisions;
    public float maxLifetime;
    public bool explodeOnTouch = true;

    //Stats
    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;

    int collisions;
    float speed;
    PhysicMaterial physics_mat;

    // ------------
    // OVERRIDES
    // ------------
    protected override void Start()
    {
        base.Start();
        Setup();
    }

    protected override void Explode()
    {
        //Explosion itself (Sound, damage, force)
        base.Explode();
    }

    // ------------
    // CLASS FUNCTIONS
    // ------------
    private void Setup()
    {
        //Create a new Physic material
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;

        //Assign material to collider
        GetComponent<SphereCollider>().material = physics_mat;

        //Set gravity
        rb.useGravity = useGravity;
    }

    private void LateUpdate()
    {
        //When to explode:
        if (collisions > maxCollisions) Explode();

        //Count down lifetime
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0) Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Don't count collisions with other bullets
        if (collision.collider.CompareTag("Projectile")) return;

        //Count up collisions
        collisions++;

        //Explode if bullet hits an enemy directly and explodeOnTouch is activated
        if (collision.collider.CompareTag("Enemy") && explodeOnTouch) Explode();
    }

}
