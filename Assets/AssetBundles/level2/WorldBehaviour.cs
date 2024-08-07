using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundles.levels.level2
{
    public class WorldBehaviour : MonoBehaviour
    {
        [SerializeField] private Material battleMaterial;

        [SerializeField] private List<MaterialTarget> renderersToChange;

        public void ChangeToBattle()
        {
            foreach (var r in renderersToChange)
            {
                r.target.material = battleMaterial;
            }
        }

        public void ChangeToNormal()
        {
            foreach (var r in renderersToChange)
            {
                r.Reset();
            }
        }

        [Serializable]
        internal class MaterialTarget
        {
            [SerializeField] private Material originalMaterial;
            [SerializeField] internal MeshRenderer target;
            internal void Reset()
            {
                target.material = originalMaterial;
            }
        }
    }
}