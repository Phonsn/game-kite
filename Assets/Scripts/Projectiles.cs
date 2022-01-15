using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectiles : MonoBehaviour
{
    public LayerMask collisionMask;
    public Color trailColor;
    float speed = 10;
    float damage = 1;
    Vector2 minMaxSpread = new Vector2(0f,0f);
    float spread;
    bool spreadSet;


    float liftime = 3;
    float skinWidth = .1f;

    private void Start()
    {
        Destroy(gameObject, liftime);
        Collider[] initalCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);

        if (initalCollisions.Length > 0)
        {
            OnHitObject(initalCollisions[0], transform.position);
        }

        spreadSet = false;
        spread = Random.Range((minMaxSpread.y- minMaxSpread.x)*-1, (minMaxSpread.y - minMaxSpread.x)) + minMaxSpread.x;

        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void SetSpread(Vector2 newSpread)
    {
        minMaxSpread = newSpread;
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);

        transform.Translate(Vector3.forward * moveDistance);
        if (!spreadSet)
        {
            transform.Rotate(0.0f, spread, 0.0f, Space.Self);
            spreadSet = true;
        }

        //transform.Translate(Vector3.forward * moveDistance);

    }

    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        IDamagable damagableObject = c.GetComponent<IDamagable>();
        if (damagableObject != null)
        {
            damagableObject.TakeHit(damage, hitPoint, transform.forward);
        }
        GameObject.Destroy(gameObject);
    }

}
