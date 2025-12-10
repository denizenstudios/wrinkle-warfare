using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator; // Optional - for when you add your mesh
    
    [Header("Detection")]
    public float detectionRadius = 15f;
    public float attackRange = 2f;
    public float fieldOfViewAngle = 110f;
    public LayerMask playerLayer;
    public LayerMask obstructionLayer;
    
    [Header("Behavior")]
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    public float chaseSpeed = 3.5f;
    public float patrolSpeed = 1.5f;
    
    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float health = 100f;
    
    // State machine
    private enum ZombieState { Idle, Patrol, Chase, Attack }
    private ZombieState currentState = ZombieState.Patrol;
    
    // Private variables
    private Vector3 patrolDestination;
    private float patrolWaitTimer;
    private float attackTimer;
    private bool playerDetected;
    private Vector3 lastKnownPlayerPosition;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        agent.speed = patrolSpeed;
        SetNewPatrolDestination();
    }
    
    void Update()
    {
        // Check for player detection
        DetectPlayer();
        
        // Update state machine
        switch (currentState)
        {
            case ZombieState.Idle:
                IdleBehavior();
                break;
            case ZombieState.Patrol:
                PatrolBehavior();
                break;
            case ZombieState.Chase:
                ChaseBehavior();
                break;
            case ZombieState.Attack:
                AttackBehavior();
                break;
        }
        
        // Update attack timer
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }
    
    #region Detection
    void DetectPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player is within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            // Check if player is within field of view
            if (angleToPlayer < fieldOfViewAngle / 2f)
            {
                // Check line of sight
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstructionLayer))
                {
                    playerDetected = true;
                    lastKnownPlayerPosition = player.position;
                    
                    // Transition to chase if not already attacking
                    if (currentState != ZombieState.Attack && distanceToPlayer > attackRange)
                    {
                        TransitionToState(ZombieState.Chase);
                    }
                    else if (distanceToPlayer <= attackRange)
                    {
                        TransitionToState(ZombieState.Attack);
                    }
                    return;
                }
            }
        }
        
        // Player lost
        if (playerDetected && currentState == ZombieState.Chase)
        {
            // Continue to last known position
            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1f)
            {
                playerDetected = false;
                TransitionToState(ZombieState.Patrol);
            }
        }
    }
    #endregion
    
    #region State Behaviors
    void IdleBehavior()
    {
        patrolWaitTimer -= Time.deltaTime;
        
        if (patrolWaitTimer <= 0)
        {
            SetNewPatrolDestination();
            TransitionToState(ZombieState.Patrol);
        }
    }
    
    void PatrolBehavior()
    {
        agent.speed = patrolSpeed;
        
        // Check if reached destination
        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolWaitTimer = patrolWaitTime;
            TransitionToState(ZombieState.Idle);
        }
    }
    
    void ChaseBehavior()
    {
        agent.speed = chaseSpeed;
        
        if (playerDetected && player != null)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            // Move to last known position
            agent.SetDestination(lastKnownPlayerPosition);
        }
    }
    
    void AttackBehavior()
    {
        agent.ResetPath();
        
        if (player == null) return;
        
        // Look at player
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if still in attack range
        if (distanceToPlayer > attackRange)
        {
            TransitionToState(ZombieState.Chase);
            return;
        }
        
        // Attack
        if (attackTimer <= 0)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
    }
    #endregion
    
    #region Actions
    void PerformAttack()
    {
        Debug.Log("Zombie attacks!");
        
        // Play attack animation if you have one
        if (animator != null)
            animator.SetTrigger("Attack");
        
        // Deal damage to player
        if (player != null)
        {
            // You'll need to implement player health system
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }
    
    void SetNewPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            patrolDestination = hit.position;
            agent.SetDestination(patrolDestination);
        }
    }
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        
        if (health <= 0)
        {
            Die();
        }
        else
        {
            // Aggro on player if hit
            playerDetected = true;
            if (player != null)
                lastKnownPlayerPosition = player.position;
            TransitionToState(ZombieState.Chase);
        }
    }
    
    void Die()
    {
        Debug.Log("Zombie died!");
        // Add death animation/effects here
        Destroy(gameObject, 0.5f);
    }
    #endregion
    
    #region State Management
    void TransitionToState(ZombieState newState)
    {
        // Exit current state
        switch (currentState)
        {
            case ZombieState.Attack:
                agent.isStopped = false;
                break;
        }
        
        // Enter new state
        currentState = newState;
        
        switch (newState)
        {
            case ZombieState.Idle:
                agent.ResetPath();
                patrolWaitTimer = patrolWaitTime;
                break;
            case ZombieState.Patrol:
                SetNewPatrolDestination();
                break;
            case ZombieState.Attack:
                agent.isStopped = true;
                break;
        }
    }
    #endregion
    
    // Visualization in editor
    void OnDrawGizmosSelected()
    {
        // Detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Field of view
        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfViewAngle / 2f, transform.up) * transform.forward * detectionRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfViewAngle / 2f, transform.up) * transform.forward * detectionRadius;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);
        
        // Patrol radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}