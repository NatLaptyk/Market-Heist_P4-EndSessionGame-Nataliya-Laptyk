using UnityEngine;

// Melee attack: on Attack input, check for enemies within m_attackRange in front
// of the player. Damage any Health component found. No animation timing logic, fire and
// resolve immediately.

// Uses Physics.OverlapSphere because it's simpler than a sphere cast and the player
// turns to face movement direction, so "in front" maps to the player's forward.

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float m_attackRange = 2f;
    [SerializeField] private int m_attackDamage = 25;
    [SerializeField] private float m_attackCooldown = 0.6f;
    [SerializeField] private LayerMask m_enemyLayer;

    [Header("References")]
    [SerializeField] private PlayerInputHandler m_inputHandler;
    [SerializeField] private Animator m_animator;

    private float m_lastAttackTime;

    private void Awake()
    {
        if (m_inputHandler == null)
        {
            Debug.LogError($"{nameof(PlayerCombat)}: PlayerInputHandler not assigned.");
        }

        if (m_animator == null)
        {
            TryGetComponent(out m_animator);
        }
    }

    private void Start()
    {
        if (m_inputHandler != null)
        {
            m_inputHandler.OnAttackPressed += OnAttack;
        }
    }

    private void OnDisable()
    {
        if (m_inputHandler != null)
        {
            m_inputHandler.OnAttackPressed -= OnAttack;
        }
    }

    private void OnAttack()
    {
        // Cooldown gate
        if (Time.time - m_lastAttackTime < m_attackCooldown) return;
        m_lastAttackTime = Time.time;

        // Trigger animation
        if (m_animator != null) m_animator.SetTrigger("Attack");

        // Detect enemies in a sphere in front of the player
        Vector3 attackOrigin = transform.position + transform.forward * (m_attackRange * 0.5f) + Vector3.up * 0.5f;
        Collider[] hits = Physics.OverlapSphere(attackOrigin, m_attackRange * 0.5f, m_enemyLayer);
       

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out Health enemyHealth))
            {
                enemyHealth.TakeDamage(m_attackDamage);
            }
        }
    }

    // Visualize attack zone in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 attackOrigin = transform.position + transform.forward * (m_attackRange * 0.5f) + Vector3.up * 0.5f;
        Gizmos.DrawWireSphere(attackOrigin, m_attackRange * 0.5f);
    }
}