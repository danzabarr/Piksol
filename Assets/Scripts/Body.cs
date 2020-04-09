using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : NetworkBehaviour
{
    public float drag;
    private Vector3 velocity;

    private void Update()
    {
        if (!isLocalPlayer)
            return;
        transform.position += velocity * Time.deltaTime;
        velocity *= Mathf.Pow(drag, Time.deltaTime);
    }

    /// <summary>
    /// Apply force to the rigidbody at its center.
    /// </summary>
    public void ApplyForce(Vector3 force)
    {
        velocity += force;
    }

    /// <summary>
    /// Apply an explosive force to the rigidbody, outwards from a given position.
    /// </summary>
    public void ApplyForce(Vector3 position, float force, float falloff)
    {
        //The vector which translates the center of the explosion to this rigidbody.
        Vector3 delta = transform.position - position;

        //The distance between the center of the explosion and this rigidbody.
        float distance = delta.magnitude;

        //Apply falloff to the force
        force *= 1f / Mathf.Pow(distance, falloff);

        //Normalize the delta to get a unit vector in the direction that the explosion would carry this rigidbody.
        Vector3 direction = delta / distance;

        //Apply force in the direction with a length equal to the force
        ApplyForce(direction * force);
    }

    /// <summary>
    /// Apply a directional force to the rigidbody from a given position, along a vector.
    /// </summary>
    public void ApplyForce(Vector3 position, Vector3 force, float falloff)
    {
        //The vector which translates the center of the explosion to this rigidbody.
        Vector3 delta = transform.position - position;

        //The distance between the center of the explosion and this rigidbody.
        float distance = delta.magnitude;

        //Apply falloff to the force
        force *= 1f / Mathf.Pow(distance, falloff);

        //Normalize the delta to get a unit vector in the direction that the explosion would carry this rigidbody.
        Vector3 direction = delta / distance;

        //Apply force in the direction with a length equal to projected scalar of 'force' along 'direction'
        ApplyForce(direction * Mathf.Max(Vector3.Dot(direction, force), 0));
    }
}
