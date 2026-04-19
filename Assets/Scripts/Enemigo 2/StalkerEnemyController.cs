using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class StalkerEnemyController : MonoBehaviour
{
    public enum EnemyState { Patrol, Pursue, Frozen, Attack }
    public EnemyState currentState;

    public Transform player;
    public LineOfSight enemyLoS;
    public Transform enemyEyes;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 6.5f;
    public float killDistance = 1.5f;

    public float pursueTimeLimit = 5f;
    public float freezeDuration = 5f;

    [HideInInspector] public NavMeshAgent agent;

    private float wanderTimer = 0f;
    private float pursueTimer = 0f;
    private float freezeTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ChangeState(EnemyState.Patrol);
    }

    private void Update()
    {
        UpdateState();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Patrol:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                wanderTimer = 0f;
                break;
            case EnemyState.Pursue:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                pursueTimer = pursueTimeLimit;
                break;
            case EnemyState.Frozen:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                freezeTimer = freezeDuration;
                break;
            case EnemyState.Attack:
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
        }
    }

    private void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (CheckLineOfSight())
                {
                    ChangeState(EnemyState.Pursue);
                    return;
                }

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
                break;

            case EnemyState.Pursue:
                if (distanceToPlayer <= killDistance)
                {
                    ChangeState(EnemyState.Attack);
                    return;
                }

                pursueTimer -= Time.deltaTime;
                if (pursueTimer <= 0f)
                {
                    ChangeState(EnemyState.Frozen);
                    return;
                }

                agent.SetDestination(player.position);
                break;

            case EnemyState.Frozen:
                freezeTimer -= Time.deltaTime;
                if (freezeTimer <= 0f)
                {
                    ChangeState(EnemyState.Patrol);
                }
                break;

            case EnemyState.Attack:
                break;
        }
    }

    private bool CheckLineOfSight()
    {
        if (enemyLoS == null || enemyEyes == null || player == null) return false;

        return enemyLoS.isInRange(enemyEyes, player) &&
               enemyLoS.isInAngle(enemyEyes, player) &&
               enemyLoS.hasLineOfSight(enemyEyes, player);
    }
}