using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using static KBBeat.Core.InPlayingEnvironment;
using KBBeat.Core;
using KBBeat.Configuring;
using KBBeat.Audio;

internal class PreviewNoteObject : MonoBehaviour
{
    // auto injected
    private ObjectPool<PreviewNoteObject> pool;
    
    private Note note;
    private float cycleStartTime;
    private float generateTime;
    private Vector3 appear;
    private Vector3 speed;
    private Track track;

    private void Start()
    {
        if (pool == null)
        {
            throw new UnityException("note pool not injected by GamingPreview");
        }
    }
    internal void Launch(Note note, float cycleStartTime, Vector3 appear, Track track)
    {
        this.note = note;
        this.cycleStartTime = cycleStartTime;
        this.appear = appear;
        this.track = track;
        this.generateTime = Time.time;

        this.transform.parent = track.transform;
        this.transform.localPosition = appear;
        this.speed = -(appear / BuiltInSettings.MoveTime);
    }

    private void Update()
    {
        float offset = BuiltInSettings.systemOffsetSeconds + Configurator.Instance.Config.customOffsetSeconds;
        
        float timeToDeath = cycleStartTime + this.note.StrikeTime + offset - Time.time;

        this.transform.SetLocalPositionAndRotation(
            Vector3.zero - this.speed * timeToDeath,
            Quaternion.Euler(0f, 0f, 0f));

        if (timeToDeath <= 0)
        {
            this.pool.Release(this);
            this.track.Spark();
            Debug.Log($"Preview strike at {note.StrikeTime + offset}, passed {Time.time - cycleStartTime}");
            Debug.Log($"Survive time: {Time.time - this.generateTime}, death at {this.appear + this.speed * (Time.time - this.generateTime)}");

            SoundManager.Instance.PlayClip("strike", Channel.Effect);
            return;
        }

    }
}