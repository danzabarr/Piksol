using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Controller : MonoBehaviour
{
    public float acceleration;
    private Rigidbody rb;
    private void Update()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();


        Vector3 input = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");

        input.Normalize();
        input *= acceleration * Time.deltaTime;

        rb.AddForce(input, ForceMode.Acceleration);
    }
}
