using UnityEngine;

// Visual feedback for pickups and interactables: continuous rotation + emission glow.
// Drop on any GameObject with a Renderer. Detaches gracefully if no Renderer is found
// (rotation still works).

public class InteractableHighlight : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private bool m_rotate = true;
    [SerializeField] private Vector3 m_rotationAxis = Vector3.up;
    [SerializeField] private float m_rotationSpeed = 90f; // degrees per second

    [Header("Bobbing (optional)")]
    [SerializeField] private bool m_bob = false;
    [SerializeField] private float m_bobAmplitude = 0.1f;
    [SerializeField] private float m_bobSpeed = 2f;

    [Header("Glow")]
    [SerializeField] private bool m_glow = true;
    [SerializeField] private Color m_glowColor = new Color(1f, 0.85f, 0.4f); // warm gold
    [SerializeField] private float m_glowIntensity = 1.5f;
    [SerializeField] private float m_glowPulseSpeed = 2f;

    private Renderer m_renderer;
    private Material m_material;
    private Vector3 m_startPosition;

    private void Awake()
    {
        // Look for renderer on self or children (cat assets often have nested mesh)
        m_renderer = GetComponent<Renderer>();
        if (m_renderer == null) m_renderer = GetComponentInChildren<Renderer>();

        if (m_renderer != null && m_glow)
        {
            // Important: use .material (not .sharedMaterial) so we don't modify the asset
            m_material = m_renderer.material;
            m_material.EnableKeyword("_EMISSION");
        }

        m_startPosition = transform.position;
    }

    private void Update()
    {
        if (m_rotate)
        {
            transform.Rotate(m_rotationAxis * m_rotationSpeed * Time.deltaTime, Space.World);
        }

        if (m_bob)
        {
            float yOffset = Mathf.Sin(Time.time * m_bobSpeed) * m_bobAmplitude;
            transform.position = m_startPosition + Vector3.up * yOffset;
        }

        if (m_glow && m_material != null)
        {
            // Pulse the emission intensity for a subtle "alive" feel
            float pulse = (Mathf.Sin(Time.time * m_glowPulseSpeed) + 1f) * 0.5f; // 0 to 1
            float intensity = m_glowIntensity * (0.6f + pulse * 0.4f); // pulse between 60% and 100%
            m_material.SetColor("_EmissionColor", m_glowColor * intensity);
        }
    }

    private void OnDestroy()
    {
        // Clean up the instanced material to prevent memory leak
        if (m_material != null)
        {
            Destroy(m_material);
        }
    }
}