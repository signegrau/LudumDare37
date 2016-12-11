using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public delegate void PickupEventHandler();
    public static event PickupEventHandler OnPickup;

    public void OnPlayerCollision()
    {
        if (OnPickup != null)
        {
            OnPickup();
        }

        Destroy(gameObject);
    }
}
