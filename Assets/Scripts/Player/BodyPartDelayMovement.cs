using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartDelayMovement : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Defaults to this transform's parent if left null.")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("If true, keeps the initial world-space offset from the target.")]
    public bool keepInitialOffset = true;

    [Tooltip("Extra world-space offset to add (on top of the initial offset if enabled).")]
    public Vector2 extraOffset;

    [Tooltip("Seconds to reach ~63% of the remaining distance. Higher = floatier lag, Lower = snappier.")]
    [Min(0.0001f)] public float delaySeconds = 0.15f;

    [Tooltip("Maximum follow speed in world units/second (Infinity = no cap).")]
    public float maxSpeed = Mathf.Infinity;

    [Tooltip("Use unscaled time (ignores Time.timeScale).")]
    public bool useUnscaledTime = false;

    [Header("Axis Constraint")]
    [Tooltip("Follow on X axis?")]
    public bool followX = true;
    [Tooltip("Follow on Y axis?")]
    public bool followY = true;

    [Header("Optional: Rotation Lag")]
    public bool followRotation = false;
    [Min(0.0001f)] public float rotationDelaySeconds = 0.12f;
    [Tooltip("Max rotation speed in degrees/second.")]
    public float rotationMaxSpeed = 720f;

    // --- internals ---
    Vector3 _vel;                 // for SmoothDamp (position)
    float   _angVel;              // for SmoothDampAngle (rotation)
    Vector3 _initialOffset;
    bool    _initialized;

    [Header("Parent Independence")]
    [Tooltip("If true, detaches from parent on Awake while preserving world transform.")]
    public bool detachFromParent = true;

    Rigidbody2D _rb2D;
    Rigidbody   _rb;

    void Awake()
    {
        // Cache current parent first
        Transform prevParent = transform.parent;

        // If no explicit target, default to the current parent (before detaching)
        if (target == null) target = prevParent;

        // Detach to move independently while preserving world transform
        if (detachFromParent && prevParent != null)
        {
            transform.SetParent(null, true); // keep world-space P/R/S
        }

        TryGetComponent(out _rb2D);
        if (_rb2D == null) TryGetComponent(out _rb);
    }

    void OnEnable()
    {
        InitializeOffset();
    }

    void InitializeOffset()
    {
        if (target == null) return;

        _initialOffset = keepInitialOffset
            ? transform.position - target.position
            : Vector3.zero;

        _initialized = true;
    }

    void FixedUpdate()
    {
        if (target == null) return;
        if (!_initialized) InitializeOffset();

        float dt = useUnscaledTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime;

        // --- POSITION ---
        Vector3 desired = target.position + _initialOffset + (Vector3)extraOffset;

        // Use physics positions when available
        Vector3 current;
        if (_rb2D != null)
        {
            Vector2 p = _rb2D.position;
            current = new Vector3(p.x, p.y, transform.position.z);
        }
        else if (_rb != null)
        {
            current = _rb.position;
        }
        else
        {
            current = transform.position;
        }

        // Constrain axes
        if (!followX) desired.x = current.x;
        if (!followY) desired.y = current.y;
        desired.z = current.z; // keep original Z for 2D

        // SmoothDamp uses "smoothTime" which is our delaySeconds
        Vector3 smoothed = Vector3.SmoothDamp(
            current, desired, ref _vel, delaySeconds, maxSpeed, dt
        );

        // Apply via Rigidbody MovePosition
        if (_rb2D != null)
        {
            _rb2D.MovePosition((Vector2)smoothed);
        }
        else if (_rb != null)
        {
            _rb.MovePosition(smoothed);
        }
        else
        {
            // Fallback if no rigidbody present
            transform.position = smoothed;
        }

        // --- ROTATION (optional) ---
        if (followRotation)
        {
            float currentZ;
            if (_rb2D != null)
                currentZ = _rb2D.rotation;
            else if (_rb != null)
                currentZ = _rb.rotation.eulerAngles.z;
            else
                currentZ = transform.eulerAngles.z;

            float desiredZ = target.eulerAngles.z;
            float smoothedZ = Mathf.SmoothDampAngle(
                currentZ, desiredZ, ref _angVel, rotationDelaySeconds, rotationMaxSpeed, dt
            );

            if (_rb2D != null)
            {
                _rb2D.MoveRotation(smoothedZ);
            }
            else if (_rb != null)
            {
                _rb.MoveRotation(Quaternion.Euler(0f, 0f, smoothedZ));
            }
            else
            {
                var e = transform.eulerAngles;
                e.z = smoothedZ;
                transform.eulerAngles = e;
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Vector3 desired = target.position +
                          (keepInitialOffset && Application.isPlaying ? _initialOffset : (transform.position - target.position)) +
                          (Vector3)extraOffset;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(desired, 0.05f);
        Gizmos.DrawLine(transform.position, desired);
    }
#endif
}