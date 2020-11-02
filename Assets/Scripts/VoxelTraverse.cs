using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    public class VoxelTraverse : MonoBehaviour
    {
        public delegate bool Callback(Vector3Int block, Vector3 intersection, Vector3 normal);

        public static void Ray(Ray ray, float maxDistance, Vector3 voxelSize, Vector3 voxelOffset, Callback callback)
        {
            Ray(ray.origin, ray.direction, maxDistance, voxelSize, voxelOffset, callback);
        }

        public static void Ray(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance, Vector3 voxelSize, Vector3 voxelOffset, Callback callback)
        {
            Vector3 p0 = rayOrigin;
            Vector3 p1 = rayOrigin + rayDirection * maxDistance;

            Vector3 Vector3Abs(Vector3 a) => new Vector3(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z));

            // voxelOffset -= new Vector3(.5f, .5f, .5f);

            p0.x /= voxelSize.x;
            p0.y /= voxelSize.y;
            p0.z /= voxelSize.z;

            p1.x /= voxelSize.x;
            p1.y /= voxelSize.y;
            p1.z /= voxelSize.z;

            p0 -= voxelOffset;
            p1 -= voxelOffset;

            Vector3 rd = p1 - p0;
            Vector3 p = new Vector3(Mathf.Floor(p0.x), Mathf.Floor(p0.y), Mathf.Floor(p0.z));
            Vector3 rdinv = new Vector3(1f / rd.x, 1f / rd.y, 1f / rd.z);
            Vector3 stp = new Vector3(Mathf.Sign(rd.x), Mathf.Sign(rd.y), Mathf.Sign(rd.z));
            Vector3 delta = Vector3.Min(Vector3.Scale(rdinv, stp), Vector3.one);
            Vector3 t_max = Vector3Abs(Vector3.Scale((p + Vector3.Max(stp, Vector3.zero) - p0), rdinv));

            Vector3Int square;
            Vector3 intersection;
            Vector3 normal;
            float next_t;

            int i = 0;
            while (i < 1000)
            {
                i++;
                square = Vector3Int.RoundToInt(p);

                next_t = Mathf.Min(Mathf.Min(t_max.x, t_max.y), t_max.z);
                intersection = p0 + next_t * rd;

                if (next_t == t_max.x)
                {
                    t_max.x += delta.x;
                    p.x += stp.x;
                    normal = Vector3.right * Mathf.Sign(delta.x);
                }
                else if (next_t == t_max.y)
                {
                    t_max.y += delta.y;
                    p.y += stp.y;
                    normal = Vector3.up * Mathf.Sign(delta.y);
                }
                else
                {
                    t_max.z += delta.z;
                    p.z += stp.z;
                    normal = Vector3.forward * Mathf.Sign(delta.z);
                }

                if (callback(square, intersection, normal))
                    break;

                if (next_t > 1.0)
                    break;
            }
        }

        public static List<Vector3Int> Line(Vector3 p0, Vector3 p1, Vector3 voxelSize, Vector3 voxelOffset)
        {
            List<Vector3Int> line = new List<Vector3Int>();
            Vector3 Vector3Abs(Vector3 a) => new Vector3(Mathf.Abs(a.x), Mathf.Abs(a.y), Mathf.Abs(a.z));

            // voxelOffset -= new Vector3(.5f, .5f, .5f);

            p0.x /= voxelSize.x;
            p0.y /= voxelSize.y;
            p0.z /= voxelSize.z;

            p1.x /= voxelSize.x;
            p1.y /= voxelSize.y;
            p1.z /= voxelSize.z;

            p0 -= voxelOffset;
            p1 -= voxelOffset;

            Vector3 rd = p1 - p0;
            Vector3 p = new Vector3(Mathf.Floor(p0.x), Mathf.Floor(p0.y), Mathf.Floor(p0.z));
            Vector3 rdinv = new Vector3(1f / rd.x, 1f / rd.y, 1f / rd.z);
            Vector3 stp = new Vector3(Mathf.Sign(rd.x), Mathf.Sign(rd.y), Mathf.Sign(rd.z));
            Vector3 delta = Vector3.Min(Vector3.Scale(rdinv, stp), Vector3.one);
            Vector3 t_max = Vector3Abs(Vector3.Scale((p + Vector3.Max(stp, Vector3.zero) - p0), rdinv));
            int i = 0;
            while (i < 1000)
            {
                i++;
                Vector3Int square = Vector3Int.RoundToInt(p);
                line.Add(square);

                float next_t = Mathf.Min(Mathf.Min(t_max.x, t_max.y), t_max.z);
                if (next_t > 1.0) break;
                //Vector2 intersection = p0 + next_t * rd;



                Vector3 cmp = new Vector3(0, 0, 0);//new Vector3(Step(t_max.x, t_max.y), Step(t_max.y, t_max.x), );

                if (t_max.x < t_max.y && t_max.x < t_max.z)
                    cmp.x = 1;
                else if (t_max.y < t_max.z && t_max.y < t_max.x)
                    cmp.y = 1;
                else
                    cmp.z = 1;

                t_max += Vector3.Scale(delta, cmp);
                p += Vector3.Scale(stp, cmp);
            }

            return line;
        }
    }
}
