using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PressurePlate : Activator
{
    [Header("Tracker")]
    public List<GameObject> objectsOnPlate = new List<GameObject>();

    [Header("References")]
    public Collider2D triggerArea;
    public Animator animator;

    [Header("FMOD Sounds")]
    public EventReference pressSound;
    public EventReference releaseSound;

    private EventInstance currentEvent; // Tracks currently active FMOD event
    private bool isPressed = false; // Tracks plate state

    private void Awake()
    {
        if (triggerArea == null)
            triggerArea = GetComponent<Collider2D>();

        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdatePlateState();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Pickup"))
            objectsOnPlate.Add(other.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Pickup"))
            objectsOnPlate.Remove(other.gameObject);
    }

    private void UpdatePlateState()
    {
        bool shouldBePressed = objectsOnPlate.Count > 0;

        // Only act if state changes
        if (shouldBePressed && !isPressed)
        {
            isPressed = true;
            ActivateReceiver();
            PlatePressed();
        }
        else if (!shouldBePressed && isPressed)
        {
            isPressed = false;
            DeactivateReceiver();
            PlateReleased();
        }
    }

    private void PlatePressed()
    {
        animator.SetBool("isPressing", true);
        PlaySound(pressSound);
    }

    private void PlateReleased()
    {
        animator.SetBool("isPressing", false);
        PlaySound(releaseSound);
    }

    private void PlaySound(EventReference soundEvent)
    {
        // Stop any currently playing event before starting a new one
        if (currentEvent.isValid())
        {
            currentEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentEvent.release();
        }

        if (!soundEvent.IsNull)
        {
            currentEvent = RuntimeManager.CreateInstance(soundEvent);
            currentEvent.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            currentEvent.start();
            currentEvent.release(); // allow FMOD to handle cleanup
        }
    }
}
