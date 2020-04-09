using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{

    public static class Raycast
    {
        public static bool Box(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Vector3 position, Quaternion rotation, Vector3 scale, out Vector3 intersection, out float distance)
        {
            Vector3 c000 = position;
            Vector3 c100 = c000 + (rotation * Vector3.right) * scale.x;
            Vector3 c010 = c000 + (rotation * Vector3.up) * scale.y;
            Vector3 c001 = c000 + (rotation * Vector3.forward) * scale.z;

            Vector3 c011 = c001 + c010 - c000;
            Vector3 c101 = c001 + c100 - c000;
            Vector3 c110 = c010 + c100 - c000;
            Vector3 c111 = c001 + c010 + c100 - c000 - c000;

            float dotX = Vector3.Dot(rotation * Vector3.right, rayDirection);
            float dotY = Vector3.Dot(rotation * Vector3.up, rayDirection);
            float dotZ = Vector3.Dot(rotation * Vector3.forward, rayDirection);

            return (dotX < 0 ? Quad(rayOrigin, rayDirection, maxDistance, c100, c101, c110, out intersection, out distance)  //E
                             : Quad(rayOrigin, rayDirection, maxDistance, c001, c000, c011, out intersection, out distance)) //W
                || (dotY < 0 ? Quad(rayOrigin, rayDirection, maxDistance, c010, c110, c011, out intersection, out distance)  //U
                             : Quad(rayOrigin, rayDirection, maxDistance, c001, c101, c000, out intersection, out distance)) //D
                || (dotZ < 0 ? Quad(rayOrigin, rayDirection, maxDistance, c101, c001, c111, out intersection, out distance)  //N
                             : Quad(rayOrigin, rayDirection, maxDistance, c000, c100, c010, out intersection, out distance));//S
        }

        public static bool Quad(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Vector3 c00, Vector3 c10, Vector3 c01, out Vector3 intersection, out float distance)
        {
            intersection = default;
            distance = 0;

            Vector3 x = c10 - c00;
            Vector3 y = c01 - c00;
            Vector3 n = Vector3.Cross(x, y);                    //Quad normal

            float ndotdR = -Vector3.Dot(n, rayDirection);

            if (Mathf.Abs(ndotdR) < 1e-6f)                      //Check not parallel
                return false;

            distance = Vector3.Dot(n, rayOrigin - c00) / ndotdR;//Distance to plane intersection

            if (distance < 0)                                   //Check in front of ray
                return false;

            if (distance > maxDistance)
                return false;

            intersection = rayOrigin + rayDirection * distance; //Plane intersection

            Vector3 i = intersection - c00;                     //Translate intersection to quad origin
            float u = Vector3.Dot(i, x);                        //Project along quad in x direction
            float v = Vector3.Dot(i, y);                        //Project along quad in y direction

            return u >= 0.0f && u <= Vector3.Dot(x, x)
                 && v >= 0.0f && v <= Vector3.Dot(y, y);        //Check intersection is inside plane
        }

        public static bool Bone(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Bone bone, out Vector3 intersection, out float distance)
        {
            if (bone.Block == null)
            {
                intersection = default;
                distance = default;
                return false;
            }


            Vector3 offset = bone.transform.localToWorldMatrix * bone.Block.Origin;

            return Box(rayOrigin, rayDirection, maxDistance, bone.transform.position - offset + bone.Block.LowerInset, bone.transform.rotation, Vector3.Scale(bone.transform.lossyScale, (Vector3)bone.Block.InsetSize * BlockObject.VoxelSize), out intersection, out distance);
        }

        public static bool Bones(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Bone[] bones, out Bone bone, out Vector3 intersection, out float distance)
        {
            bone = null;
            distance = maxDistance;
            intersection = default;

            Vector3 i;
            float d;

            foreach(Bone b in bones)
                if (Bone(rayOrigin, rayDirection, distance, b, out i, out d))
                {
                    distance = d;
                    intersection = i;
                    bone = b;
                }

            return bone != null;
        }
    }
}
