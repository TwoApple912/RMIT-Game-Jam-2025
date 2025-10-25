using UnityEngine;

public class Hand1 : MonoBehaviour
{
    [Header("Parameters")]
    public float distanceFromItem = 0.5f;
    
    [Header("References")]
    public PlayerScript player;
    public BodyPartDelayMovement bodyPartDelayMovement;

    private void Awake()
    {
        if (player == null) player = FindObjectOfType<PlayerScript>();
        if (bodyPartDelayMovement == null) bodyPartDelayMovement = GetComponent<BodyPartDelayMovement>();
    }

    void Update()
    {
        if (player.currentHeldItem != null)
        {
            bodyPartDelayMovement.enabled = false;
            
            if (player == null || player.currentHeldItem == null) return;

            Transform itemT = null;
            if (player.currentHeldItem is GameObject)
            {
                itemT = player.currentHeldItem.transform;
            }
            else if (player.currentHeldItem is Component comp)
            {
                itemT = comp.transform;
            }

            if (itemT == null || player.transform == null) return;

            Vector3 dir = player.transform.position - itemT.position;
            if (dir.sqrMagnitude < 1e-6f)
            {
                transform.position = itemT.position;
                return;
            }

            Vector3 targetPos = itemT.position + dir.normalized * distanceFromItem;
            transform.position = targetPos;
        }
        else
        {
            bodyPartDelayMovement.enabled = true;
        }
    }
}
