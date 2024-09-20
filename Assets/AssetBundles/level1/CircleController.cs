using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(AfterEffect))]
public class CircleController : MonoBehaviour
{
    [SerializeField] private Color mulColor = Color.white;
    [SerializeField] private float mulFactor = 1;
    [SerializeField] private float frequency = 1;
    [SerializeField] private float threshold = 0.6f;

    private AfterEffect afterEffect;

    private void Start() 
    {
        this.afterEffect = this.GetComponent<AfterEffect>();    
    }

    private void Update() 
    {
        try
        {
            var m = this.afterEffect.Material;
            if (m.shader.name.Equals("KBBeat PostEffects/levels/level1/circles")) {
                m.SetColor("_MulColor", mulColor);
                m.SetFloat("_MulFactor", mulFactor);
                m.SetFloat("_Frequency", frequency);
                m.SetFloat("_Threshold", threshold);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
            return;
        }
    }
}
