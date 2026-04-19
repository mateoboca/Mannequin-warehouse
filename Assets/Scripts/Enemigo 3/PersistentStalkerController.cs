using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class PersistentStalkerController : MonoBehaviour
{
    public enum EnemyState { Tracking, Chasing, Attack }
    public EnemyState currentState;

    [Header("Referencias")]
    public Transform player;
    public LineOfSight enemyLoS;
    public Transform enemyEyes;

    [Header("Configuración")]
    public float trackingSpeed = 1.5f;
    public float chaseSpeed = 4.5f;
    public float killDistance = 1.2f;

    [HideInInspector] public NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ChangeState(EnemyState.Tracking);
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
            case EnemyState.Tracking:
                agent.speed = trackingSpeed;
                agent.isStopped = false;
                break;
            case EnemyState.Chasing:
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                break;
            case EnemyState.Attack:
                agent.isStopped = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
        }
    }

    private void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= killDistance)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        bool canSeePlayer = CheckLineOfSight();

        switch (currentState)
        {
            case EnemyState.Tracking:
                if (canSeePlayer)
                {
                    ChangeState(EnemyState.Chasing);
                }
                agent.SetDestination(player.position);
                break;

            case EnemyState.Chasing:
                if (!canSeePlayer)
                {
                    ChangeState(EnemyState.Tracking);
                }
                agent.SetDestination(player.position);
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