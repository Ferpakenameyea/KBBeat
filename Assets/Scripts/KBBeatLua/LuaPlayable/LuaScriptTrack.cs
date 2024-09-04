using KBBeat.Modding.Lua;
using UnityEngine.Timeline;

[TrackColor(1f, 0.1176f, 0.6509f)]
[TrackClipType(typeof(LuaPlayableAsset))]       // 使用的切片类型是 LuaPlayableAsset
[TrackBindingType(typeof(KBBeatLuaBehaviour))]  // 目标类型是KBBeatLuaBehaviour
public class LuaScriptTrack : TrackAsset
{
    
}