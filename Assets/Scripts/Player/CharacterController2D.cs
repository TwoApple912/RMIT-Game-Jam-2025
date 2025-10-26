using System;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f;
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
    [SerializeField] private bool m_AirControl = false;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private Transform m_CeilingCheck;
    [SerializeField] private Collider2D m_CrouchDisableCollider;

    [Header("Jump Assist")]
    [SerializeField] private float m_CoyoteTime = 0.1f;
    [SerializeField] private float m_JumpBufferTime = 0.1f;

    [Header("Custom Gravity")]
    [SerializeField] private float m_FallGravityMultiplier = 2.5f;
    [SerializeField] private float m_LowJumpGravityMultiplier = 3.0f;
    [SerializeField] private float m_JumpHangGravityMultiplier = 0.7f;

    const float k_GroundedRadius = .2f;
    private bool m_Grounded;
    const float k_CeilingRadius = .2f;
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;
    private Vector3 m_Velocity = Vector3.zero;

    private float m_CoyoteCounter = 0f;
    private float m_JumpBufferCounter = 0f;
    private bool m_JumpHeld = false;
    private float m_BaseGravityScale = 1f;

    [Header("Events")]
    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;
    public Animator animator;

    public Transform hand1;

    [Header("FMOD Sounds")]
    public EventReference jumpSoundEvent;
    public EventReference landSoundEvent;

    [Header("Particles")]
    [Tooltip("Particle system that plays when jumping.")]
    public ParticleSystem jumpParticles;
    [Tooltip("Particle system that plays when landing.")]
    public ParticleSystem landParticles;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_BaseGravityScale = m_Rigidbody2D.gravityScale;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();

        animator = GetComponentInChildren<Animator>();

        // Hook up the landing sound and particles to the UnityEvent
        OnLandEvent.AddListener(PlayLandSound);
        OnLandEvent.AddListener(PlayLandParticles);
    }

    private void Update()
    {
        animator.SetBool("isGrounded", m_Grounded);
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;

                // Only trigger land effects if velocity was significant
                if (!wasGrounded && m_Rigidbody2D.velocity.y < -2f)
                {
                    OnLandEvent.Invoke();
                }
            }
        }

        if (m_Grounded)
            m_CoyoteCounter = m_CoyoteTime;
        else
            m_CoyoteCounter = Mathf.Max(0f, m_CoyoteCounter - Time.fixedDeltaTime);
    }

    public void Move(float move, bool crouch, bool jumpPressed)
    {
        Move(move, crouch, jumpPressed, false);
    }

    public void Move(float move, bool crouch, bool jumpPressed, bool jumpHeld)
    {
        m_JumpHeld = jumpHeld;

        if (jumpPressed)
            m_JumpBufferCounter = m_JumpBufferTime;
        else
            m_JumpBufferCounter = Mathf.Max(0f, m_JumpBufferCounter - Time.fixedDeltaTime);

        if (!crouch)
        {
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        if (m_Grounded || m_AirControl)
        {
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                move *= m_CrouchSpeed;

                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            if (move > 0 && !m_FacingRight)
                Flip();
            else if (move < 0 && m_FacingRight)
                Flip();
        }

        if (m_JumpBufferCounter > 0f && (m_Grounded || m_CoyoteCounter > 0f))
        {
            m_JumpBufferCounter = 0f;
            m_CoyoteCounter = 0f;

            m_Grounded = false;
            m_Rigidbody2D.gravityScale = m_BaseGravityScale;
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0f);
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));

            animator.SetTrigger("jump");

            // ✅ Play jump sound + particle
            PlayJumpSound();
            PlayJumpParticles();
        }

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
            m_Rigidbody2D.gravityScale = m_BaseGravityScale * m_FallGravityMultiplier;
        }
        else if (vy > 0.01f)
        {
            m_Rigidbody2D.gravityScale = m_BaseGravityScale *
                                         (m_JumpHeld ? m_JumpHangGravityMultiplier : m_LowJumpGravityMultiplier);
        }
        else
        {
            m_Rigidbody2D.gravityScale = m_BaseGravityScale;
        }
    }

    private void Flip()
    {
        m_FacingRight = !m_FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        if (hand1 != null)
        {
            hand1.localScale = new Vector3(hand1.localScale.x * -1, hand1.localScale.y, hand1.localScale.z);
        }
    }

    // ✅ FMOD Sounds
    private void PlayJumpSound()
    {
        if (jumpSoundEvent.IsNull == false)
            RuntimeManager.PlayOneShot(jumpSoundEvent, transform.position);
    }

    private void PlayLandSound()
    {
        if (landSoundEvent.IsNull == false)
            RuntimeManager.PlayOneShot(landSoundEvent, transform.position);
    }

    // ✅ Particle Effects
    private void PlayJumpParticles()
    {
        if (jumpParticles != null)
            jumpParticles.Play();
    }

    private void PlayLandParticles()
    {
        if (landParticles != null)
            landParticles.Play();
    }
}
