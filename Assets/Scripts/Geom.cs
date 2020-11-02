using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    public static class Geom
    {
        public static Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 lineA, Vector3 lineB)
        {
            float dot = Vector3.Dot(point - lineA, lineB - lineA);
            float len_sq = (lineB - lineA).sqrMagnitude;

            float param = -1;
            if (len_sq != 0) //in case of 0 length line
                param = dot / len_sq;

            if (param < 0)
                return lineA;

            else if (param > 1)
                return lineB;

            else
                return Vector3.Lerp(lineA, lineB, param);
        }

        public static bool SphereLineSegmentIntersection(Vector3 origin, float radius, Vector3 lineA, Vector3 lineB)
        {
            return (ClosestPointOnLineSegment(origin, lineA, lineB) - origin).sqrMagnitude < radius * radius;
        }

        public static bool RaycastAABB(Vector3 rayOrigin, Vector3 rayDirection, Vector3 boundsMin, Vector3 boundsMax, out float t)
        {
            // r.dir is unit direction vector of ray
            Vector3 dirFrac = new Vector3(1f / rayDirection.x, 1f / rayDirection.y, 1f / rayDirection.z);

            // lb is the corner of AABB with minimal coordinates - left bottom, rt is maximal corner
            // r.org is origin of ray
            float t1 = (boundsMin.x - rayOrigin.x) * dirFrac.x;
            float t2 = (boundsMax.x - rayOrigin.x) * dirFrac.x;
            float t3 = (boundsMin.y - rayOrigin.y) * dirFrac.y;
            float t4 = (boundsMax.y - rayOrigin.y) * dirFrac.y;
            float t5 = (boundsMin.z - rayOrigin.z) * dirFrac.z;
            float t6 = (boundsMax.z - rayOrigin.z) * dirFrac.z;

            float tmin = Mathf.Max(Mathf.Max(Mathf.Min(t1, t2), Mathf.Min(t3, t4)), Mathf.Min(t5, t6));
            float tmax = Mathf.Min(Mathf.Min(Mathf.Max(t1, t2), Mathf.Max(t3, t4)), Mathf.Max(t5, t6));

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tmax < 0)
            {
                t = tmax;
                return false;
            }

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax)
            {
                t = tmax;
                return false;
            }

            t = tmin;
            return true;
        }

        public static bool RaycastBox(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Vector3 position, Quaternion rotation, Vector3 scale, out Vector3 intersection, out float distance)
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

            return (dotX < 0 ? RaycastQuad(rayOrigin, rayDirection, maxDistance, c100, c110, c101, false, out intersection, out distance)  //E
                             : RaycastQuad(rayOrigin, rayDirection, maxDistance, c001, c011, c000, false, out intersection, out distance)) //W
                || (dotY < 0 ? RaycastQuad(rayOrigin, rayDirection, maxDistance, c010, c011, c110, false, out intersection, out distance)  //U
                             : RaycastQuad(rayOrigin, rayDirection, maxDistance, c001, c000, c101, false, out intersection, out distance)) //D
                || (dotZ < 0 ? RaycastQuad(rayOrigin, rayDirection, maxDistance, c101, c111, c001, false, out intersection, out distance)  //N
                             : RaycastQuad(rayOrigin, rayDirection, maxDistance, c000, c010, c100, false, out intersection, out distance));//S
        }

        public static bool RaycastQuad(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Vector3 c00, Vector3 c10, Vector3 c01, bool collideWithBackFace, out Vector3 intersection, out float distance)
        {
            intersection = default;
            distance = 0;

            Vector3 x = c10 - c00;
            Vector3 y = c01 - c00;
            Vector3 n = Vector3.Cross(x, y);                    //Quad normal

            float ndotdR = -Vector3.Dot(n, rayDirection);

            if (Mathf.Abs(ndotdR) < 1e-6f)                      //Check not parallel
                return false;

            if (!collideWithBackFace && ndotdR < 0)
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

        public static bool RaycastCylinder(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Vector3 cylinderOrigin, Vector3 cylinderDirection, float cylinderLength, float cylinderRadius, out Vector3 intersection, out Vector3 normal, out float distance)
        {
            Matrix4x4 cylinder = Matrix4x4.TRS(cylinderOrigin, Quaternion.LookRotation(cylinderDirection), Vector3.one);
            return RaycastCylinder(rayOrigin, rayDirection, maxDistance, cylinder, cylinderLength, cylinderRadius, out intersection, out normal, out distance);
        }

        public static bool RaycastCylinder(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Matrix4x4 cylinder, float cylinderLength, float cylinderRadius, out Vector3 intersection, out Vector3 normal, out float distance)
        {
            Vector3 o = cylinder.inverse.MultiplyPoint3x4(rayOrigin);
            Vector3 d = cylinder.inverse.MultiplyVector(rayDirection);

            distance = 0;
            intersection = rayOrigin;
            normal = rayDirection;

            float a = d.x * d.x + d.y * d.y;
            float b = 2 * (o.x * d.x + o.y * d.y);
            float c = o.x * o.x + o.y * o.y - cylinderRadius * cylinderRadius;

            float discr = b * b - 4 * a * c;
            if (discr < 0)
            {
                return false;
            }

            //float distance = (-b + Mathf.Sqrt(discr)) / (2 * a); back 'face'
            distance = (-b - Mathf.Sqrt(discr)) / (2 * a);

            if (distance < 0)
                return false;

            if (distance > maxDistance)
                return false;

            Vector3 i = o + d * distance;

            if (i.z < 0 || i.z > cylinderLength)
                return false;

            Vector3 n = new Vector3(2 * i.x, 2 * i.y, 0.0f).normalized;

            intersection = cylinder.MultiplyPoint3x4(i);
            normal = cylinder.MultiplyVector(n);

            return true;
        }

        public static bool RaycastBone(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Bone bone, out Vector3 intersection, out float distance)
        {
            if (bone.Block == null)
            {
                intersection = default;
                distance = default;
                Debug.Log("block was null");
                return false;
            }


            Vector3 offset = bone.transform.localToWorldMatrix * bone.Block.Origin;

            return RaycastBox(rayOrigin, rayDirection, maxDistance, bone.transform.position - offset + bone.Block.LowerInset, bone.transform.rotation, Vector3.Scale(bone.transform.lossyScale, (Vector3)bone.Block.InsetSize * BlockObject.VoxelSize), out intersection, out distance);
        }

        public static bool RaycastBones(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Bone[] bones, out Bone bone, out Vector3 intersection, out float distance)
        {
            bone = null;
            distance = maxDistance;
            intersection = default;

            foreach (Bone b in bones)
                if (RaycastBone(rayOrigin, rayDirection, distance, b, out Vector3 i, out float d))
                {
                    distance = d;
                    intersection = i;
                    bone = b;
                }

            return bone != null;
        }


        public static bool SphereAABBIntersection(Vector3 origin, float radius, Vector3 boundsMin, Vector3 boundsSize)
        {
            //The sphere is already inside the block... oh dear.
            if (new Bounds(boundsMin + boundsSize * .5f, boundsSize).Contains(origin))
                return true;

            if (!new Bounds(boundsMin + boundsSize * .5f, boundsSize + Vector3.one * radius * 2).Contains(origin))
                return false;

            if (new Bounds(boundsMin + boundsSize * .5f, boundsSize + Vector3.right * radius * 2).Contains(origin))
                return true;

            if (new Bounds(boundsMin + boundsSize * .5f, boundsSize + Vector3.up * radius * 2).Contains(origin))
                return true;

            if (new Bounds(boundsMin + boundsSize * .5f, boundsSize + Vector3.forward * radius * 2).Contains(origin))
                return true;

            Vector3 c000 = boundsMin;
            Vector3 c001 = boundsMin + new Vector3(0, 0, boundsSize.z);
            Vector3 c010 = boundsMin + new Vector3(0, boundsSize.y, 0);
            Vector3 c011 = boundsMin + new Vector3(0, boundsSize.y, boundsSize.z);
            Vector3 c100 = boundsMin + new Vector3(boundsSize.x, 0, 0);
            Vector3 c101 = boundsMin + new Vector3(boundsSize.x, 0, boundsSize.z);
            Vector3 c110 = boundsMin + new Vector3(boundsSize.x, boundsSize.y, 0);
            Vector3 c111 = boundsMin + new Vector3(boundsSize.x, boundsSize.y, boundsSize.z);

            if (SphereLineSegmentIntersection(origin, radius * 2, c000, c100))
                return true;
            if (SphereLineSegmentIntersection(origin, radius * 2, c001, c101))
                return true;
            if (SphereLineSegmentIntersection(origin, radius * 2, c011, c111))
                return true;
            if (SphereLineSegmentIntersection(origin, radius * 2, c010, c110))
                return true;

            if (SphereLineSegmentIntersection(origin, radius * 2, c000, c010))
                return true;
            if (SphereLineSegmentIntersection(origin, radius * 2, c001, c011))
                return true;
            if (SphereLineSegmentIntersection(origin, radius * 2, c101, c111))
                return true;
            if (SphereLineSegmentIntersection(origin, radius * 2, c100, c110))
                return true;

            if (SphereLineSegmentIntersection(origin, radius * 2, c000, c001))
                return true;
            if (SphereLineSegmentIntersection(origin, radius * 2, c010, c011))
                return true;
            if (SphereLineSegmentIntersection(origin, radius * 2, c110, c111))
                return true;
            if (SphereLineSegmentIntersection(origin, radius * 2, c100, c101))
                return true;

            return false;
        }

        public static bool SphereCastAABB(Vector3 rayOrigin, Vector3 rayDirection, float radius, float maxDistance, Vector3 block, Vector3 blockSize, out Vector3 collisionPoint, out Vector3 collisionNormal, out float collisionDistanceAlongRay)
        {
            
            collisionPoint = rayOrigin;
            collisionNormal = rayDirection;
            collisionDistanceAlongRay = maxDistance;

            Vector3 c000 = block;
            Vector3 c001 = block + new Vector3(0, 0, blockSize.z);
            Vector3 c010 = block + new Vector3(0, blockSize.y, 0);
            Vector3 c011 = block + new Vector3(0, blockSize.y, blockSize.z);
            Vector3 c100 = block + new Vector3(blockSize.x, 0, 0);
            Vector3 c101 = block + new Vector3(blockSize.x, 0, blockSize.z);
            Vector3 c110 = block + new Vector3(blockSize.x, blockSize.y, 0);
            Vector3 c111 = block + new Vector3(blockSize.x, blockSize.y, blockSize.z);

            //The ray completely misses the block by a margin equal to the radius.
            if (!RaycastAABB(rayOrigin, rayDirection, block - Vector3.one * radius, block + blockSize + Vector3.one * radius, out _))
                return false;


            //Already inside it...
         //   if (SphereAABBIntersection(rayOrigin, radius, block, blockSize))
         //   {
         //       collisionDistanceAlongRay = 0;
         //       collisionNormal = ((block + blockSize / 2) - rayOrigin).normalized;
         //       //do something...
         //       stuck = true;
         //       return true;
         //   }

            bool hit = false;

            #region Faces
            //East Face
            if (rayDirection.x < 0)
            {
                Vector3 normal = Vector3.right;
                Vector3 c00 = c100 + normal * radius;
                Vector3 c10 = c110 + normal * radius;
                Vector3 c01 = c101 + normal * radius;

                if (RaycastQuad(rayOrigin, rayDirection, collisionDistanceAlongRay, c00, c10, c01, false, out Vector3 i, out float d))
                {
                    collisionPoint = i - normal * radius;
                    collisionDistanceAlongRay = d;
                    collisionNormal = normal;
                    hit = true;
                }
            }
            //West Face
            if (rayDirection.x > 0)
            {
                Vector3 normal = -Vector3.right;
                Vector3 c00 = c000 + normal * radius;
                Vector3 c10 = c001 + normal * radius;
                Vector3 c01 = c010 + normal * radius;

                if (RaycastQuad(rayOrigin, rayDirection, collisionDistanceAlongRay, c00, c10, c01, false, out Vector3 i, out float d))
                {
                    collisionPoint = i - normal * radius;
                    collisionDistanceAlongRay = d;
                    collisionNormal = normal;
                    hit = true;
                }
            }
            //Up Face
            if (rayDirection.y < 0)
            {
                Vector3 normal = Vector3.up;
                Vector3 c00 = c010 + normal * radius;
                Vector3 c10 = c011 + normal * radius;
                Vector3 c01 = c110 + normal * radius;

                if (RaycastQuad(rayOrigin, rayDirection, collisionDistanceAlongRay, c00, c10, c01, false, out Vector3 i, out float d))
                {
                    collisionPoint = i - normal * radius;
                    collisionDistanceAlongRay = d;
                    collisionNormal = normal;
                    hit = true;
                }
            }
            //Down Face
            if (rayDirection.y > 0)
            {
                Vector3 normal = -Vector3.up;
                Vector3 c00 = c000 + normal * radius;
                Vector3 c10 = c100 + normal * radius;
                Vector3 c01 = c001 + normal * radius;

                if (RaycastQuad(rayOrigin, rayDirection, collisionDistanceAlongRay, c00, c10, c01, false, out Vector3 i, out float d))
                {
                    collisionPoint = i - normal * radius;
                    collisionDistanceAlongRay = d;
                    collisionNormal = normal;
                    hit = true;
                }
            }
            //North Face
            if (rayDirection.z < 0)
            {
                Vector3 normal = Vector3.forward;
                Vector3 c00 = c001 + normal * radius;
                Vector3 c10 = c101 + normal * radius;
                Vector3 c01 = c011 + normal * radius;

                if (RaycastQuad(rayOrigin, rayDirection, collisionDistanceAlongRay, c00, c10, c01, false, out Vector3 i, out float d))
                {
                    collisionPoint = i - normal * radius;
                    collisionDistanceAlongRay = d;
                    collisionNormal = normal;
                    hit = true;
                }
            }
            //South Face
            if (rayDirection.z > 0)
            {
                Vector3 normal = -Vector3.forward;
                Vector3 c00 = c000 + normal * radius;
                Vector3 c10 = c010 + normal * radius;
                Vector3 c01 = c100 + normal * radius;

                if (RaycastQuad(rayOrigin, rayDirection, collisionDistanceAlongRay, c00, c10, c01, false, out Vector3 i, out float d))
                {
                    collisionPoint = i - normal * radius;
                    collisionDistanceAlongRay = d;
                    collisionNormal = normal;
                    hit = true;
                }
            }
            #endregion

            #region Edges
            if (!hit)
            {
                float cd = collisionDistanceAlongRay;
                Vector3 cp = collisionPoint;
                Vector3 cn = collisionNormal;

                void CheckEdge(Vector3 origin, Vector3 direction, float length)
                {
                    if (RaycastCylinder(rayOrigin, rayDirection, cd, origin, direction, length, radius, out Vector3 i, out Vector3 n, out float d))
                    {
                        cp = i - n * radius;
                        cd = d;
                        cn = n;
                        hit = true;
                    }
                }

                CheckEdge(c000, Vector3.right, blockSize.x);
                CheckEdge(c001, Vector3.right, blockSize.x);
                CheckEdge(c011, Vector3.right, blockSize.x);
                CheckEdge(c010, Vector3.right, blockSize.x);

                CheckEdge(c000, Vector3.up, blockSize.y);
                CheckEdge(c001, Vector3.up, blockSize.y);
                CheckEdge(c101, Vector3.up, blockSize.y);
                CheckEdge(c100, Vector3.up, blockSize.y);

                CheckEdge(c000, Vector3.forward, blockSize.z);
                CheckEdge(c010, Vector3.forward, blockSize.z);
                CheckEdge(c110, Vector3.forward, blockSize.z);
                CheckEdge(c100, Vector3.forward, blockSize.z);

                collisionDistanceAlongRay = cd;
                collisionPoint = cp;
                collisionNormal = cn;
            }
            #endregion

            #region Corners
            if (!hit)
            {
                float cd = collisionDistanceAlongRay;
                Vector3 cp = collisionPoint;
                Vector3 cn = collisionNormal;

                void CheckCorner(Vector3 corner)
                {
                    if (RaycastSphere(corner, radius, rayOrigin, rayDirection, out float d) && d < cd)
                    {
                        cp = corner;
                        cd = d;
                        cn = ((rayOrigin + rayDirection * cd) - corner).normalized;
                        hit = true;
                    }
                }

                CheckCorner(c000);
                CheckCorner(c001);
                CheckCorner(c010);
                CheckCorner(c011);
                CheckCorner(c100);
                CheckCorner(c101);
                CheckCorner(c110);
                CheckCorner(c111);

                collisionDistanceAlongRay = cd;
                collisionPoint = cp;
                collisionNormal = cn;
            }
            #endregion

            return hit;
        }

        public delegate bool CheckSolid(Vector3Int block);

        public static void MovementCollision(Vector3 position, Vector3 velocity, float radius, float bounciness, int maxBounces, CheckSolid checkSolid, out Vector3 newPosition, out Vector3 newVelocity)
        {
            float magnitude = velocity.magnitude;
            Vector3 currentPosition = position;
            float currentMagnitude = magnitude;
            Vector3 currentDirection = velocity.normalized;

            newPosition = currentPosition;
            newVelocity = velocity;

            for (int i = 0; i < maxBounces; i++)
            {
                if (currentMagnitude <= 0)
                {
                    newPosition = currentPosition;
                    newVelocity = currentDirection * magnitude;
                    break;
                }

                if (!SphereCastVoxels(currentPosition, currentDirection, currentMagnitude, radius, checkSolid,
                    out Vector3Int collisionVoxel,
                    out Vector3 collisionPoint,
                    out Vector3 collisionNormal,
                    out float collisionDistanceAlongRay
                ))
                {
                    newPosition = currentPosition + currentDirection * Mathf.Max(currentMagnitude, 0);
                    newVelocity = currentDirection * magnitude;
                    break;
                }

                currentPosition += currentDirection * collisionDistanceAlongRay;
                currentMagnitude -= collisionDistanceAlongRay;

                Vector3 reflect = Vector3.Reflect(currentDirection, collisionNormal).normalized;
                //Vector3 slide = Vector3.ProjectOnPlane(currentDirection, collisionNormal).normalized;
                //currentDirection = Vector3.Lerp(slide, reflect, bounciness).normalized;
                currentDirection = reflect;
                magnitude *= bounciness;
            }
           

            //Gizmos.DrawLine(currentPosition, currentPosition + currentDirection * currentMagnitude);
            //Gizmos.DrawSphere(currentPosition, radius);

        }

        private static List<Vector3Int> voxels;

        public static bool SphereCastVoxels(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, float radius, CheckSolid checkSolid,
            out Vector3Int collisionVoxel,
            out Vector3 collisionPoint,
            out Vector3 collisionNormal,
            out float collisionDistanceAlongRay
        )
        {
            voxels = new List<Vector3Int>();
            VoxelTraverse.Ray(rayOrigin, rayDirection, maxDistance, Vector3.one, Vector3.zero, (Vector3Int voxel, Vector3 intersection, Vector3 n) =>
            {
                for (int y = -1; y <= 1; y++)
                    for (int z = -1; z <= 1; z++)
                        for (int x = -1; x <= 1; x++)
                        {
                            Vector3Int v = voxel + new Vector3Int(x, y, z);
                            if (checkSolid(v) && !voxels.Contains(v))
                                voxels.Add(v);
                        }
                return false;
            });

            bool hit = false;

            collisionVoxel = default;
            collisionPoint = default;
            collisionNormal = default;
            collisionDistanceAlongRay = maxDistance;

            foreach (Vector3Int v in voxels)
            {
                if (SphereCastAABB(rayOrigin, rayDirection, radius, collisionDistanceAlongRay, v, Vector3.one, out Vector3 cp, out Vector3 cn, out float cd))
                {
                    collisionVoxel = v;
                    collisionPoint = cp;
                    collisionNormal = cn;
                    collisionDistanceAlongRay = cd;
                    hit = true;
                }
            }

            return hit;
        }

        public static bool RaycastSphere(Vector3 center, float radius, Vector3 rayOrigin, Vector3 rayDirection, out float d)
        {
            Vector3 oc = rayOrigin - center;
            float a = Vector3.Dot(rayDirection, rayDirection);
            float b = 2f * Vector3.Dot(oc, rayDirection);
            float c = Vector3.Dot(oc, oc) - radius * radius;
            float discriminant = b * b - 4 * a * c;
            d = 0;

            if (discriminant < 0)
                return false;

            d = -b - Mathf.Sqrt(discriminant);

            if (d > 0)
                d /= 2f * a;
            else
                return false;

            return true;
        }
    }
}