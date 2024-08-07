using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static InPlayingEnvironment;

public class PreviewNoteObject : MonoBehaviour
{
    private ObjectPool pool;
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
            ObjectPool.TryGetPool("PreviewNotePool", out this.pool);
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
            this.pool.Release(this.gameObject);
            this.track.Spark();
            Debug.Log($"Preview strike at {note.StrikeTime + offset}, passed {Time.time - cycleStartTime}");
            Debug.Log($"Survive time: {Time.time - this.generateTime}, death at {this.appear + this.speed * (Time.time - this.generateTime)}");

            SoundManager.Instance.PlayClip("strike", Channel.Effect);
            return;
        }

    }
}