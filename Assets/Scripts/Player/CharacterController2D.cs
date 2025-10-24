using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f;                       // Amount of force added when the player jumps.
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;       // Amount of maxSpeed applied to crouching movement.
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f; // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false;                      // Whether or not a player can steer while jumping
    [SerializeField] private LayerMask m_WhatIsGround;                       // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                        // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                       // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider;             // A collider that will be disabled when crouching

    // New: jump assist and gravity tuning
    [Header("Jump Assist")]
    [SerializeField] private float m_CoyoteTime = 0.1f;                      // Time allowed to jump after leaving ground
    [SerializeField] private float m_JumpBufferTime = 0.1f;                  // Time allowed to queue jump before landing

    [Header("Custom Gravity")]
    [SerializeField] private float m_FallGravityMultiplier = 2.5f;           // Faster fall
    [SerializeField] private float m_LowJumpGravityMultiplier = 3.0f;        // Release early => shorter jump
    [SerializeField] private float m_JumpHangGravityMultiplier = 0.7f;       // Hold jump while rising => linger more (< 1 for hang)

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;

    // New: timers and state
    private float m_CoyoteCounter = 0f;
    private float m_JumpBufferCounter = 0f;
    private bool m_JumpHeld = false;
    private float m_BaseGravityScale = 1f;

    [Header("Events")]
    [Space]
    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_BaseGravityScale = m_Rigidbody2D.gravityScale;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // Ground check
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }

        // Update coyote timer
        if (m_Grounded)
            m_CoyoteCounter = m_CoyoteTime;
        else
            m_CoyoteCounter = Mathf.Max(0f, m_CoyoteCounter - Time.fixedUnscaledTime);
    }

    // Backward-compatible signature
    public void Move(float move, bool crouch, bool jumpPressed)
    {
        Move(move, crouch, jumpPressed, false);
    }

    // New signature with jumpHeld
    public void Move(float move, bool crouch, bool jumpPressed, bool jumpHeld)
    {
        m_JumpHeld = jumpHeld;

        // Update jump buffer
        if (jumpPressed)
            m_JumpBufferCounter = m_JumpBufferTime;
        else
            m_JumpBufferCounter = Mathf.Max(0f, m_JumpBufferCounter - Time.fixedUnscaledTime);

        // If crouching, check if the character can stand
        if (!crouch)
        {
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        // Only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // If crouching
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            // Smooth and apply
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // Handle facing
            if (move > 0 && !m_FacingRight)
            {
                Flip();
            }
            else if (move < 0 && m_FacingRight)
            {
                Flip();
            }
        }

        // Handle jump using buffer and coyote
        if (m_JumpBufferCounter > 0f && (m_Grounded || m_CoyoteCounter > 0f))
        {
            // Consume timers
            m_JumpBufferCounter = 0f;
            m_CoyoteCounter = 0f;

            // Reset vertical speed for consistent jump and apply force
            m_Grounded = false;
            m_Rigidbody2D.gravityScale = m_BaseGravityScale;
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }

        // Apply custom gravity based on state
        ApplyCustomGravity();
    }

    private void ApplyCustomGravity()
    {
        float vy = m_Rigidbody2D.velocity.y;

        if (m_Grounded)
        {
            m_Rigidbody2D.gravityScale = m_BaseGravityScale;
            return;
        }

        if (vy < -0.01f)
        {
            // Falling faster
            m_Rigidbody2D.gravityScale = m_BaseGravityScale * m_FallGravityMultiplier;
        }
        else if (vy > 0.01f)
        {
            // Rising: hold to linger, release to cut jump
            m_Rigidbody2D.gravityScale = m_BaseGravityScale *
                                         (m_JumpHeld ? m_JumpHangGravityMultiplier : m_LowJumpGravityMultiplier);
        }
        else
        {
            // Near apex
            m_Rigidbody2D.gravityScale = m_BaseGravityScale;
        }
    }

    private void Flip()
    {
        m_FacingRight = !m_FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
