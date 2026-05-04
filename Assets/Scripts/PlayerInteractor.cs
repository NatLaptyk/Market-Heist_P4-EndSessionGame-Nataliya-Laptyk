using UnityEngine;

// Scans for interactables in front of the player using a SphereCast each frame.
// When a DialogueTrigger is detected, shows the HUD prompt and listens for Interact.

// Uses SphereCast instead of Raycast because the player rotates with movement, not the
// camera, because it is more forgiving than a thin line.

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float m_detectionRange = 2.5f;
    [SerializeField] private float m_detectionRadius = 0.8f;
    [SerializeField] private LayerMask m_interactableLayer;

    [Header("References")]
    [SerializeField] private PlayerInputHandler m_inputHandler;
    [SerializeField] private HUDController m_hudController;

    private DialogueTrigger m_currentTarget;

    private void Awake()
    {
        if (m_inputHandler == null) Debug.LogError($"{nameof(PlayerInteractor)}: PlayerInputHandler not assigned.");
        if (m_hudController == null) Debug.LogError($"{nameof(PlayerInteractor)}: HUDController not assigned.");
    }

    private void Start()
    {
        if (m_inputHandler != null)
        {
            m_inputHandler.OnInteractPressed += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (m_inputHandler != null)
        {
            m_inputHandler.OnInteractPressed -= OnInteract;
        }
    }

    private void Update()
    {
        // Skip detection while dialogue is active — no point scanning while paused
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue) return;

        ScanForInteractable();
    }

    private void ScanForInteractable()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;

        DialogueTrigger newTarget = null;

        if (Physics.SphereCast(origin, m_detectionRadius, direction, out RaycastHit hit, m_detectionRange, m_interactableLayer))
        {
            if (hit.collider.TryGetComponent(out DialogueTrigger trigger) && trigger.CanInteract())
            {
                newTarget = trigger;
            }
        }

        // Only update HUD on change — avoids per-frame UI churn
        if (newTarget != m_currentTarget)
        {
            m_currentTarget = newTarget;

            if (m_hudController != null)
            {
                if (m_currentTarget != null)
                {
                    m_hudController.ShowInteractionPrompt(m_currentTarget.PromptText());
                }
                else
                {
                    m_hudController.HideInteractionPrompt();
                }
            }
        }
    }

    private void OnInteract()
    {
        // Don't interact while already in dialogue (prevents re-triggering)
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue) return;
        if (m_currentTarget == null) return;

        m_currentTarget.TryInteract();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawWireSphere(origin + transform.forward * m_detectionRange, m_detectionRadius);
    }
}