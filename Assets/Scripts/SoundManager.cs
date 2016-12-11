using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip pickupSound;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        Pickup.OnPickup += PlayPickup;
    }

    private void OnDisable()
    {
        Pickup.OnPickup -= PlayPickup;
    }

    private void PlayPickup()
    {
        _audioSource.PlayOneShot(pickupSound);
    }
}
