using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{

    public Rigidbody myRigidbody;
    public float forceMin;
    public float forceMax;
    public AudioClip shellSound;

    float lifetime = 4;
    float fadeTime = 2;

    // Start is called before the first frame update
    void Start()
    {
        float force = Random.Range(forceMin, forceMax);
        myRigidbody.AddForce(transform.right * force);
        myRigidbody.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
        Invoke("playBulletSound", .5f);
    }

    private void playBulletSound()
    {
        if (shellSound != null)
        {
            AudioManager.instance.PlaySound(shellSound, transform.position);
        }
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifetime);

        float percent = 0;
        float fadeSpeed = 1 / fadeTime;
        Material mat = GetComponent<Renderer>().material;
        Color initialColor = mat.color;

        while (percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null;
        }
        Destroy(gameObject);
    }
}
