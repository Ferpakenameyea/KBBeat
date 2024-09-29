using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ClearFlagsControl : MonoBehaviour
{
    [SerializeField] private CameraClearFlags cameraClearFlags = CameraClearFlags.SolidColor;

    private void Update() 
    {
        Camera.main.clearFlags = this.cameraClearFlags;    
    }
}
