using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    public float rotationSpeed = 3f;

    private void Update()
    {
        transform.Rotate(Vector3.back * Time.deltaTime * rotationSpeed);
    }
}
