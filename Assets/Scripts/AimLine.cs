using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimLine : MonoBehaviour
{

    public LineRenderer lineRenderer;
    public Transform aimStartPoint;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 startPoint = aimStartPoint.position + aimStartPoint.forward * 0.5f;
        Vector3 goalPoint = startPoint + aimStartPoint.forward * 10;

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, goalPoint);
    }
}
