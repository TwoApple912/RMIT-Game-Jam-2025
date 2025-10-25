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

    void Awake()
    {
        if (target == null) target = transform.parent;
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

    void LateUpdate()
    {
        if (target == null) return;
        if (!_initialized) InitializeOffset();

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // --- POSITION ---
        Vector3 desired = target.position + _initialOffset + (Vector3)extraOffset;

        // Constrain axes by overriding desired to current on locked axes
        Vector3 current = transform.position;
        if (!followX) desired.x = current.x;
        if (!followY) desired.y = current.y;
        desired.z = current.z; // keep original Z for 2D

        // SmoothDamp uses "smoothTime" which is our delaySeconds
        transform.position = Vector3.SmoothDamp(
            current, desired, ref _vel, delaySeconds, maxSpeed, dt
        );

        // --- ROTATION (optional) ---
        if (followRotation)
        {
            float currentZ = transform.eulerAngles.z;
            float desiredZ = target.eulerAngles.z;
            float smoothedZ = Mathf.SmoothDampAngle(
                currentZ, desiredZ, ref _angVel, rotationDelaySeconds, rotationMaxSpeed, dt
            );
            var e = transform.eulerAngles;
            e.z = smoothedZ;
            transform.eulerAngles = e;
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
