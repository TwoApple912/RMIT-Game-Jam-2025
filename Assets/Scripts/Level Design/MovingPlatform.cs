using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MovingPlatform : MonoBehaviour
{
    [Header("Parameters")]
    public bool enableMovement = true;
    [Space]
    public Vector3 pointA;
    public Vector3 pointB;
    [Space]
    public float timeAToB = 2f;
    public float timeBToA = 2f;
    [Space]
    public float pauseAtPointA = 0f;
    public float pauseAtPointB = 0f;

    [Header("Easing Curves")]
    public AnimationCurve curveAToB = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve curveBToA = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float progress = 0;
    private bool movingToB = true;
    private float pauseTimer = 0f;
    private bool isPaused = false;

    void Update()
    {
        if (enableMovement) MovePlatform();
    }

    void MovePlatform()
    {
        // Handle pause at endpoints
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
            }
            return;
        }

        float currentTime = movingToB ? timeAToB : timeBToA;
        float step = (1f / currentTime) * Time.deltaTime;
        progress += movingToB ? step : -step;

        if (progress >= 1f)
        {
            progress = 1f;
            movingToB = false;
            if (pauseAtPointB > 0f)
            {
                isPaused = true;
                pauseTimer = pauseAtPointB;
            }
        }
        else if (progress <= 0f)
        {
            progress = 0f;
            movingToB = true;
            if (pauseAtPointA > 0f)
            {
                isPaused = true;
                pauseTimer = pauseAtPointA;
            }
        }

        // Evaluate the appropriate curve based on direction
        float easedProgress = movingToB ? curveAToB.Evaluate(progress) : curveBToA.Evaluate(1f - progress);
        transform.position = Vector3.Lerp(pointA, pointB, easedProgress);
    }
}