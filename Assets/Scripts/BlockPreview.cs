using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piksol
{

    public class BlockPreview : MonoBehaviour
    {
        public Material material;
        public int size = 512;
        public Texture2D[] previews;

        private new Camera camera;

        public void Awake()
        {
            RenderPreviews();
        }

        [ContextMenu("Render Previews")]
        public void RenderPreviews()
        {
            if (previews != null)
                for (int i = previews.Length - 1; i >= 0; i--)
                    if (previews[i] != null)
                        DestroyImmediate(previews[i]);

            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.layer = LayerMask.NameToLayer("Block Preview");
            block.GetComponent<MeshRenderer>().sharedMaterial = material;

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = new RenderTexture(size, size, 0);
            camera = GetComponent<Camera>();
            camera.enabled = true;
            camera.targetTexture = RenderTexture.active;
            previews = new Texture2D[16 * 16];

            material.mainTextureScale = Vector2.one * 1f / 16f;

            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                {
                    material.mainTextureOffset = new Vector2(x / 16f, y / 16f);
                    camera.Render();
                    previews[x + y * 16] = new Texture2D(size, size);
                    previews[x + y * 16].ReadPixels(new Rect(0, 0, size, size), 0, 0, false);
                    previews[x + y * 16].Apply();
                }
            RenderTexture.active = prev;
            DestroyImmediate(block);
            camera.enabled = false;
        }
    }
}
