using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image healthBarImage;
    [SerializeField]
    private float updateSpeedSeconds = 0.5f;

    private void Awake()
    {
        GetComponentInParent<LivingEntity>().OnHealthChangedPercent += HandleHealthChange;
    }

    private void HandleHealthChange(float percent)
    {
        StartCoroutine(ChangeHealthToPercent(percent));
    }

    private IEnumerator ChangeHealthToPercent(float percent)
    {
        float percentBeforeChange = healthBarImage.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < updateSpeedSeconds)
        {
            elapsedTime += Time.deltaTime;
            healthBarImage.fillAmount = Mathf.Lerp(percentBeforeChange, percent, elapsedTime / updateSpeedSeconds);
            yield return null;
        }

        healthBarImage.fillAmount = percent;
    }

    private void LateUpdate()
    {
        Transform mainCam = FindObjectOfType<Camera>().transform;
        transform.LookAt(transform.position + mainCam.forward);
        transform.Rotate(0, 180, 0);
    }

}
