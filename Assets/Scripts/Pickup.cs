using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public delegate void PickupEventHandler();
    public static event PickupEventHandler OnPickup;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (OnPickup != null)
        {
            OnPickup();
        }

        Destroy(gameObject);
    }
}
