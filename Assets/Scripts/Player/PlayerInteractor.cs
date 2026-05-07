using UnityEngine;

// Scans for interactables in front of the player using a SphereCast each frame.
// Detects both DialogueTrigger (chef NPC) and FishWinTrigger (the win condition).
// Pattern matches Week 6 raycast interaction: detect → highlight → press E → trigger.

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float m_detectionRange = 2.5f;
    [SerializeField] private float m_detectionRadius = 0.8f;
    [SerializeField] private LayerMask m_interactableLayer;

    [Header("References")]
    [SerializeField] private PlayerInputHandler m_inputHandler;
    [SerializeField] private HUDController m_hudController;

    private object m_currentTargetRef;
    private DialogueTrigger m_currentDialogueTarget;
    private FishWinTrigger m_currentFishTarget;

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
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue) return;

        // Disable interactions after game won
        if (GameManager.Instance != null && GameManager.Instance.FishStolen)
        {
            if (m_currentTargetRef != null)
            {
                m_currentTargetRef = null;
                m_currentDialogueTarget = null;
                m_currentFishTarget = null;
                if (m_hudController != null) m_hudController.HideInteractionPrompt();
            }
            return;
        }

        ScanForInteractable();
    }

    private void ScanForInteractable()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;

        DialogueTrigger newDialogueTarget = null;
        FishWinTrigger newFishTarget = null;

        if (Physics.SphereCast(origin, m_detectionRadius, direction, out RaycastHit hit, m_detectionRange, m_interactableLayer))
        {
            Debug.Log($"[INTERACT] SphereCast hit: {hit.collider.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            if (hit.collider.TryGetComponent(out DialogueTrigger dialogueTrigger) && dialogueTrigger.CanInteract())
            {
                newDialogueTarget = dialogueTrigger;
            }
            else if (hit.collider.TryGetComponent(out FishWinTrigger fishTrigger))
            {
                newFishTarget = fishTrigger;
                Debug.Log($"[INTERACT] Fish detected, CanInteract: {fishTrigger.CanInteract()}");
            }
        }

        // Pick whichever target was found, prefer dialogue
        string newPrompt = null;
        object newTargetReference = null;

        if (newDialogueTarget != null)
        {
            newPrompt = newDialogueTarget.PromptText();
            newTargetReference = newDialogueTarget;
        }
        else if (newFishTarget != null)
        {
            newPrompt = newFishTarget.PromptText();
            newTargetReference = newFishTarget;
        }

        // Only update HUD on change
        if (newTargetReference != m_currentTargetRef)
        {
            m_currentTargetRef = newTargetReference;
            m_currentDialogueTarget = newDialogueTarget;
            m_currentFishTarget = newFishTarget;

            if (m_hudController != null)
            {
                if (newPrompt != null)
                {
                    m_hudController.ShowInteractionPrompt(newPrompt);
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
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue) return;

        if (m_currentDialogueTarget != null)
        {
            m_currentDialogueTarget.TryInteract();
        }
        else if (m_currentFishTarget != null)
        {
            m_currentFishTarget.TryInteract();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawWireSphere(origin + transform.forward * m_detectionRange, m_detectionRadius);
    }
}