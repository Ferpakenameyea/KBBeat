using System;
using KBBeat.Modding.Lua;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class KBBeatLuaScriptPlayableBehaviour : PlayableBehaviour
{
#region Overrides
    public LuaVariableCurve[] Ints { get; set; }
    public LuaVariableCurve[] Floats { get; set; }
    public LuaVariableCurve[] Doubles { get; set; }
    public LuaVariableCurve[] Bools { get; set; }
    public LuaVariableCurveVector3[] Vectors { get; set; }
    public LuaVariableCurveColor[] Colors { get; set; }
#endregion

    private KBBeatLuaBehaviour target;
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            return;
        }

        target = playerData as KBBeatLuaBehaviour;

        var time = (float)playable.GetTime();
        OverrideVariableTable(Ints, time, target);
        OverrideVariableTable(Floats, time, target);
        OverrideVariableTable(Doubles, time, target);
        OverrideVariableTable(Bools, time, target);

        foreach (var vec in Vectors)
        {
            try 
            {
                target.ScopeTable.Set(vec.fieldName, new Vector3(vec.x.Evaluate(time), vec.y.Evaluate(time), vec.z.Evaluate(time)));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        foreach (var col in Colors)
        {
            try
            {
                target.ScopeTable.Set(col.fieldName, new Color(col.r.Evaluate(time), col.g.Evaluate(time), col.b.Evaluate(time), col.a.Evaluate(time)));
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

    }

    private void OverrideVariableTable(LuaVariableCurve[] array, float time, KBBeatLuaBehaviour target)
    {
        foreach (var item in array)
        {
            try
            {
                float value = item.curve.Evaluate(time);
                target.ScopeTable.Set(item.fieldName, value);
                Debug.LogFormat("Override field {0} to {1}", item.fieldName, value);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (this.target != null && !Application.isPlaying)
        {
            this.target.ResetInitializationVariables();
        }
    }
}
