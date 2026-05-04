using UnityEngine;

// Movement uses CharacterController for clean integration with NavMesh enemies later.
// Direction is camera-relative: pressing W moves the cat away from the camera regardless
// of camera yaw. Rotation smoothly faces movement direction. Gravity is applied manually.

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private float m_rotationSpeed = 12f;
    [SerializeField] private float m_gravity = -20f;

    [Header("References")]
    [SerializeField] private PlayerInputHandler m_inputHandler;
    [SerializeField] private Transform m_cameraTransform;

    private CharacterController m_characterController;
    private Vector3 m_verticalVelocity;

    private void Awake()
    {
        if (!TryGetComponent(out m_characterController))
        {
            Debug.LogError($"{nameof(PlayerController)}: CharacterController missing on {gameObject.name}.");
        }

        if (m_inputHandler == null)
        {
            Debug.LogError($"{nameof(PlayerController)}: PlayerInputHandler reference not assigned.");
        }

        if (m_cameraTransform == null && Camera.main != null)
        {
            m_cameraTransform = Camera.main.transform;
            Debug.LogWarning($"{nameof(PlayerController)}: Camera not assigned, falling back to Camera.main.");
        }
    }
    private void Start() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    private void Update()
    {
        Move();
        ApplyGravity();
    }

    private void Move()
    {
        Vector2 input = m_inputHandler.MoveInput;
        if (input.sqrMagnitude < 0.01f) return;

        // Camera-relative movement direction, flattened to XZ plane
        Vector3 cameraForward = m_cameraTransform.forward;
        Vector3 cameraRight = m_cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 direction = (cameraForward * input.y + cameraRight * input.x).normalized;

        m_characterController.Move(direction * m_moveSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_rotationSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (m_characterController.isGrounded && m_verticalVelocity.y < 0f)
        {
            m_verticalVelocity.y = -2f;
        }
        else
        {
            m_verticalVelocity.y += m_gravity * Time.deltaTime;
        }

        m_characterController.Move(m_verticalVelocity * Time.deltaTime);
    }
}