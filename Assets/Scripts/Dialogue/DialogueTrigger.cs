using UnityEngine;

// Lives on a dialogue NPC (chef cat). Holds a root DialogueNode reference.
// PlayerInteractor detects this component via raycast and calls TryInteract() when the
// player presses Interact.
//
// Implements a simple IInteractable contract via the public method — no interface needed,
// PlayerInteractor just calls TryGetComponent<DialogueTrigger> on whatever it hits.

[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueNode m_rootNode;
    [SerializeField] private string m_promptText = "[E] Talk";

    [Header("Hostility")]
    [SerializeField] private EnemyAI m_enemyAI;
    [SerializeField] private MonoBehaviour m_friendlyBehavior; // optional: any peaceful idle script

    public string PromptText() { return m_promptText; }

    public bool CanInteract()
    {
        // Cannot talk to the chef if they've gone hostile or are dead
        if (m_enemyAI != null && m_enemyAI.enabled) return false;
        if (TryGetComponent(out Health health) && health.IsDead) return false;
        return m_rootNode != null;
    }

    public void TryInteract()
    {
        if (!CanInteract()) return;
        if (DialogueManager.Instance == null)
        {
            Debug.LogError($"{nameof(DialogueTrigger)}: DialogueManager.Instance is null.");
            return;
        }

        DialogueManager.Instance.StartDialogue(m_rootNode, this);
    }

    // Called by DialogueManager when the player picks a "fight" choice
    public void MakeChefHostile()
    {
        if (m_enemyAI != null) m_enemyAI.enabled = true;
        if (m_friendlyBehavior != null) m_friendlyBehavior.enabled = false;
    }
}