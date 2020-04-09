using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Piksol
{
    public enum Direction
    {
        None,
        North,
        East,
        South,
        West,
        Up,
        Down
    }

    public class BlockEditor : MonoBehaviour
    {
        public enum Mode
        {
            PlacePixel,
            EditBounds,
            SelectArea,
            FillArea,
        }

        public Model model;
        private Bone bone;

        public MeshFilter target;

        public Mesh cube;
        public Mesh quad;
        public Material activeBlock;
        public Material inactiveBlock;
        public Material voxelBounds;
        public Material voxelFace;

        public Mode mode;
        public int placing;
        public float breakDuration = .1f;
        public float placeDuration = .1f;

        private BlockObject block;
        private float actionCounter;

        private Vector3 targetPosition;

        public BlockObject Block
        {
            get => block;
            set
            {
                block = value;
                if (target != null)
                    target.sharedMesh = block?.Mesh;
            }
        }

        
        [Header("Rotation")]
        public Transform rotateTransform;
        public Vector2 rotateSensitivity = Vector2.one;

        [Header("Zoom")]
        public Transform zoomTransform;
        public float zoomSensitivity = 1;
        public float minZoom =  1.5f;
        public float maxZoom = 10.0f;

        private bool[] dragging = new bool[3];
        private Vector3[] dragStart = new Vector3[3];

        

        public Direction outsideFace;
        public Direction insideFace;
        public Direction draggingFace;
        public Plane plane;
        public Vector3 mouseOnBounds;
        public Vector3 boundsDrag;
        private Vector3Int startInsets;
        private float hoverVoxelCounter;
        private Vector3Int breakVoxel;

        public void RegenerateMesh()
        {
            Block.RegenerateMesh();
            target.sharedMesh = block.Mesh;
        }

        private void Awake()
        {
            //block = new BlockData(8, 8, 8);
        }

        private void Update()
        {
            #region Camera Controls

            if (Input.GetMouseButton(2))
            {
                if (!dragging[2])
                {
                    dragging[2] = true;
                    dragStart[2] = Input.mousePosition;
                }
                else
                {
                    Vector2 delta = Input.mousePosition - dragStart[2];

                    float addYaw = delta.x * rotateSensitivity.x;
                    float addPitch = -delta.y * rotateSensitivity.y;

                    rotateTransform.localRotation *= Quaternion.Euler(addPitch, addYaw, 0);

                    dragStart[2] = Input.mousePosition;
                }

            }
            else
            {
                dragging[2] = false;
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                float newZoom = Mathf.Clamp(zoomTransform.localPosition.z + Input.mouseScrollDelta.y * zoomSensitivity, -maxZoom, -minZoom);
                zoomTransform.localPosition = new Vector3(0, 0, newZoom);
            }

            #endregion

            #region Voxel Cast

            int maxRay = 50;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Vector3Int placeVoxel = default;
            Vector3Int breakVoxel = default;
            Vector3Int prevVoxel = default;

            bool validPlace = false;
            bool validBreak = false;

            Direction voxelFaceDir = Direction.None;

            float outset = .001f;

            if (!dragging[0])
                outsideFace = Direction.None;
            insideFace = Direction.None;


            if (target != null && Block != null)
            {
                Ray transformedRay = new Ray(target.transform.InverseTransformPoint(ray.origin) + Block.Origin, target.transform.InverseTransformVector(ray.direction));

                bool entered = false;

                VoxelTraverse.Ray(transformedRay, maxRay, Vector3.one * BlockObject.VoxelSize, Vector3.zero, (Vector3Int voxel, Vector3 intersection) =>
                {
                    if (voxel.x >= Block.LowerInset.x && voxel.x < Block.Size.x - Block.UpperInset.x
                     && voxel.y >= Block.LowerInset.y && voxel.y < Block.Size.y - Block.UpperInset.y
                     && voxel.z >= Block.LowerInset.z && voxel.z < Block.Size.z - Block.UpperInset.z)
                    {
                        
                        if (!entered)
                        {
                            entered = true;
                            if (prevVoxel.x == voxel.x + 1)
                                outsideFace = Direction.East;

                            else if (prevVoxel.x == voxel.x - 1)
                                outsideFace = Direction.West;

                            if (prevVoxel.y == voxel.y + 1)
                                outsideFace = Direction.Up;

                            else if (prevVoxel.y == voxel.y - 1)
                                outsideFace = Direction.Down;

                            if (prevVoxel.z == voxel.z + 1)
                                outsideFace = Direction.North;

                            else if (prevVoxel.z == voxel.z - 1)
                                outsideFace = Direction.South;
                        }

                        if (Block[voxel.x, voxel.y, voxel.z] == 0)
                        {
                            validPlace = true;
                            placeVoxel = voxel;
                        }
                        else
                        {
                            validBreak = true;
                            breakVoxel = voxel;

                            if (validPlace)
                            {
                                if (placeVoxel.x == voxel.x - 1)
                                    voxelFaceDir = Direction.East;

                                else if (placeVoxel.x == voxel.x + 1)
                                    voxelFaceDir = Direction.West;

                                if (placeVoxel.y == voxel.y - 1)
                                    voxelFaceDir = Direction.Up;

                                else if (placeVoxel.y == voxel.y + 1)
                                    voxelFaceDir = Direction.Down;

                                if (placeVoxel.z == voxel.z - 1)
                                    voxelFaceDir = Direction.North;

                                else if (placeVoxel.z == voxel.z + 1)
                                    voxelFaceDir = Direction.South;
                            }

                            return true;
                        }
                    }

                    else if (entered)
                    {
                        if (prevVoxel.x == voxel.x - 1)
                            insideFace = Direction.East;

                        else if (prevVoxel.x == voxel.x + 1)
                            insideFace = Direction.West;

                        if (prevVoxel.y == voxel.y - 1)
                            insideFace = Direction.Up;

                        else if (prevVoxel.y == voxel.y + 1)
                            insideFace = Direction.Down;

                        if (prevVoxel.z == voxel.z - 1)
                            insideFace = Direction.North;

                        else if (prevVoxel.z == voxel.z + 1)
                            insideFace = Direction.South;

                        voxelFaceDir = insideFace;

                        return true;
                    }

                    prevVoxel = voxel;
                    return false;
                });

                if (breakVoxel != this.breakVoxel || (!validBreak && !validPlace))
                {
                    hoverVoxelCounter = 0;
                    this.breakVoxel = breakVoxel;
                }
                
            }

            #endregion

            #region Mode Switch

            switch (mode)
            {
                case Mode.SelectArea:
                    {

                    }
                    break;
                case Mode.EditBounds:
                    {
                        if (Block == null)
                            break;
                        if (!dragging[0])
                        {
                            if (Input.GetMouseButtonDown(0) && outsideFace != Direction.None)
                            {
                                draggingFace = outsideFace;
                                plane = FacePlane(draggingFace);
                                if (plane.Raycast(ray, out float enter))
                                    mouseOnBounds = ray.GetPoint(enter);

                                if (draggingFace == Direction.North || draggingFace == Direction.East || draggingFace == Direction.Up)
                                    startInsets = Block.UpperInset;
                                else if (draggingFace == Direction.South || draggingFace == Direction.West || draggingFace == Direction.Down)
                                    startInsets = Block.LowerInset;
                                dragging[0] = true;
                            }
                        }
                        if (dragging[0] && draggingFace != Direction.None)
                        {
                            Plane crossPlane = new Plane(Vector3.Cross(plane.normal, transform.up), mouseOnBounds);
                            if (crossPlane.Raycast(ray, out float crossEnter))
                            {
                                Vector3 mouseOnCrossPlane = ray.GetPoint(crossEnter);
                                boundsDrag = Vector3.Project(mouseOnCrossPlane - mouseOnBounds, plane.normal);
                                Vector3 drag = boundsDrag;
                                drag.x /= target.transform.lossyScale.x;
                                drag.y /= target.transform.lossyScale.y;
                                drag.z /= target.transform.lossyScale.z;

                                if (draggingFace == Direction.North || draggingFace == Direction.East || draggingFace == Direction.Up)
                                    Block.UpperInset = startInsets + Vector3Int.RoundToInt(-drag / BlockObject.VoxelSize);

                                else if (draggingFace == Direction.South || draggingFace == Direction.West || draggingFace == Direction.Down)
                                    Block.LowerInset = startInsets + Vector3Int.RoundToInt(drag / BlockObject.VoxelSize);

                                if (model != null)
                                    model.ResetPositions();
                            }
                        }

                        if (!Input.GetMouseButton(0))
                        {
                            dragging[0] = false;
                        }
                    }
                    break;
                case Mode.PlacePixel:
                    {
                        if (Block == null)
                            break;

                        if (validBreak || validPlace)
                            hoverVoxelCounter += Time.deltaTime * 5;

                        if (validBreak)
                        {
                            voxelBounds.SetFloat("_Alpha", Mathf.Clamp(hoverVoxelCounter, 0, 1) * .5f);

                            Graphics.DrawMesh(cube, target.transform.localToWorldMatrix * Matrix4x4.TRS((Vector3)breakVoxel * (BlockObject.VoxelSize) - Vector3.one * outset - Block.Origin, Quaternion.identity, Vector3.one * (BlockObject.VoxelSize + outset * 2)), voxelBounds, LayerMask.NameToLayer("UI"));
                        }

                        if (validPlace)
                        {
                            Vector3 orientation = default;
                            Vector3 placeFace = placeVoxel;

                            if (voxelFaceDir == Direction.East)
                            {
                                orientation = -Vector3.right;
                                placeFace.x += .5f - outset;
                            }
                            else if (voxelFaceDir == Direction.West)
                            {
                                orientation = Vector3.right;
                                placeFace.x -= .5f - outset;
                            }
                            else if (voxelFaceDir == Direction.Up)
                            {
                                orientation = -Vector3.up;
                                placeFace.y += .5f - outset;
                            }
                            else if (voxelFaceDir == Direction.Down)
                            {
                                orientation = Vector3.up;
                                placeFace.y -= .5f - outset;
                            }
                            else if (voxelFaceDir == Direction.North)
                            {
                                orientation = -Vector3.forward;
                                placeFace.z += .5f - outset;
                            }
                            else if (voxelFaceDir == Direction.South)
                            {
                                orientation = Vector3.forward;
                                placeFace.z -= .5f - outset;
                            }

                            voxelFace.SetFloat("_Alpha", Mathf.Clamp(hoverVoxelCounter, 0, 1) * .5f);

                            Graphics.DrawMesh(quad,
                                target.transform.localToWorldMatrix *
                                Matrix4x4.TRS(
                                    (placeFace + Vector3.one * .5f) * BlockObject.VoxelSize - Block.Origin,
                                    Quaternion.LookRotation(orientation, Vector3.up),
                                    Vector3.one * BlockObject.VoxelSize
                                ), voxelFace, LayerMask.NameToLayer("UI"));
                        }

                        actionCounter += Time.deltaTime;

                        if (Input.GetMouseButton(0) && actionCounter >= breakDuration && validBreak)
                        {
                            Block[breakVoxel.x, breakVoxel.y, breakVoxel.z] = 0;
                            actionCounter = 0;
                        }
                        if (Input.GetMouseButton(1) && actionCounter >= placeDuration && validPlace)
                        {
                            Block[placeVoxel.x, placeVoxel.y, placeVoxel.z] = placing;
                            actionCounter = 0;
                        }
                    }
                    break;
                case Mode.FillArea:
                    {

                    }
                    break;
            }

            #endregion

            #region Voxel Picker

            if (Input.GetKeyDown(KeyCode.F) && Block != null && validBreak)
            {
                placing = Block[breakVoxel.x, breakVoxel.y, breakVoxel.z];
            }

            #endregion

            #region Voxel Selection Number Keys

            if (Input.GetKeyDown(KeyCode.Alpha1))
                placing = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                placing = 2;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                placing = 3;
            if (Input.GetKeyDown(KeyCode.Alpha4))
                placing = 4;
            if (Input.GetKeyDown(KeyCode.Alpha5))
                placing = 5;
            if (Input.GetKeyDown(KeyCode.Alpha6))
                placing = 6;
            if (Input.GetKeyDown(KeyCode.Alpha7))
                placing = 7;
            if (Input.GetKeyDown(KeyCode.Alpha8))
                placing = 8;
            if (Input.GetKeyDown(KeyCode.Alpha9))
                placing = 9;
            if (Input.GetKeyDown(KeyCode.Alpha0))
                placing = 10;

            #endregion

            #region Draw Block Grid

            void DrawBlockOutline(BlockObject Block, Transform transform)
            {
                MaterialPropertyBlock properties = new MaterialPropertyBlock();

                properties.SetVector("_Subdivisions", (Vector3)Block.InsetSize);

                Graphics.DrawMesh(cube,
                    transform.localToWorldMatrix *
                    Matrix4x4.TRS(
                        (Block.LowerInset - Vector3.one * outset) * BlockObject.VoxelSize - Block.Origin,
                        Quaternion.identity,
                        (Block.InsetSize + Vector3.one * outset * 2) * BlockObject.VoxelSize),

                    inactiveBlock, LayerMask.NameToLayer("UI"), null, 0, properties);
            }

            void DrawBlockGrid(BlockObject Block, Transform transform, Direction highlightFace)
            {

                if (Block != null)
                {
                    MaterialPropertyBlock properties = new MaterialPropertyBlock();
                    //properties.SetVector("_ModelScale", (Vector3)Block.Size * BlockData.VoxelSize);
                    //Graphics.DrawMesh(cube, target.transform.localToWorldMatrix * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, (Vector3)Block.Size * BlockData.VoxelSize), wireframe, LayerMask.NameToLayer("UI"), null, 0, properties);
                    switch (mode)
                    {
                        case Mode.PlacePixel:
                            activeBlock.SetInt("_GridCull", (int)UnityEngine.Rendering.CullMode.Front);
                            break;
                        case Mode.EditBounds:
                            activeBlock.SetInt("_GridCull", (int)UnityEngine.Rendering.CullMode.Back);

                            break;
                        case Mode.SelectArea:
                            break;
                        case Mode.FillArea:
                            break;
                    }

                    if (highlightFace == Direction.East)
                        properties.SetVector("_HighlightEUN", new Vector3(1, 0, 0));
                    else if (highlightFace == Direction.Up)
                        properties.SetVector("_HighlightEUN", new Vector3(0, 1, 0));
                    else if (highlightFace == Direction.North)
                        properties.SetVector("_HighlightEUN", new Vector3(0, 0, 1));
                    else if (highlightFace == Direction.West)
                        properties.SetVector("_HighlightWDS", new Vector3(1, 0, 0));
                    else if (highlightFace == Direction.Down)
                        properties.SetVector("_HighlightWDS", new Vector3(0, 1, 0));
                    else if (highlightFace == Direction.South)
                        properties.SetVector("_HighlightWDS", new Vector3(0, 0, 1));

                    properties.SetVector("_Subdivisions", (Vector3)Block.InsetSize);

                    Graphics.DrawMesh(cube,
                        transform.localToWorldMatrix *
                        Matrix4x4.TRS(
                            (Block.LowerInset - Vector3.one * outset) * BlockObject.VoxelSize - Block.Origin,
                            Quaternion.identity,
                            (Block.InsetSize + Vector3.one * outset * 2) * BlockObject.VoxelSize),

                        activeBlock, LayerMask.NameToLayer("UI"), null, 0, properties);
                }
            }


            if (model != null)
            {
                foreach(Bone bone in model)
                {
                    if (bone.Block == Block)
                    {
                        DrawBlockGrid(bone.Block, bone.transform, outsideFace);
                    }
                    else
                    {
                        DrawBlockOutline(bone.Block, bone.transform);
                    }
                }
            }
            else
            {
                DrawBlockGrid(Block, target.transform, outsideFace);
            }

            #endregion

            #region Select Bone
            if (!validBreak && !validPlace)
            {
                if (model is Humanoid)
                {
                    Humanoid humanoid = model as Humanoid;

                    if (Input.GetMouseButtonDown(0))
                    {
                        dragging[0] = false;

                        if (humanoid.RaycastBones(ray, out bone, out _, out _))
                        {
                            target = bone.MeshFilter;
                            Block = bone.Block;
                            humanoid.Focus(bone);
                            //mode = Mode.PlacePixel;
                        }
                        else
                        {
                            Block = null;
                            target = null;
                            humanoid.ClearFocus();
                        }
                    }
                }
            }
            #endregion

            #region Regenerate Block Mesh

            if (Block != null && Block.IsDirty)
                RegenerateMesh();

            #endregion

            #region Focus View

            if (Block != null)
            {
                targetPosition = target.transform.position - Block.Origin + ((Vector3)Block.LowerInset + (Vector3)Block.InsetSize / 2f) * BlockObject.VoxelSize;
            }
            else if (model != null)
            {
                targetPosition = model.transform.position;
            }

            transform.position = Vector3.Slerp(transform.position, targetPosition, Time.deltaTime * 5);

            #endregion
        }

        public void OnDrawGizmos()
        {
            if (Block == null)
                return;

            if (target == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(targetPosition, targetPosition + new Vector3(1, 0, 0));

            Gizmos.color = Color.green;
            Gizmos.DrawLine(targetPosition, targetPosition + new Vector3(0, 1, 0));

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(targetPosition, targetPosition + new Vector3(0, 0, 1));


            Gizmos.color = Color.gray;
            DrawCube(target.transform, Vector3.zero - Block.Origin, (Vector3) Block.Size * BlockObject.VoxelSize);

            Gizmos.color = Color.white;
            DrawCube(target.transform, (Vector3)Block.LowerInset * BlockObject.VoxelSize - Block.Origin, (Vector3)(Block.InsetSize) * BlockObject.VoxelSize);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(mouseOnBounds, .01f);
            Gizmos.DrawSphere(mouseOnBounds + boundsDrag, .01f);
            //Gizmos.DrawLine(mouseOnBounds, mouseOnBounds + plane.normal);

            Gizmos.DrawLine(mouseOnBounds, mouseOnBounds + boundsDrag);


            if (Block != null && Block.Volumes != null)
            foreach(BoundsInt v in Block.Volumes)
            {
                DrawCube(target.transform, (Vector3)v.min * BlockObject.VoxelSize - Block.Origin, (Vector3)v.size * BlockObject.VoxelSize);
            }


            //Gizmos.color = Color.red;
            //Gizmos.DrawSphere(target.transform.position - Block.Origin + (Block.LowerInset + Vector3.Scale(Block.InsetSize, new Vector3(0, .5f, .5f))) * BlockData.VoxelSize, .0125f);
            //Gizmos.DrawSphere(target.transform.position - Block.Origin + (Block.LowerInset + Vector3.Scale(Block.InsetSize, new Vector3(1, .5f, .5f))) * BlockData.VoxelSize, .0125f);
            //
            //Gizmos.color = Color.green;
            //Gizmos.DrawSphere(target.transform.position - Block.Origin + (Block.LowerInset + Vector3.Scale(Block.InsetSize, new Vector3(.5f, 0, .5f))) * BlockData.VoxelSize, .0125f);
            //Gizmos.DrawSphere(target.transform.position - Block.Origin + (Block.LowerInset + Vector3.Scale(Block.InsetSize, new Vector3(.5f, 1, .5f))) * BlockData.VoxelSize, .0125f);
            //
            //Gizmos.color = Color.blue;
            //Gizmos.DrawSphere(target.transform.position - Block.Origin + (Block.LowerInset + Vector3.Scale(Block.InsetSize, new Vector3(.5f, .5f, 0))) * BlockData.VoxelSize, .0125f);
            //Gizmos.DrawSphere(target.transform.position - Block.Origin + (Block.LowerInset + Vector3.Scale(Block.InsetSize, new Vector3(.5f, .5f, 1))) * BlockData.VoxelSize, .0125f);

            //for (int x = 0; x < Size.x; x++)
            //    for (int y = 0; y < Size.y; y++)
            //        for (int z = 0; z < Size.z; z++)
            //            if (bubbles[x, y, z] != -1)
            //                Handles.Label(new Vector3(x + .5f, y + .5f, z + .5f) * VoxelSize, bubbles[x, y, z] + "");
            //if (data[x, y, z] != 0)
            //    DrawCube(new Vector3(x, y, z) * VoxelSize, Vector3.one * VoxelSize);


           // Gizmos.color = Color.red;
           // 
           // int maxRay = 50;
           // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
           // 
           // Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * maxRay);
           // ray = new Ray(target.transform.InverseTransformPoint(ray.origin) + Block.Origin, target.transform.InverseTransformVector(ray.direction));
           //
           // bool entered = false;
           //
           // VoxelTraverse.Ray(ray, maxRay, BlockData.VoxelSize * Vector3.one, Vector3.zero, (Vector3Int voxel, Vector3 intersection) =>
           // {
           //     if (voxel.x >= Block.LowerInset.x && voxel.x < Block.Size.x - Block.UpperInset.x
           //      && voxel.y >= Block.LowerInset.y && voxel.y < Block.Size.y - Block.UpperInset.y
           //      && voxel.z >= Block.LowerInset.z && voxel.z < Block.Size.z - Block.UpperInset.z)
           //     {
           //         entered = true;
           //         DrawCube(target.transform, new Vector3(voxel.x, voxel.y, voxel.z) * BlockData.VoxelSize - Block.Origin, Vector3.one * BlockData.VoxelSize);
           //     }
           //     else if (entered)
           //         return false;
           //
           // 
           //     return false;
           // });
        }

        public static void DrawCube(Transform transform, Vector3 position, Vector3 size)
        {
            Vector3 c000 = transform.TransformPoint(position + new Vector3(0, 0, 0));
            Vector3 c001 = transform.TransformPoint(position + new Vector3(0, 0, size.z));
            Vector3 c010 = transform.TransformPoint(position + new Vector3(0, size.y, 0));
            Vector3 c011 = transform.TransformPoint(position + new Vector3(0, size.y, size.z));
            Vector3 c100 = transform.TransformPoint(position + new Vector3(size.x, 0, 0));
            Vector3 c101 = transform.TransformPoint(position + new Vector3(size.x, 0, size.z));
            Vector3 c110 = transform.TransformPoint(position + new Vector3(size.x, size.y, 0));
            Vector3 c111 = transform.TransformPoint(position + new Vector3(size.x, size.y, size.z));

            Gizmos.DrawLine(c000, c100);
            Gizmos.DrawLine(c100, c101);
            Gizmos.DrawLine(c101, c001);
            Gizmos.DrawLine(c001, c000);

            Gizmos.DrawLine(c010, c110);
            Gizmos.DrawLine(c110, c111);
            Gizmos.DrawLine(c111, c011);
            Gizmos.DrawLine(c011, c010);

            Gizmos.DrawLine(c000, c010);
            Gizmos.DrawLine(c100, c110);
            Gizmos.DrawLine(c101, c111);
            Gizmos.DrawLine(c001, c011);
        }

        public Plane FacePlane(Direction face)
        {
            Vector3 position;
            switch (face)
            {
                case Direction.None:
                    return default;
                case Direction.East:
                    position = Vector3.right * (Block.Size.x - Block.UpperInset.x) * BlockObject.VoxelSize - Block.Origin;
                    return new Plane(target.transform.right, target.transform.TransformPoint(position));

                case Direction.West:
                    position = Vector3.right * Block.LowerInset.x * BlockObject.VoxelSize - Block.Origin;
                    return new Plane(-target.transform.right, target.transform.TransformPoint(position));

                case Direction.Up:
                    position = Vector3.up * (Block.Size.y - Block.UpperInset.y) * BlockObject.VoxelSize - Block.Origin;
                    return new Plane(target.transform.up, target.transform.TransformPoint(position));

                case Direction.Down:
                    position = Vector3.up * Block.LowerInset.y * BlockObject.VoxelSize - Block.Origin;
                    return new Plane(-target.transform.up, target.transform.TransformPoint(position));

                case Direction.North:
                    position = Vector3.forward * (Block.Size.z - Block.UpperInset.z) * BlockObject.VoxelSize - Block.Origin;
                    return new Plane(target.transform.forward, target.transform.TransformPoint(position));

                case Direction.South:
                    position = Vector3.forward * Block.LowerInset.z * BlockObject.VoxelSize - Block.Origin;
                    return new Plane(-target.transform.forward, target.transform.TransformPoint(position));

                default:
                    return default;
            }
        }
    }
}
