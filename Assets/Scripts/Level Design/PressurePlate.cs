using System;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Activator
{
    [Header("Tracker")]
    public List<GameObject> objectsOnPlate = new List<GameObject>();
    
    [Header("References")]
    public Collider2D triggerArea;
    
    public Animator animator;

    private void Awake()
    {
        if (triggerArea == null) triggerArea = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        UpdatePlateState();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Pickup")) objectsOnPlate.Add(other.gameObject);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Pickup")) objectsOnPlate.Remove(other.gameObject);
    }
    
    void UpdatePlateState()
    {
        if (objectsOnPlate.Count > 0)
        {
            ActivateReceiver();
            PlatePressed();
        }
        else
        {
            DeactivateReceiver();
            PlateStopped();
        }
    }
    
    void PlatePressed()
    {
        animator.SetBool("isPressing",true);
    }
    void PlateStopped()
    {
        animator.SetBool("isPressing",false);
    }
}
