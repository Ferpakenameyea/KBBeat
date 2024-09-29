using System;
using System.Collections;
using System.Diagnostics;
using KBbeat;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class HoldNoteObject : NoteObject
{
    [SerializeField] private GameObject start;
    [SerializeField] private GameObject end;
    [SerializeField] private GameObject body;
    private ReportCallback reportCallback;
    private Score prejudgedScore;
    private Latency prejudgedLatency;
    private Vector3 scale;
    private bool freezeHead = false;

    public override NoteType noteType => NoteType.HOLD;
    public InPlayingEnvironment.HoldNote HoldNote { get => base.note as InPlayingEnvironment.HoldNote; }

    private void OnEnable() 
    {
        scale = body.transform.localScale;
        this.freezeHead = false;
    }

    private new void Update() 
    {
        if (!freezeHead) 
        {
            base.Update();
        }

        this.end.transform.localPosition = 
            - LevelPlayer.Instance.NoteMoveSpeed * (TimeToHit + HoldNote.Length) 
            - this.transform.localPosition;



        var startPos = start.transform.localPosition;
        var endPos = end.transform.localPosition;

        var newScale = scale;
        newScale.z = (endPos.z - startPos.z);

        body.transform.localPosition = (startPos + endPos) / 2;
        body.transform.localScale = newScale;
    }

    public override bool Detach(Score score, Latency latency, ReportCallback reportCallback)
    {
        if (score.name.Equals("MISS")) 
        {
            return false;
        }

        this.reportCallback = reportCallback;
        this.prejudgedScore = score;
        this.prejudgedLatency = latency;
        StartCoroutine(TraceCoroutine());
        return false;
    }

    private IEnumerator TraceCoroutine()
    {
        float viewEnd = this.HoldNote.StrikeTime + this.HoldNote.Length;
        float actualEnd = viewEnd - BuiltInSettings.holdEndOffset;
        freezeHead = true;
        float timer = 0f;

        while (LevelPlayer.Instance.Time() < actualEnd) 
        {
            timer += Time.deltaTime;
            if (timer >= 0.1f) 
            {
                timer -= 0.1f;
                this.track.Spark();
            }

            int c = InputManager.GetHoldCount(this.Direction);
            
            if (c == 0) 
            {
                this.reportCallback?.Invoke(Score.MISS, Latency.LATE, 1);

                this.freezeHead = false;
                yield return new WaitUntil(() => LevelPlayer.Instance.Time() >= viewEnd);
                LevelPlayer.Instance.HoldNotePool.Release(this);
                yield break;
            }

            yield return null;
        }

        switch (this.prejudgedScore.name)
        {
            case "TOOWEAK":
                this.reportCallback?.Invoke(Score.TOOWEAK, this.prejudgedLatency, 1);
                break;
            case "TOOHEAVY":
                this.reportCallback?.Invoke(Score.TOOHEAVY, this.prejudgedLatency, 1);
                break;
            case "FINE":
                this.reportCallback?.Invoke(Score.FINE, this.prejudgedLatency, 1);
                break;
            default:
                this.reportCallback?.Invoke(Score.CUTE, this.prejudgedLatency, 1);
                break;
        }

        yield return new WaitUntil(() => LevelPlayer.Instance.Time() >= viewEnd);
        this.freezeHead = false;
        LevelPlayer.Instance.HoldNotePool.Release(this);
    }

    public override void Die()
    {
        LevelPlayer.Instance.HoldNotePool.Release(this);
    }
}