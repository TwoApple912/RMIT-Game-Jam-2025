using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachinePriorityRaise : MonoBehaviour
{
    [Header("Cinemachine Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private int priorityIncrease = 10;
    private bool decreaseOnExit = true;
    
    [Header("Optional: Auto-find camera on this GameObject")]
    [SerializeField] private bool autoFindCamera = true;

    void Start()
    {
        if (autoFindCamera && virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }
        
        if (virtualCamera == null)
        {
            Debug.LogWarning("CinemachinePriorityRaise: No CinemachineVirtualCamera assigned!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (virtualCamera != null && other.CompareTag("Player"))
        {
            virtualCamera.Priority += priorityIncrease;
            Debug.Log($"Camera priority increased to: {virtualCamera.Priority}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (virtualCamera != null && decreaseOnExit && other.CompareTag("Player"))
        {
            virtualCamera.Priority -= priorityIncrease;
            Debug.Log($"Camera priority decreased to: {virtualCamera.Priority}");
        }
    }
}
