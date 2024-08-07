using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AssetBundles.levels.level0
{
    [RequireComponent(typeof(TextMeshPro))]
    public class TextChanger : MonoBehaviour
    {
        private TextMeshPro textMeshPro;

        private void Start()
        {
            this.textMeshPro = GetComponent<TextMeshPro>();
        }

        public void ChangeText(string text)
        {
            if (this.textMeshPro == null && Application.isEditor)
            {
                this.textMeshPro = GetComponent<TextMeshPro>();   
            }

            this.textMeshPro.text = text;
        }
    }
}
