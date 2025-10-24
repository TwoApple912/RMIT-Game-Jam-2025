using UnityEngine;

public abstract class Receiver : MonoBehaviour
{
    public bool activated;
    
    public virtual void Activated()
    {
        activated = true;
    }
    
    public virtual void Deactivated()
    {
        activated = false;
    }
}
