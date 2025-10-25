using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class YarnGameManager : MonoBehaviour
{
    public static event Action OnPopDialogueUI;
    public static event Action OnStopDialogueUI;

    [YarnCommand("UIStart")]
    public static void PopDialogueUI()
    {
        OnPopDialogueUI.Invoke();
    }

    [YarnCommand("UIStop")]
    public static void StopDialogueUI()
    {
        OnStopDialogueUI.Invoke();
    }
    
}
