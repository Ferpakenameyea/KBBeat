using System;
using KBbeat;
using UnityEngine;
using UnityEngine.Pool;

public class NoteObject : MonoBehaviour
{
    public InPlayingEnvironment.Note note { get; private set; } = null;
    public bool IsPlaying { get; private set; } = false;
    public Track track { get; internal set; } = null;
    public bool underRecording { get; internal set; } = false;
    public Directions Direction { get => this.track.parent.Group; }
    public virtual NoteType noteType { get => NoteType.UNKNOWN; }
    public delegate void ReportCallback(Score score, Latency latency, int count);
    public bool IsMissed 
    {
        get 
        {
            if (LevelPlayer.Instance.Time() > this.note?.StrikeTime)
            {
                var delay = LevelPlayer.Instance.Time() - this.note.StrikeTime;

                if (delay > BuiltInSettings.missLine)
                {
                    return true;
                }
            }
            return false;
        }
    }
    protected float TimeToHit { get => this.note.StrikeTime - LevelPlayer.Instance.Time(); }

    public void Fire(in InPlayingEnvironment.Note note, TrackGroup group)
    {
        this.note = note;
        this.IsPlaying = true;
        group.PutNoteObject(note.TrackIndex, this);
    }
    
    protected void Update()
    {
        if (!this.IsPlaying || this.underRecording)
        {
            return;
        }

        this.transform.localPosition = -TimeToHit * LevelPlayer.Instance.NoteMoveSpeed;
    }

    // return true if there's an immediate destroy
    // and it will delegate reporting score to track group
    public virtual bool Detach(Score score, Latency latency, ReportCallback reportCallback = null) 
    {
        throw new NotImplementedException("No implementation for anolymous note object");
    }

    public bool Triggered()
    {
        var delta = Mathf.Abs(this.note.StrikeTime - LevelPlayer.Instance.Time());
        if (delta < BuiltInSettings.farLine)
        {
            var hitCount = this.track.parent.Group switch
            {
                Directions.LEFT => InputManager.LeftHitCount,
                Directions.RIGHT => InputManager.RightHitCount,
                _ => 0
            };

            if (hitCount > 0)
            {
                return true;
            }
        }
        return false;
    }

    public virtual void Die() 
    {
        throw new NotImplementedException("No implementation for anolymous note object");
    }
}