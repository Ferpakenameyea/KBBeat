using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightningController : MonoBehaviour
{
    [SerializeField] private bool syncFog = false;
    [SerializeField] private bool syncAmbientLight = false;

    [SerializeField] private float targetFogDensity;
    [SerializeField] private Color targetFogColor;

    [SerializeField] private float ambientLightIntensity;
    [SerializeField] private Color ambientLightColor;

    private void Update() 
    {
        if (syncFog) {
            RenderSettings.fogDensity = targetFogDensity;
            RenderSettings.fogColor = targetFogColor;
        }

        if (syncAmbientLight) {
            RenderSettings.ambientLight = ambientLightColor;
            RenderSettings.ambientIntensity = ambientLightIntensity;
        }
    }
}
