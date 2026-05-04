using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Subscribes to DialogueManager events and renders the current node. Spawns choice
// buttons dynamically from the ChoiceButton prefab; bribe choices are hidden if the
// player doesn't have enough coins.
//
// Lives on DialoguePanel itself, but DialoguePanel starts inactive. To avoid the
// "scripts on inactive GameObjects miss events" rule, this script lives on the always-
// active HUD_Canvas root and TOGGLES the panel GameObject on/off via SetActive.

public class DialoguePanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject m_panelRoot;

    [Header("Text")]
    [SerializeField] private TMP_Text m_speakerNameText;
    [SerializeField] private TMP_Text m_bodyText;

    [Header("Choices")]
    [SerializeField] private Transform m_choicesContainer;
    [SerializeField] private GameObject m_choiceButtonPrefab;

    private void Awake()
    {
        if (m_panelRoot == null) Debug.LogError($"{nameof(DialoguePanel)}: Panel Root not assigned.");
        if (m_speakerNameText == null) Debug.LogError($"{nameof(DialoguePanel)}: Speaker Name Text not assigned.");
        if (m_bodyText == null) Debug.LogError($"{nameof(DialoguePanel)}: Body Text not assigned.");
        if (m_choicesContainer == null) Debug.LogError($"{nameof(DialoguePanel)}: Choices Container not assigned.");
        if (m_choiceButtonPrefab == null) Debug.LogError($"{nameof(DialoguePanel)}: Choice Button Prefab not assigned.");

        if (m_panelRoot != null) m_panelRoot.SetActive(false);
    }

    private void Start()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError($"{nameof(DialoguePanel)}: DialogueManager.Instance is null in Start.");
            return;
        }

        DialogueManager.Instance.OnNodeChanged += OnNodeChanged;
        DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance == null) return;

        DialogueManager.Instance.OnNodeChanged -= OnNodeChanged;
        DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
    }

    private void OnNodeChanged(DialogueNode node)
    {
        if (node == null) return;

        if (m_panelRoot != null) m_panelRoot.SetActive(true);

        if (m_speakerNameText != null) m_speakerNameText.text = node.SpeakerName();
        if (m_bodyText != null) m_bodyText.text = node.BodyText();

        RebuildChoices(node);
    }

    private void OnDialogueEnded()
    {
        if (m_panelRoot != null) m_panelRoot.SetActive(false);
        ClearChoices();
    }

    private void RebuildChoices(DialogueNode node)
{
    ClearChoices();

    DialogueNode.DialogueChoice[] choices = node.Choices();
    if (choices == null) return;

    Debug.Log($"[REBUILD] Building {choices.Length} choice buttons for node '{node.SpeakerName()}'");

    for (int i = 0; i < choices.Length; i++)
    {
        DialogueNode.DialogueChoice choice = choices[i];

        int playerCoins = GameManager.Instance != null ? GameManager.Instance.ItemCount : 0;
        if (choice.RequiredCoins() > 0 && playerCoins < choice.RequiredCoins())
        {
            Debug.Log($"[REBUILD] Skipping choice {i} (requires {choice.RequiredCoins()} coins, player has {playerCoins})");
            continue;
        }

        GameObject buttonObj = Instantiate(m_choiceButtonPrefab, m_choicesContainer);
        Button button = buttonObj.GetComponentInChildren<Button>();
        TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();

        Debug.Log($"[REBUILD] Created button {i}: button={button}, text={buttonText}");

        if (buttonText != null) buttonText.text = choice.ChoiceText();

        int capturedIndex = i;
        if (button != null)
        {
            button.onClick.AddListener(() => OnChoiceClicked(capturedIndex));
            Debug.Log($"[REBUILD] Listener added to button {i}");
        }
        else
        {
            Debug.LogError($"[REBUILD] Button component is NULL on instantiated prefab!");
        }
        // Force layout rebuild — Content Size Fitter doesn't update reliably when children
        // are destroyed and recreated in the same frame, especially with Time.timeScale = 0.
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_choicesContainer.GetComponent<RectTransform>());
    }
}

    private void ClearChoices()
    {
        if (m_choicesContainer == null) return;

        for (int i = m_choicesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(m_choicesContainer.GetChild(i).gameObject);
        }
    }

    private void OnChoiceClicked(int index)
    {
        Debug.Log($"[DIALOGUE] Choice clicked: {index}");
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ChooseOption(index);
        }
        else
        {
            Debug.LogError("[DIALOGUE] DialogueManager.Instance is null!");
        }
    }
}