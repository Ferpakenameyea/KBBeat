using KBBeat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackGroup : MonoBehaviour
{
    [Range(1, 5)][SerializeField] private int groupSize;
    [SerializeField] private Vector3 offsetOnPressedDown;
    [SerializeField] private float lerpSpeed;
    [SerializeField] private GameObject editModeFramePrefab;
    public event Action OnPressDownGroup;
    public Directions Group { get; private set; }
    private List<Track> tracks = new();
    private Queue<NoteObject> notes = new();
    private List<NoteObject> autoPlayHittingList = new();
    private GameObject TrackPrefab 
    {
        get 
        {
            if (Application.isEditor)
            {
                return editModeFramePrefab;
            }
            try
            {
                return LevelManager.Instance.LoadedLevel.inPlaying.FramePrefab;
            }
            catch (NullReferenceException)
            {
                Debug.LogWarning("Failed to fetch prefab in levelplayer, returning default prefab");
                return editModeFramePrefab;
            }
        }
    }

    private void Awake()
    {
        this.OnPressDownGroup = null;
        this.CreatePlaceHolder();
    
        switch (this.tag) 
        {
            case "LeftGroup":
                this.Group = Directions.LEFT;
                break;
            case "RightGroup":
                this.Group = Directions.RIGHT;
                break;
            default:
                Debug.LogError($"Unknown track group tag: {this.tag}");
                break;
        }
    }

    public void CreatePlaceHolder()
    {
        while (this.transform.childCount > 0)
        {
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }
        this.tracks.Clear();

        var trackPrefab = this.TrackPrefab;

        while (tracks.Count < groupSize)
        {
            var newTrack = Instantiate(trackPrefab, this.transform).GetComponent<Track>();
            newTrack.parent = this;
            tracks.Add(newTrack);
        }

        var targetLocalWidth = 1.0f / this.groupSize;
        var placer = new GameObject();
        placer.transform.parent = this.transform;
        var step = new Vector3(2.0f / this.groupSize, 0.0f, 0.0f);
        placer.transform.localPosition = step / 2 + Vector3.left;
        for (int i = 0; i < this.groupSize; i++)
        {
            tracks[i].transform.localPosition = placer.transform.localPosition;
            tracks[i].transform.localScale = new(targetLocalWidth, 1.0f, 1.0f);
            placer.transform.localPosition += step;
        }

        DestroyImmediate(placer);
    }

    public void PutNoteObject(int index, NoteObject noteObject)
    {
        noteObject.track = this.tracks[index];
        noteObject.transform.parent = null;
        noteObject.transform.localScale = this.tracks[index].transform.lossyScale;
        noteObject.transform.parent = this.tracks[index].transform;
        noteObject.transform.SetLocalPositionAndRotation(
            LevelPlayer.Instance.PlayingLevel.meta.NoteAppearPosition,
            Quaternion.Euler(0, 0, 0)
            );

        this.notes.Enqueue(noteObject);
    }

    private void Update()
    {
        this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, Vector3.zero, this.lerpSpeed * Time.deltaTime);
        if (InputManager.GetHitCount(this.Group) != 0)
        {
            this.OnPressDownGroup?.Invoke();
            this.transform.localPosition = offsetOnPressedDown;
        }

        if (this.notes.Count == 0)
        {
            return;
        }

        var head = this.notes.Peek();

        #region AUTOPLAY
        if (LevelPlayer.Instance.AutoPlay)
        {
            if (head.note.StrikeTime.EpsilonGreater(LevelPlayer.Instance.Time()))
            {
                return;
            }

            while (this.notes.Count != 0 && this.notes.Peek().note.StrikeTime.Equals(head.note.StrikeTime))
            {
                autoPlayHittingList.Add(this.notes.Dequeue());
            }
            foreach (var note in autoPlayHittingList)
            {
                note.track.Spark();
                note.Detach(Score.CUTE, Latency.OK);
                SoundManager.Instance.PlayClip("strike", Channel.Effect);
            }
            this.ReportResult(Score.CUTE, autoPlayHittingList.Count, Latency.OK);
            autoPlayHittingList.Clear();
            return;
        }
        #endregion

        if (head.IsMissed)
        {
            int missCount = 0;
            // MISS
            while (this.notes.Count != 0 && this.notes.Peek().note.StrikeTime.Equals(head.note.StrikeTime))
            {
                var n = this.notes.Dequeue();
                n.Detach(Score.MISS, Latency.LATE);
                n.Die();
                missCount++;
            }
            this.ReportResult(Score.MISS, missCount, Latency.LATE);
            return;
        }

        if (!head.Triggered())
        {
            return;
        }

        var list = new List<NoteObject>();

        while (this.notes.Count != 0 &&
                /*
                this is same as the change in LevelPlayer, if wanna use 
                the old one, simply change it to a stupid "==" comparison
                */
                Mathf.Approximately(
                    this.notes.Peek().note.StrikeTime,
                    head.note.StrikeTime
                ))
        {
            list.Add(this.notes.Dequeue());
        }

        var recording = new ForceRecord(list, this);
        StartCoroutine(recording.RecordingCoroutine());
    }

    internal void ReportResult(Score score, int count, Latency latency)
    {
        ScoreCounter.Counter.Report(score, count, latency);
    }
}


