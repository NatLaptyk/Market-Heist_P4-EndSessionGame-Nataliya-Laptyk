using UnityEngine;

[CreateAssetMenu(fileName = "NewCatData", menuName = "Cats/Cat Data")]
public class CatData : ScriptableObject
{
    [SerializeField] private string m_catName = "Generic Cat";
    [SerializeField] private int m_maxHealth = 50;
    [SerializeField] private float m_moveSpeed = 3f;
    [SerializeField] private int m_attackDamage = 10;

    // Public read-only methods to access the data — keeps fields private and allows for future logic if needed
    public string CatName() { return m_catName; }
    public int MaxHealth() { return m_maxHealth; }
    public float MoveSpeed() { return m_moveSpeed; }
    public int AttackDamage() { return m_attackDamage; }
}