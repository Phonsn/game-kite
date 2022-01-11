using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosives : MonoBehaviour
{
    [Header("Explosion Settings")]
    //Assignables
    public GameObject explosion;
    public GameObject blast;
    public AudioClip explosionSound;
    public LayerMask whatIsTakingDamage;
    public LayerMask whatIsCollisions;
    public LayerMask ignoreInExplosion;
    public LayerMask blockingExplosion;

    //Damage
    public float explosionDamage;
    public float explosionRange;
    public float explosionForce;

    protected bool explosionStarted;

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }

    protected virtual void Start()
    {
        explosionStarted = false;
    }

    protected virtual void Explode()
    {
        if (explosionStarted) return;
        explosionStarted = true;

        // Instantiate the boom
        if (explosion != null)
        {
            float explosionDuration = explosion.GetComponent<ParticleSystem>().main.duration;
            Destroy(Instantiate(explosion, transform.position, Quaternion.identity), explosionDuration);
        }

        if (blast != null)
        {
            float explosionDuration = 0.5f;
            //Quaternion spawnRotation = Quaternion.Euler(90, 0, 0);
            Destroy(Instantiate(blast, new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity), explosionDuration);
        }

        if (explosionSound != null) AudioManager.instance.PlaySound(explosionSound, transform.position);

        //Check for enemies and assign damage
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsTakingDamage);
        for (int i = 0; i < enemies.Length; i++)
        {
            //Get component of enemy and call Take Hit
            IDamagable damagableObject = enemies[i].GetComponent<IDamagable>();
            if (damagableObject != null)
            {
                //RAYCAST ALL
                Vector3 dirToEnemy = (enemies[i].gameObject.transform.position - transform.position).normalized;
                RaycastHit[] allRayHits;
                allRayHits = Physics.RaycastAll(transform.position, dirToEnemy, explosionRange);

                float minDistanceObstacle = Mathf.Infinity;
                float distanceGameObject = Mathf.Infinity;
                int objectID = enemies[i].gameObject.GetInstanceID();

                Vector3 hitPoint = enemies[i].transform.position; //define default hitpoint, just to have one

                foreach (RaycastHit hitLocal in allRayHits)
                {
                    if (hitLocal.collider.gameObject.tag == "Obstacle" && hitLocal.distance < minDistanceObstacle)
                    {
                        minDistanceObstacle = hitLocal.distance;
                    }

                    if (hitLocal.collider.gameObject.GetInstanceID() == objectID)
                    {
                        distanceGameObject = hitLocal.distance;
                        hitPoint = hitLocal.point;
                        //Debug.Log("Enemy found");
                    }
                }

                if (distanceGameObject < minDistanceObstacle)
                {
                    damagableObject.TakeHit(explosionDamage, hitPoint, dirToEnemy);
                }

            }

            //Add explosion force (if enemy has a rigidbody)
            if (enemies[i].GetComponent<Rigidbody>())
            {
                // Run coroutine on enemy, not the explosice as that gets destroyed, cancelling the coroutine here
                Enemy obj = enemies[i].GetComponent<Enemy>();
                obj.StartCoroutine(obj.SetExplosionForce(new Vector3(transform.position.x, 0, transform.position.z), explosionRange, explosionForce));
            }
        }

        Invoke("Delay", 0.05f);
    }

    private void Delay()
    {
        Destroy(gameObject);
    }

    public void SetDamage(float newDamage)
    {
        explosionDamage = newDamage;
    }


    //private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    //{
    //    return ((layerMask.value & (1 << obj.layer)) > 0);
    //}

}
