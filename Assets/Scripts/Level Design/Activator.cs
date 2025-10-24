using System.Collections.Generic;
using UnityEngine;

public abstract class Activator : MonoBehaviour
{
    public List<Receiver> activateeToOpen;
    public bool oneTimeActivation;
    
    public void ActivateReceiver()
    {
        if (activateeToOpen == null) return;
        
        foreach (Receiver receiver in activateeToOpen)
        {
            if (receiver != null) receiver.Activated();
        }
    }

    public void DeactivateReceiver()
    {
        if (activateeToOpen == null || oneTimeActivation) return;
        
        foreach (Receiver receiver in activateeToOpen)
        {
            if (receiver != null) receiver.Deactivated();
        }
    }
}
