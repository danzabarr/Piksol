using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    public class Body : NetworkBehaviour
    {
        public float gravity;
        public float drag;
        public float radius;
        [Range(1, 10)] public int maxBounces;

        [Range(0, 1)] public float bounciness;
        [ReadOnly, SerializeField] private Vector3 velocity;

        public bool disableCollision;

        private void Update()
        {
            if (!isLocalPlayer)
                return;

            if (disableCollision)
            {
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                Vector3 position = transform.position;

                Geom.MovementCollision(position, velocity * Time.deltaTime, radius, bounciness, maxBounces,
                    (Vector3Int block) => World.IsCollidable(World.Instance.GetBlock(block)),
                    out Vector3 newPosition, out Vector3 newVelocity);

                transform.position = newPosition;
                velocity = newVelocity / Time.deltaTime;
            }

            velocity *= Mathf.Pow(drag, Time.deltaTime);

            ApplyForce(Vector3.down * gravity * Time.deltaTime);
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
}
