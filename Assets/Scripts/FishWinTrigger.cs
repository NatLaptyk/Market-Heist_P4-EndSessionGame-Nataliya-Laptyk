using UnityEngine;

// The win condition — interacting with this fish triggers SetFishStolen, which fires
// OnGameWon on GameManager and shows the win screen.

// Gated: only interactable if chef is persuaded OR chef is dead. Otherwise displays a
// "watched" prompt to communicate the gate to the player.

[RequireComponent(typeof(Collider))]
public class FishWinTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string m_unlockedPromptText = "[E] Steal Fish";
    [SerializeField] private string m_lockedPromptText = "The chef is watching...";

    [Header("Chef Reference")]
    [SerializeField] private Health m_chefHealth; // for "is chef dead?" check

    public string PromptText()
    {
        return CanInteract() ? m_unlockedPromptText : m_lockedPromptText;
    }

    public bool CanInteract()
    {
        if (GameManager.Instance == null) return false;

        // Win path 1: chef persuaded through dialogue
        if (GameManager.Instance.ChefCatPersuaded) return true;

        // Win path 2: chef defeated through combat
        if (m_chefHealth != null && m_chefHealth.IsDead) return true;

        return false;
    }

    public void TryInteract()
    {
        if (!CanInteract()) return;
        if (GameManager.Instance == null) return;

        GameManager.Instance.SetFishStolen(true);

        // Optional visual: hide the fish object after grabbing
        gameObject.SetActive(false);
    }
}