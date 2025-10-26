using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODKiller : MonoBehaviour
{
    private static bool hasInitialized = false;

    void Awake()
    {
        // If a previous FMODKiller already existed (scene reloaded)
        if (hasInitialized)
        {
            StopAllFMODEvents();
        }

        hasInitialized = true;
    }

    private void StopAllFMODEvents()
    {
        Debug.Log("[FMODKiller] Stopping all FMOD events due to scene reload.");

        // Access the main FMOD Studio system
        FMOD.Studio.System system = RuntimeManager.StudioSystem;

        // Stop all playing events
        system.flushCommands();

        // Get all active buses and stop their events
        StopAllBuses(system);

        system.flushCommands();
    }

    private void StopAllBuses(FMOD.Studio.System system)
    {
        // FMOD buses that usually exist
        string[] commonBuses = new string[]
        {
            "bus:/",
            "bus:/SFX",
            "bus:/Music",
            "bus:/Ambience",
            "bus:/Voice"
        };

        foreach (string path in commonBuses)
        {
            if (system.lookupID(path, out FMOD.GUID busGuid) == FMOD.RESULT.OK &&
                system.getBusByID(busGuid, out Bus bus) == FMOD.RESULT.OK)
            {
                bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }
    }

    void OnApplicationQuit()
    {
        hasInitialized = false;
    }
}