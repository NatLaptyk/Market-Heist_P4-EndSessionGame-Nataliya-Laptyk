using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD subscribes to GameManager events. No direct reference to player or enemies.
// Lives on an always-active GameObject (the HUD_Canvas itself) so it never misses events
// — the death/win panels start inactive but the controller does not.

public class HUDController : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private Slider m_healthBar;
    [SerializeField] private TMP_Text m_itemCounterText;
    [SerializeField] private TMP_Text m_interactionPromptText;

    [Header("Screens")]
    [SerializeField] private GameObject m_deathScreen;
    [SerializeField] private GameObject m_winScreen;

    private void Awake()
    {
        if (m_healthBar == null)
        {
            Debug.LogError($"{nameof(HUDController)}: Health Bar not assigned.");
        }
        if (m_itemCounterText == null)
        {
            Debug.LogError($"{nameof(HUDController)}: Item Counter Text not assigned.");
        }
        if (m_interactionPromptText == null)
        {
            Debug.LogError($"{nameof(HUDController)}: Interaction Prompt Text not assigned.");
        }

        // Hide screens at startup — they'll appear via events
        if (m_deathScreen != null) m_deathScreen.SetActive(false);
        if (m_winScreen != null) m_winScreen.SetActive(false);

        // Clear interaction prompt at startup
        if (m_interactionPromptText != null) m_interactionPromptText.text = "";
    }

    private void Start()
    {
        // Subscribe in Start — GameManager.Instance may be null in OnEnable
        if (GameManager.Instance == null)
        {
            Debug.LogError($"{nameof(HUDController)}: GameManager.Instance is null in Start. Check execution order.");
            return;
        }

        GameManager.Instance.OnPlayerHealthChanged += OnPlayerHealthChanged;
        GameManager.Instance.OnPlayerDied += OnPlayerDied;
        GameManager.Instance.OnItemCountChanged += OnItemCountChanged;
        GameManager.Instance.OnGameWon += OnGameWon;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnPlayerHealthChanged -= OnPlayerHealthChanged;
        GameManager.Instance.OnPlayerDied -= OnPlayerDied;
        GameManager.Instance.OnItemCountChanged -= OnItemCountChanged;
        GameManager.Instance.OnGameWon -= OnGameWon;
    }

    private void OnPlayerHealthChanged(int current, int max)
    {
        if (m_healthBar == null) return;
        m_healthBar.value = (max <= 0) ? 0f : (float)current / max;
    }

    private void OnPlayerDied()
    {
        if (m_deathScreen != null) m_deathScreen.SetActive(true);
    }

    private void OnItemCountChanged(int count)
    {
        if (m_itemCounterText == null) return;
        m_itemCounterText.text = $"Coins: {count}";
    }

    private void OnGameWon()
    {
        if (m_winScreen != null) m_winScreen.SetActive(true);
    }

    // Called by InteractionDetector (Raycast system, coming later) when in/out of range
    public void ShowInteractionPrompt(string message)
    {
        if (m_interactionPromptText == null) return;
        m_interactionPromptText.text = message;
    }

    public void HideInteractionPrompt()
    {
        if (m_interactionPromptText == null) return;
        m_interactionPromptText.text = "";
    }
}