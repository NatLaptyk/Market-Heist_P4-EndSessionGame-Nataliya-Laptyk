using UnityEngine;

// Singleton. Holds the current dialogue node and broadcasts events when the node changes
// or when the conversation ends. The UI listens; the world (DialogueTrigger) starts
// conversations; the GameManager listens for outcomes (persuaded, bribed, combat).
//
// Pause behavior: When dialogue is active we set Time.timeScale = 0. UI animations and
// inputs continue (Time.unscaledDeltaTime), but enemy AI, gravity, and pickups freeze.
// We restore timeScale to 1 when dialogue ends.

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private DialogueNode m_currentNode;
    private DialogueTrigger m_currentTrigger; // who started this conversation

    public DialogueNode CurrentNode => m_currentNode;
    public bool IsInDialogue => m_currentNode != null;

    [Header("Testing")]
    [SerializeField] private DialogueNode m_testRootNode;

    [ContextMenu("Test: Start Dialogue")]
    private void TestStartDialogue()
    {
        StartDialogue(m_testRootNode, null);
    }   

    // Events
    public event System.Action<DialogueNode> OnNodeChanged; // fires when a new node becomes active
    public event System.Action OnDialogueEnded;             // fires when conversation closes

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void StartDialogue(DialogueNode rootNode, DialogueTrigger trigger)
    {
        if (rootNode == null)
        {
            Debug.LogError($"{nameof(DialogueManager)}: Cannot start dialogue with null root node.");
            return;
        }

        m_currentTrigger = trigger;
        Time.timeScale = 0f;

        // Unlock cursor so player can click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetNode(rootNode);
    }

    public void ChooseOption(int choiceIndex)
{
    if (m_currentNode == null) return;

    DialogueNode.DialogueChoice[] choices = m_currentNode.Choices();
    if (choiceIndex < 0 || choiceIndex >= choices.Length)
    {
        Debug.LogError($"{nameof(DialogueManager)}: Invalid choice index {choiceIndex}.");
        return;
    }

    DialogueNode.DialogueChoice choice = choices[choiceIndex];

    // Apply outcome — returns true if dialogue should end immediately
    bool dialogueEndedByOutcome = ApplyOutcome(choice.Outcome(), choice.RequiredCoins());
    if (dialogueEndedByOutcome) return;

    // Move to next node, or end conversation if no next
    DialogueNode next = choice.NextNode();
    if (next == null)
    {
        EndDialogue();
    }
    else
    {
        SetNode(next);
    }
}
    private bool ApplyOutcome(DialogueNode.DialogueOutcome outcome, int requiredCoins)
{
        if (GameManager.Instance == null) return false;

        if (outcome == DialogueNode.DialogueOutcome.PersuadeChef)
        {
            GameManager.Instance.SetChefCatPersuaded(true);
            return false;
        }
        else if (outcome == DialogueNode.DialogueOutcome.BribeChef)
        {
            if (GameManager.Instance.ItemCount >= requiredCoins)
            {
                GameManager.Instance.AddItem(-requiredCoins);
                GameManager.Instance.SetChefCatPersuaded(true);
            }
            return false;
        }
        else if (outcome == DialogueNode.DialogueOutcome.StartCombat)
        {
            if (m_currentTrigger != null) m_currentTrigger.MakeChefHostile();
            EndDialogue();
            return true; // dialogue is over, don't process next node
        }
        else if (outcome == DialogueNode.DialogueOutcome.EndConversation)
        {
            EndDialogue();
            return true;
        }

        return false;
}

    // Helper for the switch — C# doesn't allow function calls in case labels
    private DialogueNode.DialogueOutcome DialogueOutcome_Persuade()
    {
        return DialogueNode.DialogueOutcome.PersuadeChef;
    }

    private void SetNode(DialogueNode node)
    {
        m_currentNode = node;
        OnNodeChanged?.Invoke(node);
    }

    private void EndDialogue()
    {
        m_currentNode = null;
        m_currentTrigger = null;
        Time.timeScale = 1f;

        // Re-lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnDialogueEnded?.Invoke();
    }

    [ContextMenu("Test: Force End Dialogue")]
    private void TestForceEnd()
    {
        EndDialogue();
    }
}