using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTest : MonoBehaviour
{

    public Transform ray;
    public float velocity;
    public Transform cylinder;

    public Vector3 block;
    public Vector3 blockSize;

    public float radius;

    public static Mesh cylinderMesh;

    public bool radiusOnBlock;

    public void Blah()
    {
    
        Vector3 rayOrigin = ray.transform.position;
        Vector3 rayDirection = ray.transform.forward;
        float maxDistance = this.velocity;
        Vector3 cylinderOrigin = cylinder.transform.position;
        Vector3 cylinderDirection = cylinder.transform.forward;
        float cylinderLength = 1;
        float cylinderRadius = radius;

        DrawCylinder(cylinderOrigin, cylinderOrigin + cylinderDirection * cylinderLength, cylinderRadius);

        
        if (Piksol.Geom.SphereLineSegmentIntersection(rayOrigin, cylinderRadius * 2, cylinderOrigin, cylinderOrigin + cylinderDirection * cylinderLength))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rayOrigin, radius);
            return;
        }

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(rayOrigin, radius);

        if (Piksol.Geom.RaycastCylinder(rayOrigin, rayDirection, maxDistance, cylinderOrigin, cylinderDirection, cylinderLength, cylinderRadius, out Vector3 intersection, out Vector3 normal, out float distance))
        {
            Gizmos.DrawSphere(intersection, .01f);
            Gizmos.DrawLine(intersection, intersection + normal);
        }
        Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * maxDistance);
    }

    public void OnDrawGizmos()
    {
        Vector3 rayOrigin = ray.position;
        Vector3 rayDirection = ray.forward;

        void DrawBlock(Vector3 block, Vector3 blockSize)
        {
            Gizmos.color = Color.white;
            DrawCube(block.x, block.y, block.z, blockSize.x, blockSize.y, blockSize.z);
        
            Vector3 c000 = block;
            Vector3 c001 = block + new Vector3(0, 0, blockSize.z);
            Vector3 c010 = block + new Vector3(0, blockSize.y, 0);
            Vector3 c011 = block + new Vector3(0, blockSize.y, blockSize.z);
            Vector3 c100 = block + new Vector3(blockSize.x, 0, 0);
            Vector3 c101 = block + new Vector3(blockSize.x, 0, blockSize.z);
            Vector3 c110 = block + new Vector3(blockSize.x, blockSize.y, 0);
            Vector3 c111 = block + new Vector3(blockSize.x, blockSize.y, blockSize.z);
        
            if (radiusOnBlock)
            {
        
                Gizmos.color = new Color(0, 1, 1, .25f);
                DrawCube(block.x - radius, block.y, block.z, blockSize.x + radius * 2, blockSize.y, blockSize.z);
                DrawCube(block.x, block.y - radius, block.z, blockSize.x, blockSize.y + radius * 2, blockSize.z);
                DrawCube(block.x, block.y, block.z - radius, blockSize.x, blockSize.y, blockSize.z + radius * 2);
        
                Gizmos.color = new Color(1, 1, 0, .25f);
                DrawCylinder(c000, c001, radius);
                DrawCylinder(c001, c101, radius);
                DrawCylinder(c101, c100, radius);
                DrawCylinder(c100, c000, radius);
        
                DrawCylinder(c010, c011, radius);
                DrawCylinder(c011, c111, radius);
                DrawCylinder(c111, c110, radius);
                DrawCylinder(c110, c010, radius);
        
                DrawCylinder(c000, c010, radius);
                DrawCylinder(c001, c011, radius);
                DrawCylinder(c101, c111, radius);
                DrawCylinder(c100, c110, radius);
        
                Gizmos.color = new Color(1, 0, 1, .25f);
                Gizmos.DrawSphere(c000, radius);
                Gizmos.DrawSphere(c001, radius);
                Gizmos.DrawSphere(c010, radius);
                Gizmos.DrawSphere(c011, radius);
                Gizmos.DrawSphere(c100, radius);
                Gizmos.DrawSphere(c101, radius);
                Gizmos.DrawSphere(c110, radius);
                Gizmos.DrawSphere(c111, radius);
        
            }
        
            Gizmos.color = new Color(1, 1, 1, .125f);
            DrawCube(block.x - radius, block.y - radius, block.z - radius, blockSize.x + radius * 2, blockSize.y + radius * 2, blockSize.z + radius * 2);
        }

        DrawBlock(block, blockSize);

        int size = 16;
        bool[,,] blocks = new bool[size, size, size];

        blocks[0, 0, 0] = true;
        blocks[3, 0, 5] = true;
        blocks[2, 2, 2] = true;
        blocks[1, 1, 2] = true;
        blocks[1, 2, 1] = true;

        Gizmos.color = Color.white;

        //for (int y = 0; y < 8; y++)
        //    for (int z = 0; z < 8; z++)
        //        for (int x = 0; x < 8; x++)
        //            if (blocks[x, y, z])
        //                DrawCube(x, y, z, 1, 1, 1);

        if (!radiusOnBlock)
        {
            Gizmos.color = new Color(0, 0, 1, .125f);
            DrawCylinder(rayOrigin, rayOrigin + rayDirection * velocity, radius);
            Gizmos.DrawSphere(rayOrigin, radius);
            Gizmos.DrawSphere(rayOrigin + rayDirection * velocity, radius);
        }
        
        Gizmos.color = new Color(1, 0, 0, 1);
        Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * velocity);

        //List<Vector3Int> voxels = new List<Vector3Int>();
        //Piksol.VoxelTraverse.Ray(rayOrigin, rayDirection, velocity, Vector3.one, Vector3.zero, (Vector3Int voxel, Vector3 intersection, Vector3 n) =>
        //{
        //    for (int y = -1; y <= 1; y++)
        //        for (int z = -1; z <= 1; z++)
        //            for (int x = -1; x <= 1; x++)
        //            {
        //                Vector3Int v = voxel + new Vector3Int(x, y, z);
        //                if (v.x < 0 || v.y < 0 || v.z < 0)
        //                    continue;
        //                if (v.x >= size || v.y >= size || v.z >= size)
        //                    continue;
        //                if (blocks[v.x, v.y, v.z] && !voxels.Contains(v))
        //                    voxels.Add(v);
        //            }
        //    return false;
        //});
        //
        //float rayLength = velocity;
        //
        //foreach(Vector3Int v in voxels)
        //{
        //    Gizmos.color = new Color(1, 1, 1, .125f);
        //    DrawCube(v.x, v.y, v.z, 1, 1, 1);
        //    if (Piksol.Geom.SphereCastAABB(rayOrigin, rayDirection, radius, rayLength, v, Vector3.one, out Vector3 collisionPoint, out Vector3 normal, out float collisionDistanceAlongRay))
        //    {
        //        rayLength = collisionDistanceAlongRay;
        //        Gizmos.DrawSphere(rayOrigin + rayDirection * collisionDistanceAlongRay, radiusOnBlock ? .01f : radius);
        //        Gizmos.color = Color.green;
        //        Gizmos.DrawSphere(collisionPoint, .01f);
        //        Gizmos.DrawLine(rayOrigin + rayDirection * collisionDistanceAlongRay, rayOrigin + rayDirection * collisionDistanceAlongRay + normal);
        //
        //
        //        float bouncedVelocity = (velocity - collisionDistanceAlongRay);
        //
        //        Gizmos.color = Color.yellow;
        //        Vector3 reflect = Vector3.Reflect(rayDirection * bouncedVelocity, normal);
        //        Gizmos.DrawLine(rayOrigin + rayDirection * collisionDistanceAlongRay, rayOrigin + rayDirection * collisionDistanceAlongRay + reflect);
        //        Gizmos.DrawSphere(rayOrigin + rayDirection * collisionDistanceAlongRay + reflect, radiusOnBlock ? .01f : radius);
        //
        //        Gizmos.color = Color.cyan;
        //        //The vector tangent to the normal.
        //        //Vector3 tangent = new Vector2(normal.y, -normal.x);
        //        Vector3 slide = Vector3.ProjectOnPlane(rayDirection, normal).normalized * bouncedVelocity;
        //
        //        Gizmos.DrawLine(rayOrigin + rayDirection * collisionDistanceAlongRay, rayOrigin + rayDirection * collisionDistanceAlongRay + slide);
        //        Gizmos.DrawSphere(rayOrigin + rayDirection * collisionDistanceAlongRay + slide, radiusOnBlock ? .01f : radius);
        //    }
        //}

        if (Piksol.Geom.SphereCastAABB(rayOrigin, rayDirection, radius, velocity, block, blockSize, out Vector3 collisionPoint, out Vector3 normal, out float collisionDistanceAlongRay))
        {
            Gizmos.DrawSphere(rayOrigin + rayDirection * collisionDistanceAlongRay, radiusOnBlock ? .01f : radius);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(collisionPoint, .01f);
            Gizmos.DrawLine(rayOrigin + rayDirection * collisionDistanceAlongRay, rayOrigin + rayDirection * collisionDistanceAlongRay + normal);
        
        
            float bouncedVelocity = (velocity - collisionDistanceAlongRay);
        
            Gizmos.color = Color.yellow;
            Vector3 reflect = Vector3.Reflect(rayDirection * bouncedVelocity, normal);
            Gizmos.DrawLine(rayOrigin + rayDirection * collisionDistanceAlongRay, rayOrigin + rayDirection * collisionDistanceAlongRay + reflect);
            Gizmos.DrawSphere(rayOrigin + rayDirection * collisionDistanceAlongRay + reflect, radiusOnBlock ? .01f : radius);
        
            Gizmos.color = Color.cyan;
            //The vector tangent to the normal.
            //Vector3 tangent = new Vector2(normal.y, -normal.x);
            Vector3 slide = Vector3.ProjectOnPlane(rayDirection, normal).normalized * bouncedVelocity;
        
            Gizmos.DrawLine(rayOrigin + rayDirection * collisionDistanceAlongRay, rayOrigin + rayDirection * collisionDistanceAlongRay + slide);
            Gizmos.DrawSphere(rayOrigin + rayDirection * collisionDistanceAlongRay + slide, radiusOnBlock ? .01f : radius);
        }
    }

    public static void CreateCylinderMesh()
    {
        GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinderMesh = p.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(p);
    }

    public static void DrawCube(float x, float y, float z, float xSize, float ySize, float zSize)
    {
        Gizmos.DrawCube(new Vector3(x + xSize * .5f, y + ySize * .5f, z + zSize * .5f), new Vector3(xSize, ySize, zSize));
        Gizmos.DrawWireCube(new Vector3(x + xSize * .5f, y + ySize * .5f, z + zSize * .5f), new Vector3(xSize, ySize, zSize));
    }

    public static void DrawCylinder(Vector3 p0, Vector3 p1, float radius)
    {
        if (cylinderMesh == null)
            CreateCylinderMesh();

        float length = (p1 - p0).magnitude;
        Vector3 direction = (p1 - p0).normalized;

        Vector3 position = p0 + direction * length / 2;
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right);
        Vector3 scale = new Vector3(radius * 2, length / 2, radius * 2);

        Gizmos.DrawMesh(cylinderMesh, position, rotation, scale);
    }
}
