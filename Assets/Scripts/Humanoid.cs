using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    public class Humanoid : Model
    {
        private static Bone bonePrefab;

        private HumanoidData data;

        public Bone Body
        {
            get => bones[0];
            private set => bones[0] = value;
        }
        public Bone Head
        {
            get => bones[1];
            private set => bones[1] = value;
        }
        public Bone LeftArm
        {
            get => bones[2];
            private set => bones[2] = value;
        }
        public Bone RightArm
        {
            get => bones[3];
            private set => bones[3] = value;
        }
        public Bone LeftLeg
        {
            get => bones[4];
            private set => bones[4] = value;
        }
        public Bone RightLeg
        {
            get => bones[5];
            private set => bones[5] = value;
        }

        private bool initialized;

        public void Awake()
        {
            LoadEmpty();
        }

        private void Init()
        {
            bones = new Bone[6];

            if (bonePrefab == null)
                bonePrefab = Resources.Load<Bone>("Bone");

            data.body.Origin = (data.body.LowerInset + Vector3.Scale(data.body.InsetSize, new Vector3(.5f, 0, .5f))) * BlockObject.VoxelSize;
            data.head.Origin = (data.head.LowerInset + Vector3.Scale(data.head.InsetSize, new Vector3(.5f, 0, .5f))) * BlockObject.VoxelSize;
            data.armLeft.Origin = (data.armLeft.LowerInset + Vector3.Scale(data.armLeft.InsetSize, new Vector3(.5f, 1, .5f))) * BlockObject.VoxelSize;
            data.armRight.Origin = (data.armRight.LowerInset + Vector3.Scale(data.armRight.InsetSize, new Vector3(.5f, 1, .5f))) * BlockObject.VoxelSize;
            data.legLeft.Origin = (data.legLeft.LowerInset + Vector3.Scale(data.legLeft.InsetSize, new Vector3(.5f, 1, .5f))) * BlockObject.VoxelSize;
            data.legRight.Origin = (data.legRight.LowerInset + Vector3.Scale(data.legRight.InsetSize, new Vector3(.5f, 1, .5f))) * BlockObject.VoxelSize;

            if (data.body.IsDirty) data.body.RegenerateMesh();
            if (data.head.IsDirty) data.head.RegenerateMesh();
            if (data.armLeft.IsDirty) data.armLeft.RegenerateMesh();
            if (data.armRight.IsDirty) data.armRight.RegenerateMesh();
            if (data.legLeft.IsDirty) data.legLeft.RegenerateMesh();
            if (data.legRight.IsDirty) data.legRight.RegenerateMesh();

            Body = Instantiate(bonePrefab, transform);
            Head = Instantiate(bonePrefab, Body.transform);
            LeftArm = Instantiate(bonePrefab, Body.transform);
            RightArm = Instantiate(bonePrefab, Body.transform);
            LeftLeg = Instantiate(bonePrefab, Body.transform);
            RightLeg = Instantiate(bonePrefab, Body.transform);

            Body.name = "Body";
            Head.name = "Head";
            LeftArm.name = "Left Arm";
            RightArm.name = "Right Arm";
            LeftLeg.name = "Left Leg";
            RightLeg.name = "Right Leg";

            Body.Block = data.body;
            Head.Block = data.head;
            LeftArm.Block = data.armLeft;
            RightArm.Block = data.armRight;
            LeftLeg.Block = data.legLeft;
            RightLeg.Block = data.legRight;

            ResetPositions();
            
            initialized = true;
        }

        public override void ResetPositions()
        {
            Body.transform.localPosition = Vector3.zero;
            Head.transform.localPosition = new Vector3(0, data.body.InsetSize.y * BlockObject.VoxelSize + data.headOffsetY, data.headOffsetZ);
            LeftArm.transform.localPosition = new Vector3(-(data.body.InsetSize.x / 2f * BlockObject.VoxelSize + data.armOffsetX), data.body.InsetSize.y * BlockObject.VoxelSize + data.armOffsetY, data.armOffsetZ);
            RightArm.transform.localPosition = new Vector3((data.body.InsetSize.x / 2f * BlockObject.VoxelSize + data.armOffsetX), data.body.InsetSize.y * BlockObject.VoxelSize + data.armOffsetY, data.armOffsetZ);
            LeftLeg.transform.localPosition = new Vector3(-(data.legLeft.InsetSize.x / 2f * BlockObject.VoxelSize + data.legOffsetX), data.legOffsetY, data.legOffsetZ);
            RightLeg.transform.localPosition = new Vector3((data.legRight.InsetSize.x / 2f * BlockObject.VoxelSize + data.legOffsetX), data.legOffsetY, data.legOffsetZ);

            data.body.Origin = (data.body.LowerInset + Vector3.Scale(data.body.InsetSize, new Vector3(.5f, 0, .5f))) * BlockObject.VoxelSize;
            data.head.Origin = new Vector3(data.head.LowerInset.x + data.head.InsetSize.x * .5f, data.head.LowerInset.y, data.head.Size.z * .5f) * BlockObject.VoxelSize;
            data.armLeft.Origin = (data.armLeft.LowerInset + Vector3.Scale(data.armLeft.InsetSize, new Vector3(.5f, 1, .5f))) * BlockObject.VoxelSize;
            data.armRight.Origin = (data.armRight.LowerInset + Vector3.Scale(data.armRight.InsetSize, new Vector3(.5f, 1, .5f))) * BlockObject.VoxelSize;
            data.legLeft.Origin = (data.legLeft.LowerInset + Vector3.Scale(data.legLeft.InsetSize, new Vector3(.5f, 1, .5f))) * BlockObject.VoxelSize;
            data.legRight.Origin = (data.legRight.LowerInset + Vector3.Scale(data.legRight.InsetSize, new Vector3(.5f, 1, .5f))) * BlockObject.VoxelSize;
        }

        public void LoadEmpty()
        {
            data = new HumanoidData()
            {
                head = new BlockObject(8, 8, 8),
                body = new BlockObject(12, 12, 6),
                armLeft = new BlockObject(4, 12, 4),
                armRight = new BlockObject(4, 12, 4),
                legLeft = new BlockObject(4, 12, 4),
                legRight = new BlockObject(4, 12, 4),
                headOffsetY = 0,
                headOffsetZ = 0,
                headYawRange = 180,
                headPitch = 0,
                headPitchRange = 180,
                armOffsetX = 0,
                armOffsetY = 0,
                armOffsetZ = 0,
                armSwingRange = 180,
                armSplay = 5,
                legOffsetX = 0,
                legOffsetY = 0,
                legOffsetZ = 0,
                legSwingRange = 180,
                legSplay = 0
            };

            Init();
        }

        public void Load(HumanoidData data)
        {
            this.data = data;
            Init();
        }

        public void OnDrawGizmos()
        {
            if (!initialized)
                return;

            if (Camera.main == null)
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Gizmos.color = Color.white;
            Head.DrawCube();
            Body.DrawCube();
            LeftArm.DrawCube();
            RightArm.DrawCube();
            LeftLeg.DrawCube();
            RightLeg.DrawCube();

            if (Raycast.Bones(ray.origin, ray.direction, float.MaxValue, bones, out Bone bone, out Vector3 intersection, out _))
            {
                Gizmos.color = Color.red;
                bone.DrawCube();
            }
        }

        public bool RaycastBones(Ray ray, out Bone bone, out Vector3 intersection, out float distance)
        {
            return Raycast.Bones(ray.origin, ray.direction, float.MaxValue, bones, out bone, out intersection, out distance);
        }
    }

    [System.Serializable]
    public class HumanoidData
    {
        public BlockObject head;
        public BlockObject body;
        public BlockObject armLeft;
        public BlockObject armRight;
        public BlockObject legLeft;
        public BlockObject legRight;

        public float headOffsetY;
        public float headOffsetZ;

        public float headYawRange;

        public float headPitch;
        public float headPitchRange;

        public float armOffsetX;
        public float armOffsetY;
        public float armOffsetZ;

        public float armSwingRange;
        public float armSplay;

        public float legOffsetX;
        public float legOffsetY;
        public float legOffsetZ;

        public float legSwingRange;
        public float legSplay;
    }
}
