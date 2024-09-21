using System;
using KBBeat.Modding.Lua;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class LuaPlayableAsset : PlayableAsset, ITimelineClipAsset
{
    [SerializeField] private LuaVariableCurve[] ints;
    [SerializeField] private LuaVariableCurve[] floats;
    [SerializeField] private LuaVariableCurve[] doubles;
    [SerializeField] private LuaVariableCurve[] bools;
    [SerializeField] private LuaVariableCurveVector3[] vectors;
    [SerializeField] private LuaVariableCurveColor[] colors;

    private static readonly KBBeatLuaScriptPlayableBehaviour template = new();

    public ClipCaps clipCaps => ClipCaps.All;

    // Factory method that generates a playable based on this asset
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var playable = ScriptPlayable<KBBeatLuaScriptPlayableBehaviour>.Create(graph, template);


        var behaviour = playable.GetBehaviour();
        
        behaviour.Ints = ints;
        behaviour.Floats = floats;
        behaviour.Doubles = doubles;
        behaviour.Bools = bools;
        behaviour.Vectors = vectors;
        behaviour.Colors = colors;

        return playable;
    }
}

[Serializable]
public class LuaVariableCurve
{
    public string fieldName;
    public AnimationCurve curve;
}

[Serializable]
public class LuaVariableCurveVector3
{
    public string fieldName;
    public AnimationCurve x;
    public AnimationCurve y;
    public AnimationCurve z;
}

[Serializable]
public class LuaVariableCurveColor
{
    public string fieldName;
    public AnimationCurve r;
    public AnimationCurve g;
    public AnimationCurve b;
    public AnimationCurve a;
}