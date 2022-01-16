using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]

public class Enemy : LivingEntity
{
    public enum State {Idle, GoToWaypoint, ChasingPlayer, Attacking };
    UnityEngine.AI.NavMeshAgent pathfinder;

    // Enemy Characteristics
    public float moveSpeed;
    public float playerDetectionRadius;
    public float followPlayerDuration;
    public Transform statusLight;
    public int enemyType; //TODO make private later
    public static event System.Action<int> OnDeathStatic;
    public GameObject deathEffect;
    public int pointsPerKill = 5;
    public bool devTest = false;
    public int damage;
    public float timeBetweenAttacks = 1.5f;

    //Animation
    public Animation anim;

    // Script internal variables
    float myCollisionRadius;
    float waypointCollisionRadius;
    float playerCollisionRadius;
    float stoppingDistance = 0.5f;
    float attackDuration;
    float nextAttackTime;

    float playerLastSpotted;

    Material skinMaterial;
    Color originalColor;

    Transform waypoint;
    Transform playerT;
    Transform target;
    LivingEntity playerEntity;
    State currentState;

    bool playerDied;
    bool enemyHasAnimation = false;

    // Collision Detection

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
    }

    private void Awake()
    {
        // GENERAL SETUP
        myCollisionRadius = GetComponent<CapsuleCollider>().radius * transform.localScale.x;
        pathfinder = GetComponent<UnityEngine.AI.NavMeshAgent>();
        pathfinder.speed = moveSpeed;
        playerLastSpotted = Time.time - 100;
        dead = false;

        if (GetComponent<Animation>() != null)
        {
            anim = GetComponent<Animation>();
            enemyHasAnimation = true;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            playerT = GameObject.FindGameObjectWithTag("Player").transform;
            playerCollisionRadius = playerT.GetComponent<CharacterController>().radius;
            playerEntity = playerT.GetComponent<LivingEntity>();
            playerDied = false;
            nextAttackTime = Time.time + 1f; // First attacj only possible 1 second after spawning
        }

        if (GameObject.FindGameObjectWithTag("Waypoint") != null)
        {
            waypoint = GameObject.FindGameObjectWithTag("Waypoint").transform;
            waypointCollisionRadius = waypoint.GetComponent<SphereCollider>().radius;
        }

        // SET INITAL TARGET
        if (waypoint != null)
        {
            if(enemyType == 1)
            {
                target = waypoint;
                currentState = State.GoToWaypoint;
                statusLight.GetComponent<Renderer>().material.color = Color.black;
            } else if (enemyType == 2)
            {
                target = playerT;
                currentState = State.ChasingPlayer;
                statusLight.GetComponent<Renderer>().material.color = Color.red;
            } else
            {
                currentState = State.Idle;
                Debug.Log("Enemy has no Type!");
            }
        }

        //Physics.IgnoreCollision(playerT.GetComponent<CharacterController>(), GetComponent<CapsuleCollider>());

        if (target != null)
        {
            playerEntity.OnDeath += OnPlayerDeath;
            if(!devTest)
            {
                StartCoroutine("UpdatePath");
            }
        }
    }
    void OnPlayerDeath()
    {
        playerDied = true;
        currentState = State.Idle;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemyType == 1)
        {
            Debug.Log("TargetReached");
            Die();
        }  
    }

    public IEnumerator SetKinematic(bool kinematicStatus, float waitTime)
    {
        if (GetComponent<Rigidbody>() != null)
        {
            yield return new WaitForSeconds(waitTime);
            GetComponent<Rigidbody>().isKinematic = kinematicStatus;
        }
    }

    public IEnumerator SetExplosionForce(Vector3 origin, float explosionRange, float explosionForce)
    {
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            yield return new WaitForSeconds(0.1f);
            GetComponent<Rigidbody>().AddExplosionForce(explosionForce, origin, explosionRange);
            yield return new WaitForSeconds(2);
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;
        Vector3 targetPosition;
        Vector3 dirToTarget;

        while (!dead)
        {
            if(!playerDied)
            {
                if (enemyType == 2)
                {
                    targetPosition = playerT.position;

                    dirToTarget = (playerT.position - transform.position).normalized;
                    targetPosition = target.position - dirToTarget * (myCollisionRadius + playerCollisionRadius + stoppingDistance);

                    currentState = State.ChasingPlayer;
                    statusLight.GetComponent<Renderer>().material.color = Color.red;
                }
                else
                {
                    float targetCollisionRadius = waypointCollisionRadius;

                    //IDENTIFY TARGET
                    Vector3 dirToPlayer = (playerT.position - transform.position).normalized;
                    Ray ray = new Ray(transform.position + Vector3.up, dirToPlayer);

                    //Debug.DrawLine(transform.position, playerT.position, Color.green, refreshRate);

                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        //Debug.Log(hit.collider.name);
                        if (hit.distance <= playerDetectionRadius && hit.collider.name == "Player")
                        {
                            target = playerT;
                            targetCollisionRadius = playerCollisionRadius;
                            playerLastSpotted = Time.time;
                            currentState = State.ChasingPlayer;
                        }
                    }

                    if (Time.time > playerLastSpotted + followPlayerDuration)
                    {
                        target = waypoint;
                        currentState = State.GoToWaypoint;
                    }

                    // SET TARGET POSITION
                    targetPosition = target.position;
                    if (target.name == "Player")
                    {
                        dirToTarget = (target.position - transform.position).normalized;
                        targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + stoppingDistance);
                    }

                    //SHOW CURRENT STATUS
                    if (hit.collider.name == "Player" && hit.distance <= playerDetectionRadius)
                    {
                        statusLight.GetComponent<Renderer>().material.color = Color.red;
                    }
                    else if (Time.time > playerLastSpotted && Time.time < playerLastSpotted + followPlayerDuration)
                    {
                        statusLight.GetComponent<Renderer>().material.color = Color.blue;
                    }
                    else
                    {
                        statusLight.GetComponent<Renderer>().material.color = Color.black;
                    }
                }
            } else
            {
                currentState = State.Idle;
                targetPosition = transform.position;
                statusLight.GetComponent<Renderer>().material.color = Color.black;
            }

            if (!dead)
            {
                pathfinder.SetDestination(targetPosition);
            }

            if(pathfinder.remainingDistance <= 2 && currentState == State.ChasingPlayer) transform.LookAt(playerT.position);

            if (currentState == State.Idle)
            {
                if(enemyHasAnimation) anim.CrossFade("Idle", 0.1f);
            }
            else if(pathfinder.remainingDistance <= 0.6)
            {
                if(Time.time >= nextAttackTime)
                {
                    if (enemyHasAnimation) anim.CrossFade("Lumbering", 0.1f);
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    playerT.GetComponent<Player>().TakeDamage(damage);
                } else
                {
                    if (enemyHasAnimation) anim.CrossFade("Idle", 0.1f);
                }
                
            } else
            {
                if (enemyHasAnimation) anim.CrossFade("Walk", 0.1f);
            }

            yield return new WaitForSeconds(refreshRate);
        }

    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);

        float particleLifeTime = deathEffect.GetComponent<ParticleSystem>().main.startLifetime.constant;

        if (damage >= health && !dead)
        {
            if (OnDeathStatic != null)
            {
                OnDeathStatic(pointsPerKill);
            }
            AudioManager.instance.PlaySound("EnemyDeath", transform.position);
            Destroy(Instantiate(deathEffect, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, particleLifeTime);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    public void SetCharacteristics(int parEnemyType, float parMoveSpeed, float parHealth, int parDamage, Color skinColor) {
        enemyType = parEnemyType;
        startingHealth = parHealth;
        pathfinder.speed = parMoveSpeed;
        damage = parDamage;

        if(skinMaterial!=null)
        {
            skinMaterial.color = skinColor;
            originalColor = skinMaterial.color;
        }
    }
}
