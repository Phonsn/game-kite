using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single };
    public enum ProjectileType {Projectile, Grenade};

    [Header("Main Characteristics")]
    public FireMode fireMode;
    public ProjectileType projectileType;
    public Transform[] projectileSpawn;

    public float damage = 1;
    public float msBetweenShots = 100;
    float secondsBetweenShots;
    
    public int burstCount = 0;
    public int projectilesPerMag = 10;
    public float reloadTime = .3f;
    public Sprite gunImage;

    [Header("Granade")]
    public Grenade grenade;
    public float shootForce, upwardForce;

    [Header("Projectile")]
    public Projectiles projectile;
    public float muzzleVelocity = 35;

    [Header("Graphics")]
    public GameObject equippedMagazine;
    public GameObject magazinePrefab;
    Material magazineMat;
    Color magazineColor;

    float nextShotTime;
    bool triggerReleaseSinceLastShot;
    int shotsRemainingInBurst;
    public int projectilesRemainingInMag { get; private set; }
    public int projectilesPerShot { get; private set; }
    bool isReloading;
    Vector3 directionWithoutSpread;

    [Header("Effects")]
    public Transform shell;
    public Transform[] shellEjection;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    MuzzleFlash muzzleFlash;

    [Header("Recoil")]
    public Vector2 gunKickbackMinMax = new Vector2(.05f, .2f);
    public Vector2 gunRecoilAnglePerShotMinMax = new Vector2(3, 5);
    public float gunMaxRecoil = 30;
    public float recoilMoveSettleTime = .1f;
    public float recoilRecoilSettleTime = .1f;

    Vector3 recoilSmoothDampVelocity;
    float recoilRotSmoothDampVelocity;
    float recoilAngle;

    private void Update()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        Debug.DrawRay(projectileSpawn[0].transform.position, fwd * 100, Color.black);
    }

    private void Awake()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        if(fireMode == FireMode.Single)
        {
            burstCount = 1;
        }

        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = projectilesPerMag;
        isReloading = false;
        projectilesPerShot = projectileSpawn.Length;

        secondsBetweenShots = msBetweenShots / 1000;
        if (equippedMagazine != null)
        {
            magazineMat = equippedMagazine.GetComponent<Renderer>().material;
            magazineColor = magazineMat.color;
        }

    }

    private void LateUpdate()
    {
        //Recoil animation

        // 1) Back and Forward
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);

        // 2) Up and down
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRecoilSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

        if (!isReloading)
        {
            transform.localEulerAngles = Vector3.left * recoilAngle;
        }

        if (!isReloading && projectilesRemainingInMag == 0)
        {
            Reload();
        }
    }

    public IEnumerator RapidFire()
    {
        while(!triggerReleaseSinceLastShot)
        {
            Shoot();
            yield return new WaitForSeconds(secondsBetweenShots);
        }
    }

    void Shoot()
    {
        if (!isReloading && Time.time > nextShotTime && projectilesRemainingInMag > 0)
        {
            if (fireMode == FireMode.Burst || fireMode == FireMode.Single)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }

            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                if (projectilesRemainingInMag == 0)
                {
                    break;
                }
                projectilesRemainingInMag--;
                nextShotTime = Time.time + msBetweenShots / 1000;

                if(projectileType == ProjectileType.Projectile)
                {
                    Projectiles newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as Projectiles;
                    newProjectile.SetSpeed(muzzleVelocity);
                    newProjectile.SetDamage(damage);
                } else if (projectileType == ProjectileType.Grenade)
                {
                    Grenade newGrenade = Instantiate(grenade, projectileSpawn[i].position, projectileSpawn[i].rotation) as Grenade;
                    newGrenade.SetDamage(damage);
                    Vector3 fwd = transform.TransformDirection(Vector3.forward);

                    RaycastHit hit;

                    if (Physics.Raycast(projectileSpawn[i].transform.position, fwd, out hit, 100))
                    {
                        Vector3 targetPosition = hit.point;
                        directionWithoutSpread = targetPosition - projectileSpawn[i].transform.position;
                        newGrenade.transform.forward = directionWithoutSpread.normalized;

                        //Add forces to bullet
                        newGrenade.GetComponent<Rigidbody>().AddForce(directionWithoutSpread.normalized * shootForce, ForceMode.Impulse);
                        newGrenade.GetComponent<Rigidbody>().AddForce(projectileSpawn[i].transform.up * upwardForce, ForceMode.Impulse);
                    }
                }
            }

            for (int i = 0; i < shellEjection.Length; i++)
            {
                Instantiate(shell, shellEjection[i].position, shellEjection[i].rotation);
            }

            muzzleFlash.Activate();

            //Recoil animation
            transform.localPosition -= Vector3.forward * Random.Range(gunKickbackMinMax.x, gunKickbackMinMax.y);

            recoilAngle += Random.Range(gunRecoilAnglePerShotMinMax.x, gunRecoilAnglePerShotMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, gunMaxRecoil);

            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }

    public void Reload()
    {
        if (!isReloading && projectilesRemainingInMag != projectilesPerMag)
        {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
    }

    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(.2f);

        if (equippedMagazine != null)
        {
            equippedMagazine.GetComponent<Renderer>().material.color = Color.clear;
            Instantiate(magazinePrefab, equippedMagazine.transform.position, equippedMagazine.transform.rotation);
        }

        float reloadSpeed = 1f / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;

        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;
            yield return null;
        }
        projectilesRemainingInMag = projectilesPerMag;

        if (equippedMagazine != null)
        {
            equippedMagazine.GetComponent<Renderer>().material.color = magazineColor;
        }

        isReloading = false;
    }

    public void OnTriggerHold()
    {
        triggerReleaseSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleaseSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }

}
