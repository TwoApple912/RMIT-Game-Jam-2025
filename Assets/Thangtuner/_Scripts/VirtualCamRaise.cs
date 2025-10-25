using UnityEngine;
    using Cinemachine;
    
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class VirtualCamRaise : MonoBehaviour
    {
        [SerializeField] string playerTag = "Player";
        [SerializeField] int priorityDelta = 1;
    
        CinemachineVirtualCamera vcam;
    
        void Awake()
        {
            vcam = GetComponent<CinemachineVirtualCamera>();
            if (vcam == null)
                Debug.LogWarning($"CinemachineVirtualCamera not found on {name}");
        }
    
        void OnTriggerEnter2D(Collider2D other)
        {
            if (vcam == null) return;
            if (!other.CompareTag(playerTag)) return;
            vcam.Priority += priorityDelta;
        }
    
        void OnTriggerExit2D(Collider2D other)
        {
            if (vcam == null) return;
            if (!other.CompareTag(playerTag)) return;
            vcam.Priority -= priorityDelta;
        }
    }