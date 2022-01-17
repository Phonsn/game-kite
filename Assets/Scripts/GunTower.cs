using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunTower : MonoBehaviour
{
    [Header("Main Characteristics")]
    public Transform[] projectileSpawn;
    public Transform turretHead;
    public float range = 5;

    public float damage = 1;
    public float msBetweenShots = 100;
    float secondsBetweenShots;

    public Projectiles projectile;
    public float muzzleVelocity = 35;
    public bool active;
    public float rotationSpeed =2f;

    public LayerMask whatIsTakingDamage;
    public LayerMask whatIsBlockingSight;

    [Header("Effects")]
    public Transform shell;
    public Transform[] shellEjection;
    public AudioClip shootAudio;

    // SCRIPT INTERNAL
    MuzzleFlash muzzleFlash;
    float nextShotTime;
    Transform currentTarget;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    private void Awake()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
    }

    void Start()
    {
        secondsBetweenShots = msBetweenShots / 1000;

        StartCoroutine(SetTarget());
    }

    void Update()
    {
        if (currentTarget!=null)
        {
            Vector3 targetDirection = currentTarget.position - turretHead.position;
            float singleStep = rotationSpeed * Time.deltaTime;

            Vector3 newDirection = Vector3.RotateTowards(turretHead.forward, targetDirection, singleStep, 0.0f);

            turretHead.rotation = Quaternion.LookRotation(newDirection);
            turretHead.localEulerAngles = new Vector3(0, turretHead.localEulerAngles.y, 0);
        }



        if (Time.time >= nextShotTime && currentTarget != null)
        {
            Shoot();
        }

    }

    IEnumerator SetTarget()
    {
        float shortestDistance;

        while (active)
        {
            shortestDistance = Mathf.Infinity;
            currentTarget = null;

            Collider[] enemies = Physics.OverlapSphere(transform.position, range, whatIsTakingDamage);
            for (int i = 0; i < enemies.Length; i++)
            {
                //Get component of enemy and call Take Hit
                IDamagable damagableObject = enemies[i].GetComponent<IDamagable>();
                if (damagableObject != null)
                {
                    Vector3 enemyPostionCorrected = enemies[i].gameObject.transform.position + Vector3.up;
                    Vector3 turretPositionCorrected = transform.position + Vector3.up;

                    Vector3 dirToEnemy = (enemies[i].gameObject.transform.position + Vector3.up - transform.position + Vector3.up).normalized;
                    
                    Debug.DrawLine(transform.position+Vector3.up, enemies[i].gameObject.transform.position + Vector3.up, Color.gray, 0.25f);

                    if (!Physics.Raycast(transform.position + Vector3.up, dirToEnemy, range, whatIsBlockingSight.value))
                    {
                        //Debug.Log("ClearSight");
                        if (Vector3.Distance(transform.position, enemies[i].gameObject.transform.position) < shortestDistance)
                        {
                            shortestDistance = Vector3.Distance(transform.position, enemies[i].gameObject.transform.position);
                            currentTarget = enemies[i].gameObject.transform;
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.25f);
        }
        
    }

    private void Shoot()
    {
        for (int i = 0; i < projectileSpawn.Length; i++)
        {
            nextShotTime = Time.time + secondsBetweenShots;

            Projectiles newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectiles;
            newProjectile.SetSpeed(muzzleVelocity);
            newProjectile.SetDamage(damage);   
        }

        muzzleFlash.Activate();

        for (int i = 0; i < shellEjection.Length; i++)
        {
            Instantiate(shell, shellEjection[i].position, shellEjection[i].rotation);
        }

        AudioManager.instance.PlaySound(shootAudio, transform.position);
    }
}