#region RECORD
internal class ForceRecord
{
    private List<NoteObject> notes;

    private TrackGroup trackGroup;

    internal ForceRecord(List<NoteObject> notes, TrackGroup parent)
    {
        this.notes = notes;
        this.trackGroup = parent;
    }

    private void ReportCallback(Score score, Latency latency, int count = 1) 
    {
        this.trackGroup.ReportResult(score, count, latency);
    }

    internal IEnumerator RecordingCoroutine()
    {
        var score = Score.MISS;
        var first = this.notes[0];
        int delegatedReportCount = 0;

        if (Judging.IsAwful(LevelPlayer.Instance.Time(), first.note))
        {
            foreach (var note in notes)
            {
                if (note.Detach(Score.AWFUL, Latency.EARLY)) 
                {
                    delegatedReportCount++;
                }

                note.Die();
            }

            if (delegatedReportCount > 0) 
            {
                this.trackGroup.ReportResult(Score.AWFUL, delegatedReportCount, Latency.EARLY);
            }

            yield break;
        }

        foreach (var note in notes)
        {
            note.underRecording = true;
            SoundManager.Instance.PlayClip("strike", Channel.Effect);
            note.track.Spark();
        
            if (note is HitNoteObject)
            {
                note.Die();
            }
        }

        var totalHit = 0;

        Latency latency = Latency.UNJUDGED;

        for (var frame = 0; frame < BuiltInSettings.recordingFrames; ++frame)
        {
            totalHit += InputManager.GetHitCount(this.trackGroup.Group);
            var judgedScore = Judging.JudgeAccuracy(LevelPlayer.Instance.Time(), first.note, out Latency latencyThisFrame);
            
            if (judgedScore > score)
            {
                score = judgedScore;
            }

            if (latency != Latency.OK)
            {
                latency = latencyThisFrame;
            }

            yield return null;
        }

        var force = Judging.JudgeForce(totalHit, this.notes.Count);

        if (score.Equals(Score.CUTE))
        {
            score = force switch
            {
                ForceScore.TOOHEAVY => Score.TOOHEAVY,
                ForceScore.TOOWEAK => Score.TOOWEAK,
                ForceScore.ACCURATE => Score.CUTE,
                _ => Score.CUTE
            };
        }

        foreach (var note in notes)
        {
            note.underRecording = false;
            if (note.Detach(score, latency, ReportCallback)) 
            {
                delegatedReportCount++;
            }
        }

        this.trackGroup.ReportResult(score, delegatedReportCount, latency);

        yield break;
    }
}

#endregion