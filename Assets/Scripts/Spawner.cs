using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public enum SpawnType { Lane,Hunter };

    //SPAWNER CHARACHTERISTICS
    public SpawnType spawnType;
    public Enemy enemyToSpawn;
    public Color skinColor = Color.black;

    public float timeBeforeFirstSpawn;
    public float timeBetweenSpawns;
    public float enemyHealth;

    public float enemyMoveSpeed;

    public int numberOfSpawns;

    //STATUS
    public bool active;

    //SCRIPT INTERNAL
    float enemiesSpawned;
    int enemyTypeAsInt;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position+ new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(1, 1, 1));
    }

    void Start()
    {
        enemiesSpawned = 0;

        if (spawnType == SpawnType.Lane)
        {
            enemyTypeAsInt = 1;
        }
        if (spawnType == SpawnType.Hunter)
        {
            enemyTypeAsInt = 2;
        }

        //Debug.Log(spawnType);

        StartCoroutine("SpawnEnemies");
    }

    IEnumerator SpawnEnemies()
    {
        if(timeBeforeFirstSpawn != 0)
        {
            yield return new WaitForSeconds(timeBeforeFirstSpawn);
        }

        while (active)
        {
            if(enemiesSpawned < numberOfSpawns)
            {
                Enemy spawnedEnemy = Instantiate(enemyToSpawn, transform.position, Quaternion.identity) as Enemy;
                spawnedEnemy.SetCharacteristics(enemyTypeAsInt, enemyMoveSpeed,enemyHealth,skinColor);

                enemiesSpawned++;
            }
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

    }
}
