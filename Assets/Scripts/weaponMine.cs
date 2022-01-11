using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponMine : Explosives
{
    [Header("Mine Settings")]
    public float triggerRadius;

    public LayerMask triggerEntities;
    public float explosionDelay;
    public Color flashColor;
    public Transform flashLight;

    public AudioClip activateSound;
    public AudioClip placeSound;

    float checkForTriggerTime = 0.25f;
    float lightFlashSpeed = 4;
    bool triggered;
    Color originalColor;
    Material flashLightMaterial;

    // ------------
    // OVERRIDES
    // ------------
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        triggered = false;
        flashLightMaterial = flashLight.GetComponent<Renderer>().material;
        originalColor = flashLightMaterial.color;
        if (placeSound != null) AudioManager.instance.PlaySound(placeSound, transform.position);

        StartCoroutine("CheckForTrigger");
    }

    protected override void Explode()
    {
        //Explosion itself (Sound, damage, force)
        base.Explode();
    }

    // ------------
    // CLASS FUNCTIONS
    // ------------
    private IEnumerator CheckForTrigger()
    {
        while (!triggered)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, triggerRadius, triggerEntities);
            for(int i=0; i <enemies.Length; i++)
            {
                if (!Physics.Raycast(transform.position, (enemies[i].transform.position - transform.position).normalized, triggerRadius, blockingExplosion.value))
                {
                    triggered = true;
                    if (activateSound != null) InvokeRepeating("playSound", 0, 1 / lightFlashSpeed);
                    StartCoroutine("PrepareForExplosion");
                    break;
                }
            }
            yield return new WaitForSeconds(checkForTriggerTime);
        }
        
    }

    private void playSound()
    {
        AudioManager.instance.PlaySound(activateSound, transform.position);
    }

    private IEnumerator PrepareForExplosion()
    {
        float explodeTimer = 0;
        
        while (explodeTimer < explosionDelay)
        {
            flashLightMaterial.color = Color.Lerp(originalColor, flashColor, Mathf.PingPong(explodeTimer * lightFlashSpeed, 1));
            explodeTimer += Time.deltaTime;
            yield return null;
        }

        Explode();
    }

}
