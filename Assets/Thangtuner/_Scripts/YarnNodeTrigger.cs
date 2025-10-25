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


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
           YarnEventBridge.CallYarnEvent(nodeToTrigger);
        }
    }
}
