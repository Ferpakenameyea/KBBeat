using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RainbowControl : MonoBehaviour
{
    [SerializeField] private MeshRenderer renderer1;
    [SerializeField] private MeshRenderer renderer2;

    [SerializeField] private Vector4 weights;
    [SerializeField] private float amplitude;
    [SerializeField] private float frequency;
    [SerializeField] private float phase;

    private void Sync(Material material) {
        material.SetVector("_Weights", weights);
        material.SetFloat("_A", amplitude);
        material.SetFloat("_Omega", frequency);
        material.SetFloat("_Phi", phase);
    }

    private void Update()
    {
        if (renderer1 != null) {
            Sync(renderer1.sharedMaterial);
        }
        if (renderer2 != null) {
            Sync(renderer2.sharedMaterial);
        }
    }
}
