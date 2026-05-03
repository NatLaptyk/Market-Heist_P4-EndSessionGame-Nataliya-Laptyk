using UnityEngine;

// Singleton. Owns global game state and broadcasts events.

// Events approach rationale:
// C# events used (not UnityEvents) for inter-script communication. C# events are
// type-safe at compile time, decoupled from the Inspector (no risk of a wire breaking
// when scenes change), and ~3x faster than UnityEvents. UnityEvents would be appropriate
// for designer-facing hooks (e.g., wiring an audio clip to a door open event in the
// Inspector), but this prototype has no such hooks — every subscriber is a script.

// GameManager is the relay: PlayerHealth fires local events, GameManager listens and
// re-broadcasts for the HUD. The HUD never has a direct reference to the player,
// it only knows about GameManager.

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Health m_playerHealth;

    // World state
    private int m_itemCount;
    private bool m_chefCatPersuaded;
    private bool m_fishStolen;

    public int ItemCount => m_itemCount;
    public bool ChefCatPersuaded => m_chefCatPersuaded;
    public bool FishStolen => m_fishStolen;

    // Events the HUD and other systems subscribe to
    public event System.Action<int, int> OnPlayerHealthChanged; // (current, max)
    public event System.Action OnPlayerDied;
    public event System.Action<int> OnItemCountChanged;         // new count
    public event System.Action OnGameWon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (m_playerHealth == null)
        {
            Debug.LogError($"{nameof(GameManager)}: PlayerHealth reference not assigned in Inspector.");
        }
    }

    private void Start()
    {
        // Subscribe in Start (not OnEnable) — Awake order is not guaranteed, and other
        // scripts may rely on Instance being set before they wake.
        if (m_playerHealth != null)
        {
            m_playerHealth.OnHealthChanged += OnPlayerHealthChangedInternal;
            m_playerHealth.OnDied += OnPlayerDiedInternal;
        }
    }

    private void OnDisable()
    {
        if (m_playerHealth != null)
        {
            m_playerHealth.OnHealthChanged -= OnPlayerHealthChangedInternal;
            m_playerHealth.OnDied -= OnPlayerDiedInternal;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnPlayerHealthChangedInternal(int current, int max)
    {
        OnPlayerHealthChanged?.Invoke(current, max);
    }

    private void OnPlayerDiedInternal()
    {
        OnPlayerDied?.Invoke();
    }

    // Called by ItemPickup when player picks up a collectible
    public void AddItem(int amount = 1)
    {
        m_itemCount += amount;
        OnItemCountChanged?.Invoke(m_itemCount);
    }

    // Called by DialogueManager when the player successfully persuades the chef
    public void SetChefCatPersuaded(bool value)
    {
        m_chefCatPersuaded = value;
    }

    // Called when the player picks up the fish — triggers the win
    public void SetFishStolen(bool value)
    {
        m_fishStolen = value;
        if (value) OnGameWon?.Invoke();
    }

    // Save/Load helper — restore world state without re-firing the win event
    public void RestoreWorldState(int itemCount, bool chefCatPersuaded, bool fishStolen)
    {
        m_itemCount = itemCount;
        m_chefCatPersuaded = chefCatPersuaded;
        m_fishStolen = fishStolen;
        OnItemCountChanged?.Invoke(m_itemCount);
    }

    [ContextMenu("Test: Add Item")]
    private void TestAddItem()
    {
        AddItem(1);
    }

    [ContextMenu("Test: Damage Player 10")]
    private void TestDamagePlayer()
    {
        m_playerHealth?.TakeDamage(10);
    }

    [ContextMenu("Test: Heal Player 20")]
    private void TestHealPlayer()
    {
        m_playerHealth?.Heal(20);
    }

    [ContextMenu("Test: Persuade Chef")]
    private void TestPersuadeChef()
    {
        SetChefCatPersuaded(true);
    }

    [ContextMenu("Test: Steal Fish (Win)")]
    private void TestStealFish()
    {
        SetFishStolen(true);
    }
}