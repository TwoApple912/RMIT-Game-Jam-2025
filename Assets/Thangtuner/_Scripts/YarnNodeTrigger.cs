using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;

public class YarnNodeTrigger : MonoBehaviour
{
    public static event Action<string> OnYarnNodeTriggered;
    public string nodeToTrigger;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Prevent multiple triggers if triggerOnce is enabled
            if (triggerOnce && hasTriggered) return;

            hasTriggered = true;

            YarnEventBridge.CallYarnEvent(nodeToTrigger);
            OnYarnNodeTriggered?.Invoke(nodeToTrigger);
        }
    }
}