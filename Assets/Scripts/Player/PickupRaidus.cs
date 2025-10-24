using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupRaidus : MonoBehaviour
{
    [Header("References")]
    public PlayerScript playerScript;
    [Space]
    public Collider2D pickupRadiusCollider;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            playerScript.UpdatePickupItemsInRangeList(other.gameObject, true);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            playerScript.UpdatePickupItemsInRangeList(other.gameObject, false);
        }
    }
}
