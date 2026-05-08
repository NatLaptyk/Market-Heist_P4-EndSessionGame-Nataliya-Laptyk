using UnityEngine;

// Trigger-based pickup. Player walks over it → AddItem to GameManager → destroy self.
// The pickup data (item type, amount) lives on a ScriptableObject so coins, fish,
// any future collectibles share the same script.

// Uses OnTriggerEnter — player must have a Collider AND a Rigidbody (CharacterController
// counts as the body for trigger purposes, but Unity also wants a Rigidbody to fire
// 3D triggers reliably). 

[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    [SerializeField] private PickupData m_pickupData;
    [SerializeField] private string m_pickupId; // Unique ID for save system to track collected pickups

    public string PickupId => m_pickupId;

    private void Awake()
    {
        if (m_pickupData == null)
        {
            Debug.LogError($"{nameof(Pickup)}: PickupData not assigned on {gameObject.name}.");
        }

        // Ensure the collider is set as a trigger
        if (TryGetComponent(out Collider col) && !col.isTrigger)
        {
            Debug.LogWarning($"{nameof(Pickup)}: Collider on {gameObject.name} is not a trigger. Setting it now.");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only respond to the player
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null && m_pickupData != null)
        {
            GameManager.Instance.AddItem(m_pickupData.Amount());
        }

        // Register with save system so the pickup stays gone after a load
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.NotifyPickupCollected(m_pickupId);
        }

            Destroy(gameObject);
    }
}