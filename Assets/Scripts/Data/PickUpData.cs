using UnityEngine;

[CreateAssetMenu(fileName = "NewPickupData", menuName = "Pickups/Pickup Data")]
public class PickupData : ScriptableObject
{
    [SerializeField] private string m_displayName = "Coin";
    [SerializeField] private int m_amount = 1;

    public string DisplayName() { return m_displayName; }
    public int Amount() { return m_amount; }
}