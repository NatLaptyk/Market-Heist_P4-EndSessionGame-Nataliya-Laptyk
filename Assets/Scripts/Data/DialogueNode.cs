using UnityEngine;

// One conversation node = one .asset. Each node holds the speaker's text and a list of
// choices. Each choice can lead to another DialogueNode (or null = end of conversation).
//
// Choices can be conditionally locked behind world state: requiredCoins for the bribe
// option, requiresChefHostile to gate the "fight" option after persuasion succeeded, etc.
//
// Side effects (setting flags like ChefCatPersuaded) are triggered by the choice itself,
// not by the node, so a node can be reused in multiple branches.

[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    public enum DialogueOutcome
    {
        None,
        PersuadeChef,   // sets ChefCatPersuaded = true
        BribeChef,      // sets ChefCatPersuaded = true AND deducts coins
        StartCombat,    // closes dialogue, makes chef hostile
        EndConversation // just closes the dialogue panel
    }

    [System.Serializable]
    public class DialogueChoice
    {
        [SerializeField] private string m_choiceText;
        [SerializeField] private DialogueNode m_nextNode;
        [SerializeField] private DialogueOutcome m_outcome = DialogueOutcome.None;
        [SerializeField] private int m_requiredCoins = 0;

        public string ChoiceText() { return m_choiceText; }
        public DialogueNode NextNode() { return m_nextNode; }
        public DialogueOutcome Outcome() { return m_outcome; }
        public int RequiredCoins() { return m_requiredCoins; }
    }

    [SerializeField] private string m_speakerName;
    [SerializeField] [TextArea(3, 6)] private string m_bodyText;
    [SerializeField] private DialogueChoice[] m_choices;

    public string SpeakerName() { return m_speakerName; }
    public string BodyText() { return m_bodyText; }
    public DialogueChoice[] Choices() { return m_choices; }
}