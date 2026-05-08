using UnityEngine;

// Movement uses CharacterController for clean integration with NavMesh enemies.
// Direction is camera-relative: pressing W moves the cat away from the camera regardless
// of camera yaw.

// Jump uses a simple impulse on the vertical velocity.

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private float m_rotationSpeed = 12f;
    [SerializeField] private float m_gravity = -20f;

    [Header("Movement Smoothing")]
    [SerializeField] private float m_smoothTime = 0.15f;

    private Vector2 m_smoothedInput;
    private Vector2 m_inputVelocity;

    [Header("Jump")]
    [SerializeField] private float m_jumpForce = 8f;

    [Header("References")]
    [SerializeField] private PlayerInputHandler m_inputHandler;
    [SerializeField] private Transform m_cameraTransform;
    [SerializeField] private Animator m_animator;

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

        if (m_animator == null)
        {
            TryGetComponent(out m_animator);
        }
    }

    private void Start()
    {
        // Subscribe in Start — input handler is on same GameObject so technically safe in
        // OnEnable, but keeping consistent with project conventions.
        if (m_inputHandler != null)
        {
            m_inputHandler.OnJumpPressed += OnJump;
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        if (m_inputHandler != null)
        {
            m_inputHandler.OnJumpPressed -= OnJump;
        }
    }

    private void Update()
    {
        Move();
        ApplyGravity();
        UpdateAnimator();
    }

    private void Move()
    {
        Vector2 rawInput = m_inputHandler.MoveInput;

        // Smoothly interpolate toward target input — eases acceleration and deceleration
        m_smoothedInput = Vector2.SmoothDamp(m_smoothedInput, rawInput, ref m_inputVelocity, m_smoothTime);

        // If smoothed input is essentially zero, skip movement work
        if (m_smoothedInput.sqrMagnitude < 0.001f) return;

        // Camera-relative direction
        Vector3 cameraForward = m_cameraTransform.forward;
        Vector3 cameraRight = m_cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 direction = cameraForward * m_smoothedInput.y + cameraRight * m_smoothedInput.x;

        m_characterController.Move(direction * m_moveSpeed * Time.deltaTime);

        // Only rotate if there's meaningful direction (avoid jitter when nearly stopped)
        if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_rotationSpeed * Time.deltaTime);
            }
    }    
    private void OnJump()
    {
        // Only jump if grounded — no double-jump
           
        if (!m_characterController.isGrounded) return;

        m_verticalVelocity.y = m_jumpForce;
        
       // if (m_animator != null)
       // {
            //m_animator.SetTrigger("Jump");
        //}
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

    private void UpdateAnimator()
    {
        if (m_animator == null) return;

        // Use smoothed input magnitude instead of raw velocity — gives natural ease-in/ease-out
        float currentSpeed = m_smoothedInput.magnitude * m_moveSpeed;
        m_animator.SetFloat("Speed", currentSpeed);
    }
}