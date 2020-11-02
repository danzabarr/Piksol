using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    [RequireComponent(typeof(Body))]
    public class Player : NetworkBehaviour
    {
        public enum Movement
        {
            Creative,
            SimplePhysics,
            RigidbodyPhysics,
        }

        private Body body;
        private Rigidbody rb;
        private new Camera camera;
        private float pitchDegrees, yawDegrees;

        public Transform yaw;
        public Transform pitch;

        public Movement movementMode;
        public float acceleration;
        public Vector2 jump;
        public float gravity;
        private Vector3 velocity;
        [Range(0, 1)] public float drag;

        [ReadOnly, SerializeField] private bool grounded;
        public Vector2 mouseSensitivity;
        public bool paused;

        public float range;

        [ReadOnly, SerializeField] private Direction blockFace;

        public float breakSpeed;
        public float placeSpeed;

        public int placing;
        [ReadOnly, SerializeField] private int breaking;

        [ReadOnly, SerializeField] private Vector3Int placePosition;
        [ReadOnly, SerializeField] private Vector3Int breakPosition;
        [ReadOnly, SerializeField] private Vector3Int currentBreakPosition;

        [ReadOnly, SerializeField] private float breakCounter;
        [ReadOnly, SerializeField] private float placeCounter;

        private void Start()
        {
            body = GetComponent<Body>();
            camera = GetComponentInChildren<Camera>();
            if (!isLocalPlayer)
                camera.enabled = false;

            if (isLocalPlayer)
            {
                Debug.Log("Requesting chunks for new player...");
                for (int x = -2; x <= 2; x++)
                    for (int y = -2; y <= 2; y++)
                        for (int i = 0; i < 4; i++)
                            CmdRequestChunk(x, y, 4096 * i, 4096, false);
            }
        }
        
        [Command]
        public void CmdSetBlock(Vector3Int block, int id)
        {
            RpcBlockUpdate(block, id);
        }

        [Command]
        public void CmdRequestChunk(int x, int y, int index, int length, bool sendAir)
        {
            if (!World.Instance.RequestChunkData(x, y, index, length, sendAir, out int[] blockData))
                return;
            TargetChunkUpdate(connectionToClient, x, y, blockData, index);
        }

        [ClientRpc]
        public void RpcBlockUpdate(Vector3Int block, int id)
        {
            World.Instance.SetBlock(block, id);
        }

        [TargetRpc]
        public void TargetChunkUpdate(NetworkConnection connection, int x, int y, int[] blockData, int index)
        {
            World.Instance.WriteToChunk(x, y, blockData, index);
        }

        [Client]
        private void Update()
        {
            if (!isLocalPlayer)
                return;

            #region Mouse Camera Rotation

            yawDegrees += Input.GetAxis("Mouse X") * mouseSensitivity.x;
            pitchDegrees -= Input.GetAxis("Mouse Y") * mouseSensitivity.y;
            pitchDegrees = Mathf.Clamp(pitchDegrees, -90, 90);

            yaw.localEulerAngles = new Vector3(0, yawDegrees, 0);
            pitch.localEulerAngles = new Vector3(pitchDegrees, 0, 0);

            #endregion

            #region Check Grounded

            grounded = World.IsCollidable(World.Instance.GetBlock(Vector3Int.FloorToInt(transform.position + new Vector3(0, -1.25f, 0) * body.radius)))
                    || World.IsCollidable(World.Instance.GetBlock(Vector3Int.FloorToInt(transform.position + new Vector3(+1f, -1.25f, +1f) * body.radius)))
                    || World.IsCollidable(World.Instance.GetBlock(Vector3Int.FloorToInt(transform.position + new Vector3(-1f, -1.25f, +1f) * body.radius)))
                    || World.IsCollidable(World.Instance.GetBlock(Vector3Int.FloorToInt(transform.position + new Vector3(-1f, -1.25f, -1f) * body.radius)))
                    || World.IsCollidable(World.Instance.GetBlock(Vector3Int.FloorToInt(transform.position + new Vector3(+1f, -1.25f, -1f) * body.radius)));

            #endregion

            #region Movement

            switch (movementMode)
            {
                case Movement.Creative:
                    {
                        body.enabled = true;
                        body.gravity = 0;

                        Vector3 move = default;
                        move += transform.forward * Input.GetAxis("Vertical");
                        move += transform.right * Input.GetAxis("Horizontal");
                        move += transform.up * (Input.GetKey(KeyCode.Space) ? 1 : 0);
                        move -= transform.up * (Input.GetKey(KeyCode.LeftShift) ? 1 : 0);
                        move.Normalize();
                        move *= acceleration;
                        move *= Time.deltaTime;

                        body.ApplyForce(move);
                    }
                    break;
                case Movement.SimplePhysics:
                    {
                        body.enabled = false;

                        if (grounded)
                        {
                            velocity += transform.forward * Input.GetAxis("Vertical");
                            velocity += transform.right * Input.GetAxis("Horizontal");
                            velocity.Normalize();
                            velocity *= acceleration;
                            velocity *= Time.deltaTime;

                            if (Input.GetKeyDown(KeyCode.Space))
                                velocity += Vector3.up * jump.y + transform.forward * Input.GetAxis("Vertical") * jump.x;
                        }
                        else
                        {
                            velocity += Vector3.down * gravity * Time.deltaTime;
                        }
                        Vector3 moveX = new Vector3(velocity.x, 0, 0);
                        Vector3 moveY = new Vector3(0, velocity.y, 0);
                        Vector3 moveZ = new Vector3(0, 0, velocity.z);

                        //do simple collision detection

                        Vector3Int xBlock = Vector3Int.FloorToInt(transform.position + moveX);
                        if (!World.IsCollidable(World.Instance.GetBlock(xBlock)))
                            transform.position += moveX;

                        Vector3Int yBlock = Vector3Int.FloorToInt(transform.position + moveY);
                        if (!World.IsCollidable(World.Instance.GetBlock(yBlock)))
                            transform.position += moveY;

                        Vector3Int zBlock = Vector3Int.FloorToInt(transform.position + moveZ);
                        if (!World.IsCollidable(World.Instance.GetBlock(zBlock)))
                            transform.position += moveZ;

                        velocity *= Mathf.Pow(drag, Time.deltaTime);
                    }
                        
                    break;

                case Movement.RigidbodyPhysics:
                    {
                        body.enabled = false;
                        body.gravity = grounded ? 0 : gravity;

                        if (rb == null)
                            rb = GetComponent<Rigidbody>();

                        if (grounded)
                        {
                            Vector3 move = default;
                            move += transform.forward * Input.GetAxis("Vertical");
                            move += transform.right * Input.GetAxis("Horizontal");
                            move.Normalize();
                            move *= acceleration;
                            move *= Time.deltaTime;
                            rb.AddForce(move);

                            if (Input.GetKeyDown(KeyCode.Space))
                                rb.AddForce(Vector3.up * jump.y + transform.forward * Input.GetAxis("Vertical") * jump.x);
                        }
                    }
                    break;
            }

            #endregion

            #region Break/Place/Pick Block

            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));

            blockFace = Direction.None;
            bool validPlace = false;
            bool validBreak = false;
            placePosition = default;
            breakPosition = default;
            breaking = 0;

            VoxelTraverse.Ray(ray, range, Vector3.one, Vector3.zero, (Vector3Int voxel, Vector3 intersection, Vector3 normal) =>
            {
                int block = World.Instance.GetBlock(voxel);

                if (block == 0)
                {
                    validPlace = true;
                    placePosition = voxel;
                }
                else if (block != -1)
                {
                    validBreak = true;
                    breakPosition = voxel;
                    breaking = block;

                    if (validPlace)
                    {
                        if (placePosition.x == voxel.x - 1)
                            blockFace = Direction.East;

                        else if (placePosition.x == voxel.x + 1)
                            blockFace = Direction.West;

                        if (placePosition.y == voxel.y - 1)
                            blockFace = Direction.Up;

                        else if (placePosition.y == voxel.y + 1)
                            blockFace = Direction.Down;

                        if (placePosition.z == voxel.z - 1)
                            blockFace = Direction.North;

                        else if (placePosition.z == voxel.z + 1)
                            blockFace = Direction.South;
                    }

                    return true;
                }
                return false;
            });

            #endregion

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

            if (breakPosition != currentBreakPosition)
            {
                breakCounter = 0;
                currentBreakPosition = breakPosition;
            }

            if (Input.GetMouseButton(0) && validBreak)
            {
                breakCounter += Time.deltaTime * breakSpeed * World.BreakSpeed(breaking);
                if (breakCounter >= 1)
                {
                    breakCounter = 0;
                    //World.Instance.SetBlock(breakPosition, 0);
                    CmdSetBlock(breakPosition, 0);
                }
            }
            else
                breakCounter = 0;

            if (Input.GetMouseButton(1) && validPlace && validBreak)
            {
                placeCounter += Time.deltaTime * placeSpeed;
                if (placeCounter >= 1)
                {
                    placeCounter = 0;
                    //World.Instance.SetBlock(placePosition, placing);
                    CmdSetBlock(placePosition, placing);
                }
            }
            else 
                placeCounter = 1;

            if (Input.GetKeyDown(KeyCode.F) && validBreak)
            {
                placing = breaking;
            }
        }
    }
}
