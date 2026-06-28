using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class StalkerEnemyController : MonoBehaviour
{
    public enum EnemyState { Patrol, Pursue, Frozen, Attack }
    public EnemyState currentState;

    [Header("Referencias")]
    public Transform player;
    public LineOfSight enemyLoS;
    public Transform enemyEyes;

    [Header("Configuración")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 6.5f;
    public float killDistance = 1.5f;
    public float pursueTimeLimit = 5f;
    public float freezeDuration = 5f;

    [Header("Pathfinding A*")]
    [Tooltip("Radio para buscar nodos cercanos")]
    public float nodeSearchRadius = 15f;
    [Tooltip("Cada cuántos segundos recalcular A* cuando no ve al jugador")]
    public float pathRecalculateInterval = 0.5f;

    [HideInInspector] public NavMeshAgent agent;

    private float wanderTimer = 0f;
    private float pursueTimer = 0f;
    private float freezeTimer = 0f;

    private List<Vector3> currentPath = new List<Vector3>();
    private int waypointIndex = 0;
    private float recalculateTimer = 0f;

    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        ChangeState(EnemyState.Patrol);
    }

    private void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude * 2f);
        UpdateState();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState = newState;
        animator.SetBool("Frozen", false);
        currentPath.Clear();
        waypointIndex = 0;

        switch (currentState)
        {
            case EnemyState.Patrol:
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                wanderTimer = 0f;
                MoveToRandomNode();
                break;

            case EnemyState.Pursue:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                pursueTimer = pursueTimeLimit;
                recalculateTimer = 0f;
                break;

            case EnemyState.Frozen:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                freezeTimer = freezeDuration;
                animator.SetBool("Frozen", true);
                break;

            case EnemyState.Attack:
                agent.isStopped = true;
                animator.SetTrigger("Attack");
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
                if (wanderTimer <= 0f || IsPathFinished())
                {
                    MoveToRandomNode();
                    wanderTimer = 4f;
                }

                FollowPath();
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

                if (CheckLineOfSight())
                {
                    currentPath.Clear();
                    agent.SetDestination(player.position);
                }
                else
                {
                    recalculateTimer -= Time.deltaTime;
                    if (recalculateTimer <= 0f)
                    {
                        RecalculatePath(player.position);
                        recalculateTimer = pathRecalculateInterval;
                    }
                    FollowPath();
                }
                break;

            case EnemyState.Frozen:
                freezeTimer -= Time.deltaTime;
                if (freezeTimer <= 0f)
                    ChangeState(EnemyState.Patrol);
                break;

            case EnemyState.Attack:
                break;
        }
    }

    private void RecalculatePath(Vector3 destination)
    {
        Node startNode = GetClosestNode(transform.position);
        Node goalNode = GetClosestNode(destination);

        if (startNode == null || goalNode == null)
        {
            agent.SetDestination(destination);
            return;
        }

        List<Node> nodePath = AStarPathfinder.Run(
            startNode,
            node => node == goalNode,
            node => node.neightbourds,
            (a, b) => Vector3.Distance(a.transform.position, b.transform.position),
            node => Vector3.Distance(node.transform.position, goalNode.transform.position)
        );

        currentPath.Clear();
        waypointIndex = 0;

        if (nodePath == null || nodePath.Count == 0)
        {
            agent.SetDestination(destination);
            return;
        }

        foreach (Node n in nodePath)
            currentPath.Add(n.transform.position);

        currentPath.Add(destination);
        agent.SetDestination(currentPath[waypointIndex]);
    }

    private void FollowPath()
    {
        if (currentPath.Count == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waypointIndex++;

            if (waypointIndex >= currentPath.Count)
            {
                currentPath.Clear();
                return;
            }

            agent.SetDestination(currentPath[waypointIndex]);
        }
    }

    private void MoveToRandomNode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, nodeSearchRadius * 3f, LayerMask.GetMask("Node"));

        if (colliders.Length == 0)
        {
            Vector3 randomDir = Random.insideUnitSphere * 10f + transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, 10f, 1))
                agent.SetDestination(hit.position);
            return;
        }

        int randomIndex = Random.Range(0, colliders.Length);
        Node targetNode = colliders[randomIndex].GetComponent<Node>();
        if (targetNode == null) return;

        RecalculatePath(targetNode.transform.position);
    }

    private bool IsPathFinished()
    {
        return currentPath.Count == 0 ||
               (waypointIndex >= currentPath.Count && !agent.pathPending && agent.remainingDistance < 0.5f);
    }

    private Node GetClosestNode(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, nodeSearchRadius, LayerMask.GetMask("Node"));

        Node closest = null;
        float nearestDist = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            Node node = col.GetComponent<Node>();
            if (node == null) continue;

            float dist = Vector3.Distance(position, col.transform.position);
            if (dist >= nearestDist) continue;

            Vector3 dir = col.transform.position - position;
            if (Physics.Raycast(position, dir.normalized, dist, LayerMask.GetMask("Obstaculos"))) continue;

            nearestDist = dist;
            closest = node;
        }

        return closest;
    }

    private bool CheckLineOfSight()
    {
        if (enemyLoS == null || enemyEyes == null || player == null) return false;

        return enemyLoS.isInRange(enemyEyes, player) &&
               enemyLoS.isInAngle(enemyEyes, player) &&
               enemyLoS.hasLineOfSight(enemyEyes, player);
    }
}