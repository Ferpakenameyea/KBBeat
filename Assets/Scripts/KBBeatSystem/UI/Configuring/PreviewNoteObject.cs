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
    private Vector3 previewSpeed { get => -(appear / BuiltInSettings.MoveTime); }
    public float StrikeTime { get => note.StrikeTime; }
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
    }

    private void Update()
    {
        float offset = BuiltInSettings.systemOffsetSeconds + Configurator.Instance.Config.customOffsetSeconds;
        
        float timeToDeath = cycleStartTime + this.note.StrikeTime + offset - Time.time;

        this.transform.SetLocalPositionAndRotation(
            Vector3.zero - this.previewSpeed * timeToDeath,
            Quaternion.Euler(0f, 0f, 0f));

        if (timeToDeath <= 0)
        {
            this.pool.Release(this);
            this.track.Spark();

            SoundManager.Instance.PlayClip("strike", Channel.Effect);
            return;
        }

    }
}