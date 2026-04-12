using UnityEngine;
using UnityEngine.AI; // Necesario para el NavMesh
using UnityEngine.SceneManagement;

public class MannequinController : MonoBehaviour
{
    public State CurrentState { get; private set; }

    public PatrolState patrolState;
    public PursuitState pursuitState;
    public FrozenState frozenState;
    public AttackState attackState;
    public SearchState searchState;

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
    [HideInInspector] public NavMeshAgent agent; // Usamos el agente oficial de Unity

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        patrolState = new PatrolState(this);
        pursuitState = new PursuitState(this);
        frozenState = new FrozenState(this);
        attackState = new AttackState(this);
        searchState = new SearchState(this);

        CurrentState = patrolState;
        CurrentState.Enter();
    }

    private void Update()
    {
        Transform targetToLook = chestPoint != null ? chestPoint : transform;

        bool isBeingLookedAt = playerLoS.isInRange(playerCamera, targetToLook) &&
                               playerLoS.isInAngle(playerCamera, targetToLook) &&
                               playerLoS.hasLineOfSight(playerCamera, targetToLook);

        CurrentState.UpdateState(isBeingLookedAt);
    }

    public void ChangeState(State newState)
    {
        if (CurrentState == newState) return;
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}

// =================================================================
// CLASES DE ESTADO (Usando NavMesh para esquivar paredes)
// =================================================================

public abstract class State
{
    protected MannequinController enemy;
    public State(MannequinController enemy) { this.enemy = enemy; }
    public virtual void Enter() { }
    public virtual void Exit() { }
    public abstract void UpdateState(bool isBeingLookedAt);
}

public class PatrolState : State
{
    private float wanderTimer = 0f;
    public PatrolState(MannequinController enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.agent.isStopped = false;
        enemy.agent.speed = enemy.patrolSpeed;
    }

    public override void UpdateState(bool isBeingLookedAt)
    {
        if (isBeingLookedAt) { enemy.ChangeState(enemy.frozenState); return; }

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            // Elige un punto al azar en el mapa y el NavMesh busca el camino
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection += enemy.transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 10f, 1))
            {
                enemy.agent.SetDestination(hit.position);
            }
            wanderTimer = 3f;
        }

        if (Vector3.Distance(enemy.transform.position, enemy.player.position) < 8f)
            enemy.ChangeState(enemy.pursuitState);
    }
}

public class PursuitState : State
{
    public PursuitState(MannequinController enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.agent.isStopped = false;
        enemy.agent.speed = enemy.chaseSpeed;
        enemy.lastKnownPosition = enemy.player.position;
    }

    public override void UpdateState(bool isBeingLookedAt)
    {
        if (isBeingLookedAt) { enemy.ChangeState(enemy.frozenState); return; }

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if (distanceToPlayer <= enemy.killDistance)
        {
            enemy.ChangeState(enemy.attackState);
            return;
        }

        if (distanceToPlayer > 15f)
        {
            enemy.ChangeState(enemy.searchState);
            return;
        }

        enemy.lastKnownPosition = enemy.player.position;
        // ESTA LÍNEA HACE LA MAGIA: El motor calcula cómo doblar las paredes para llegar a vos
        enemy.agent.SetDestination(enemy.player.position);
    }
}

public class SearchState : State
{
    public SearchState(MannequinController enemy) : base(enemy) { }
    public override void Enter()
    {
        enemy.agent.isStopped = false;
        enemy.agent.speed = enemy.patrolSpeed;
        enemy.agent.SetDestination(enemy.lastKnownPosition);
    }

    public override void UpdateState(bool isBeingLookedAt)
    {
        if (isBeingLookedAt) { enemy.ChangeState(enemy.frozenState); return; }

        if (Vector3.Distance(enemy.transform.position, enemy.player.position) < 8f)
        {
            enemy.ChangeState(enemy.pursuitState);
            return;
        }

        // Si ya llegó al lugar donde te vio por última vez y no estás, vuelve a patrullar
        if (!enemy.agent.pathPending && enemy.agent.remainingDistance < 0.5f)
        {
            enemy.ChangeState(enemy.patrolState);
        }
    }
}

public class FrozenState : State
{
    public FrozenState(MannequinController enemy) : base(enemy) { }

    public override void Enter()
    {
        // Frena en seco al bicho
        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;
    }

    public override void UpdateState(bool isBeingLookedAt)
    {
        if (!isBeingLookedAt)
        {
            if (Vector3.Distance(enemy.transform.position, enemy.player.position) < 12f)
                enemy.ChangeState(enemy.pursuitState);
            else
                enemy.ChangeState(enemy.searchState);
        }
    }
}

public class AttackState : State
{
    public AttackState(MannequinController enemy) : base(enemy) { }
    public override void Enter()
    {
        Debug.Log("Empiezo a atacar");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public override void UpdateState(bool isBeingLookedAt) { }
}