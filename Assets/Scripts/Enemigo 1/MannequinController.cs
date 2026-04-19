using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class MannequinController : MonoBehaviour
{
    public enum EnemyState { Patrol, Pursuit, Search, Frozen, Attack }
    public EnemyState currentState;

    [Header("Referencias")]
    public Transform player;
    public Transform playerCamera;
    public LineOfSight playerLoS;
    public Transform chestPoint;

    [Header("Configuración")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 6.5f;
    public float killDistance = 1.5f;

    [HideInInspector] public Vector3 lastKnownPosition;
    [HideInInspector] public NavMeshAgent agent;

    private float wanderTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ChangeState(EnemyState.Patrol);
    }

    private void Update()
    {
        Transform targetToLook = chestPoint != null ? chestPoint : transform;

        bool isBeingLookedAt = playerLoS.isInRange(playerCamera, targetToLook) &&
                               playerLoS.isInAngle(playerCamera, targetToLook) &&
                               playerLoS.hasLineOfSight(playerCamera, targetToLook);

        UpdateState(isBeingLookedAt);
    }

    public void ChangeState(EnemyState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Patrol:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                break;
            case EnemyState.Pursuit:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                lastKnownPosition = player.position;
                break;
            case EnemyState.Search:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                agent.SetDestination(lastKnownPosition);
                break;
            case EnemyState.Frozen:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                break;
            case EnemyState.Attack:
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
        }
    }

    private void UpdateState(bool isBeingLookedAt)
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                if (isBeingLookedAt) { ChangeState(EnemyState.Frozen); return; }

                wanderTimer -= Time.deltaTime;
                if (wanderTimer <= 0f)
                {
                    Vector3 randomDirection = Random.insideUnitSphere * 10f;
                    randomDirection += transform.position;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomDirection, out hit, 10f, 1))
                    {
                        agent.SetDestination(hit.position);
                    }
                    wanderTimer = 3f;
                }

                if (Vector3.Distance(transform.position, player.position) < 8f)
                    ChangeState(EnemyState.Pursuit);
                break;

            case EnemyState.Pursuit:
                if (isBeingLookedAt) { ChangeState(EnemyState.Frozen); return; }

                float distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer <= killDistance)
                {
                    ChangeState(EnemyState.Attack);
                    return;
                }

                if (distanceToPlayer > 15f)
                {
                    ChangeState(EnemyState.Search);
                    return;
                }

                lastKnownPosition = player.position;
                agent.SetDestination(player.position);
                break;

            case EnemyState.Search:
                if (isBeingLookedAt) { ChangeState(EnemyState.Frozen); return; }

                if (Vector3.Distance(transform.position, player.position) < 8f)
                {
                    ChangeState(EnemyState.Pursuit);
                    return;
                }

                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    ChangeState(EnemyState.Patrol);
                }
                break;

            case EnemyState.Frozen:
                if (!isBeingLookedAt)
                {
                    if (Vector3.Distance(transform.position, player.position) < 12f)
                        ChangeState(EnemyState.Pursuit);
                    else
                        ChangeState(EnemyState.Search);
                }
                break;

            case EnemyState.Attack:
                break;
        }
    }
}