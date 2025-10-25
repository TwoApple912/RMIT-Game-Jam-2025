using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class YarnEventBridge : MonoBehaviour
{
    [TextArea(3, 10)] public string note;
    
    
    public DialogueRunner runner;
    public static event Action<string> OnYarnEventCalled;

    private void OnEnable()
    {
        OnYarnEventCalled += CallYarnEventInstance;
    }
    
    private void OnDisable()
    {
        OnYarnEventCalled -= CallYarnEventInstance;
    }

    public static void CallYarnEvent(string eventName)
    {
       OnYarnEventCalled?.Invoke(eventName);
    }
    
    private void CallYarnEventInstance(string eventName)
    {
        runner.StartDialogue(eventName);
    }
}
