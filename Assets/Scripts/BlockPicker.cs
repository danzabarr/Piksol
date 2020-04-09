using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Piksol
{
    public class BlockPicker : MonoBehaviour
    {
        public ToggleGroup toggleGroup;
        public BlockPreview previewer;
        public BlockEditor editor;
        public Toggle buttonPrefab;

        public void Start()
        {
            CreateButtons();
        }

        public void CreateButtons()
        {
            for (int i = 0; i < 16 * 16; i++)
            {
                Toggle toggle = Instantiate(buttonPrefab, transform);
                toggle.image.sprite = Sprite.Create(previewer.previews[i], new Rect(0, 0, previewer.size, previewer.size), Vector2.one * .5f);
                toggle.group = toggleGroup;
                byte id = (byte)i;
                toggle.onValueChanged.AddListener(delegate
                {
                    editor.placing = id;
                });
            }
        }
    }
}
