using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
	public Sprite buttonUp, buttonDown;

	public delegate void PickupEvent();
	public static event PickupEvent OnPickup;

	private SpriteRenderer sr;

	void Start() {
		sr = GetComponent<SpriteRenderer>();
		sr.sprite = buttonUp;
	}

    public void OnPlayerCollision()
    {
		OnPickup();

        GetComponent<Collider2D>().enabled = false;

		sr.sprite = buttonDown;
    }
}
