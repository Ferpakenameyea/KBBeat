using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class MaterialColorController : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    [SerializeField] private Color color;

    private void Start() 
    {
        this.meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update() 
    {
    #if UNITY_EDITOR
        if (this.meshRenderer == null)
        {
            this.meshRenderer = GetComponent<MeshRenderer>();
        }
        Debug.Log("Run");
    #endif

        this.meshRenderer.sharedMaterial.color = color;
    }
}