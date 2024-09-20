using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(AfterEffect))]
public class ReverseCycleController : MonoBehaviour
{
    private AfterEffect afterEffect;

    [SerializeField] private float frequency;
    [SerializeField] private float mulFactor;
    [SerializeField] private float thresHold;
    [SerializeField] private float theta;
    [SerializeField] private float distanceLimit;

    [SerializeField] private Color mulColor;

    private void Start() 
    {
        this.afterEffect = GetComponent<AfterEffect>();    
    }

    private void Update() 
    {
        if (this.afterEffect.Material != null && 
            this.afterEffect.Material.shader.name.Equals("KBBeat PostEffects/levels/level1/reverse circles")) 
        {
            var m = this.afterEffect.Material;
            m.SetFloat("_Frequency", frequency);
            m.SetFloat("_MulFactor", mulFactor);
            m.SetFloat("_Threshold", thresHold);
            m.SetFloat("_Theta", theta);
            m.SetFloat("_DistanceLimit", distanceLimit); 
            m.SetColor("_MulColor", mulColor);
        }    
    }
}
