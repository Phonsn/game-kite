using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildingGhostWorld : MonoBehaviour
{

    public Transform buildingGhostPlayer;
    public bool canPlaceBuilding;
    //public Transform pathStart;
    //public Transform pathEnd;
    //public NavMeshPath navMeshPath;

    Color originalColor;
    Vector3 truePos;
    Rigidbody myRigidbody;

    private void Awake()
    {
        originalColor = GetComponent<Renderer>().material.color;
        myRigidbody = GetComponent<Rigidbody>();

        transform.position = buildingGhostPlayer.position;
    }

    private void Start()
    {
        Collider[] initalCollisions = Physics.OverlapSphere(transform.position, .1f);

        if (initalCollisions.Length > 0)
        {
            //Debug.Log("InitialCollisions");
            canPlaceBuilding = false;
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            //Debug.Log("NO InitialCollisions");
            canPlaceBuilding = true;
            GetComponent<Renderer>().material.color = originalColor;
        }
    }

    private void FixedUpdate()
    {
        float gridSize = 1;

        truePos.x = Mathf.Floor(buildingGhostPlayer.position.x / gridSize) * gridSize + 0.5f;
        truePos.y = Mathf.Floor(buildingGhostPlayer.position.y / gridSize) * gridSize + 0.5f;
        truePos.z = Mathf.Floor(buildingGhostPlayer.position.z / gridSize) * gridSize + 0.5f;

        myRigidbody.MovePosition(truePos);
        //myRigidbody.MovePosition(transform.position + m_Input * Time.deltaTime * m_Speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Trigger Start");
        GetComponent<Renderer>().material.color = Color.red;
        canPlaceBuilding = false;
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("Trigger End");
        GetComponent<Renderer>().material.color = originalColor;
        canPlaceBuilding = true;
    }

}
