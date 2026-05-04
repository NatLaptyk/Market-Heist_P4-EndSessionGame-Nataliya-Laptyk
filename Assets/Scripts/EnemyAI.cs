using UnityEngine;
using UnityEngine.AI;

// AI uses enum-based state machine: Patrol -> Chase -> Attack -> Chase -> Patrol.
// Distance checks in EvaluateState() decide which state runs each frame.
// Movement handled by NavMeshAgent (Week 12 pattern) — agent.SetDestination does the
// pathfinding, we just tell it where to go.
//
// Each enemy holds a reference to a CatData ScriptableObject for stats.
// Animator hooks: Speed float drives Idle/Trot/Run blend; Attack/TakeDamage triggers fire on actions.

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("Data")]
    [SerializeField] private CatData m_catData;

    [Header("Patrol")]
    [SerializeField] private Transform[] m_waypoints;
    [SerializeField] private float m_waypointAccuracy = 0.5f;

    [Header("Detection (overrides CatData if needed)")]
    [SerializeField] private float m_chaseRange = 6f;
    [SerializeField] private float m_attackRange = 1.5f;
    [SerializeField] private float m_attackCooldown = 1.5f;

    [Header("References")]
    [SerializeField] private Transform m_player;
    [SerializeField] private Animator m_animator;

    private NavMeshAgent m_agent;
    private Health m_health;
    private EnemyState m_state = EnemyState.Patrol;
    private int m_currentWaypoint;
    private float m_lastAttackTime;

    private void Awake()
    {
        if (!TryGetComponent(out m_agent))
        {
            Debug.LogError($"{nameof(EnemyAI)}: NavMeshAgent missing on {gameObject.name}.");
        }

        if (!TryGetComponent(out m_health))
        {
            Debug.LogError($"{nameof(EnemyAI)}: Health missing on {gameObject.name}.");
        }

        if (m_animator == null)
        {
            TryGetComponent(out m_animator);
        }

        if (m_catData == null)
        {
            Debug.LogWarning($"{nameof(EnemyAI)}: CatData not assigned on {gameObject.name}. Using default values.");
        }
    }

    private void Start()
    {
        // Apply CatData values if assigned
        if (m_catData != null)
        {
            m_agent.speed = m_catData.MoveSpeed();
            m_health.SetHealth(m_catData.MaxHealth());
        }

        // Auto-find player if not assigned
        if (m_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) m_player = playerObj.transform;
        }

        // Subscribe to own death event
        m_health.OnDied += OnDied;

        // Start patrolling toward first waypoint
        if (m_waypoints != null && m_waypoints.Length > 0 && m_agent.isOnNavMesh)
        {
            m_agent.SetDestination(m_waypoints[0].position);
        }
    }

    private void OnDisable()
    {
        if (m_health != null)
        {
            m_health.OnDied -= OnDied;
        }
    }

    private void Update()
    {
        // Guards from teacher's slides
        if (!m_agent.isOnNavMesh) return;
        if (m_health.IsDead) return;
        if (m_player == null) return;

        EvaluateState();

        switch (m_state)
        {
            case EnemyState.Patrol:
                PatrolUpdate();
                break;
            case EnemyState.Chase:
                ChaseUpdate();
                break;
            case EnemyState.Attack:
                AttackUpdate();
                break;
        }

        UpdateAnimator();
    }

    private void EvaluateState()
    {
        float dist = Vector3.Distance(transform.position, m_player.position);

        // Order matters — tightest range first
        if (dist < m_attackRange) m_state = EnemyState.Attack;
        else if (dist < m_chaseRange) m_state = EnemyState.Chase;
        else m_state = EnemyState.Patrol;
    }

    private void PatrolUpdate()
    {
        if (m_waypoints == null || m_waypoints.Length == 0) return;
        if (m_agent.pathPending) return;

        if (m_agent.remainingDistance <= m_waypointAccuracy)
        {
            m_currentWaypoint = (m_currentWaypoint + 1) % m_waypoints.Length;
            m_agent.SetDestination(m_waypoints[m_currentWaypoint].position);
        }
    }

    private void ChaseUpdate()
    {
        m_agent.SetDestination(m_player.position);
    }

    private void AttackUpdate()
    {
        // Stop moving and face the player
        m_agent.ResetPath();

        Vector3 lookDir = m_player.position - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 8f * Time.deltaTime);
        }

        // Cooldown gate
        if (Time.time - m_lastAttackTime < m_attackCooldown) return;
        m_lastAttackTime = Time.time;

        // Trigger animation
        if (m_animator != null) m_animator.SetTrigger("Attack");

        // Deal damage to player
        if (m_player.TryGetComponent(out Health playerHealth))
        {
            int damage = m_catData != null ? m_catData.AttackDamage() : 10;
            playerHealth.TakeDamage(damage);
        }
    }

    private void UpdateAnimator()
    {
        if (m_animator == null) return;
        m_animator.SetFloat("Speed", m_agent.velocity.magnitude);
    }

    private void OnDied()
    {
        // Stop the agent and disable AI behavior
        if (m_agent != null && m_agent.isOnNavMesh) m_agent.ResetPath();
        if (m_animator != null) m_animator.SetTrigger("Die");

        // Disable this AI script — corpse stays in scene but stops thinking
        enabled = false;

        // Optional: disable the agent so player can walk through the corpse
        if (m_agent != null) m_agent.enabled = false;
    }

    // Editor visualization for ranges — yellow for chase, red for attack
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_attackRange);
    }
}