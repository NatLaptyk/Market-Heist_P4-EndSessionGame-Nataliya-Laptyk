using UnityEngine;

// Generic health component used by both Player and Enemy.
// Fires its own local C# events. The Player will have a PlayerHealth wrapper that
// forwards these into GameManager events for the HUD; enemies handle them locally.

public class Health : MonoBehaviour
{
    [SerializeField] private int m_maxHealth = 100;

    private int m_currentHealth;

    public int CurrentHealth => m_currentHealth;
    public int MaxHealth => m_maxHealth;
    public bool IsDead => m_currentHealth <= 0;

    public event System.Action<int, int> OnHealthChanged; // (current, max)
    public event System.Action OnDied;

    private void Awake()
    {
        m_currentHealth = m_maxHealth;
    }

    private void Start()
    {
        // Fire initial value so any listeners (HUD) can sync
        OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        m_currentHealth -= amount;
        if (m_currentHealth < 0) m_currentHealth = 0;

        OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);

        if (m_currentHealth == 0)
        {
            OnDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        m_currentHealth += amount;
        if (m_currentHealth > m_maxHealth) m_currentHealth = m_maxHealth;

        OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
    }

    // Used by Save/Load to restore health without firing OnDied logic
    public void SetHealth(int value)
    {
        m_currentHealth = Mathf.Clamp(value, 0, m_maxHealth);
        OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
    }

    [ContextMenu("Test: Take 10 Damage")]
    private void TestDamage()
    {
        TakeDamage(10);
    }

    [ContextMenu("Test: Heal 10")]
    private void TestHeal()
    {
        Heal(10);
    }
}