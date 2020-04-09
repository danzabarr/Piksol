using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{
    [RequireComponent(typeof(Body))]
    public class Player : NetworkBehaviour
    {
        private Body body;
        private new Camera camera;
        private float pitchDegrees, yawDegrees;

        public Transform yaw;
        public Transform pitch;

        public float acceleration;
        public int placing;
        public float range;
        public Vector2 mouseSensitivity;
        public bool paused;

        [ReadOnly] public Direction blockFace;
        [ReadOnly] public Vector3Int placePosition;
        [ReadOnly] public Vector3Int breakPosition;

        //public delegate void BlockUpdate(Vector3Int block, int id);

        private void Start()
        {
            body = GetComponent<Body>();
            camera = GetComponentInChildren<Camera>();
            if (!isLocalPlayer)
                camera.enabled = false;

            //if (isLocalPlayer)
            //{
            //    Debug.Log("Sending chunk requests");
            //
            //    for (int x = -5; x < 5; x++)
            //        for (int y = -5; y < 5; y++)
            //            CmdRequestChunk(x, y, 0, 16384);
            //}
        }
        
        [Command]
        public void CmdSetBlock(Vector3Int block, int id)
        {
            RpcBlockUpdate(block, id);
        }

        [Command]
        public void CmdRequestChunk(int x, int y, int index, int length)
        {
            TargetChunkUpdate(connectionToClient, x, y, World.Instance.RequestChunkData(x, y, index, length), index);
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

        public void SetPause(bool pause)
        {
            paused = pause;
            if (pause)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        [Client]
        private void Update()
        {
            if (!isLocalPlayer)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
                SetPause(!paused);

            if (!NetworkClient.isConnected)
                SetPause(true);

            if (Input.GetKeyDown(KeyCode.F))
                Screen.fullScreen = !Screen.fullScreen;

            if (paused)
                return;
            else
                Cursor.lockState = CursorLockMode.Locked;

            yawDegrees += Input.GetAxis("Mouse X") * mouseSensitivity.x;
            pitchDegrees -= Input.GetAxis("Mouse Y") * mouseSensitivity.y;
            pitchDegrees = Mathf.Clamp(pitchDegrees, -90, 90);

            yaw.localEulerAngles = new Vector3(0, yawDegrees, 0);
            pitch.localEulerAngles = new Vector3(pitchDegrees, 0, 0);

            Vector3 move = default;
            move += transform.forward * Input.GetAxis("Vertical");
            move += transform.right * Input.GetAxis("Horizontal");
            move += transform.up * (Input.GetKey(KeyCode.Space) ? 1 : 0);
            move -= transform.up * (Input.GetKey(KeyCode.LeftShift) ? 1 : 0);
            move.Normalize();
            move *= acceleration;
            move *= Time.deltaTime;

            body.ApplyForce(move);

            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));

            blockFace = Direction.None;
    
            bool validPlace = false;
            bool validBreak = false;
            placePosition = default;
            breakPosition = default;


            VoxelTraverse.Ray(ray, range, Vector3.one, Vector3.zero, (Vector3Int voxel, Vector3 intersection) =>
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

            if (Input.GetMouseButtonDown(0) && validBreak)
            {
                //World.Instance.SetBlock(breakPosition, 0);
                CmdSetBlock(breakPosition, 0);
            }
            if (Input.GetMouseButtonDown(1) && validPlace && validBreak)
            {
                //World.Instance.SetBlock(placePosition, placing);
                CmdSetBlock(placePosition, placing);
            }
        }
    }
}
