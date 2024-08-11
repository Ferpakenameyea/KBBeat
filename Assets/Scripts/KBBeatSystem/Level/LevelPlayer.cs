using KBBeat;
using KBBeat.Common;
using KBBeat.Core;
using KBBeat.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace KBBeat.Core
{
    public class LevelPlayer : MonoBehaviour
    {
        public static LevelPlayer Instance { get; private set; } = null;
        public Level PlayingLevel { get; private set; } = null;
        public Vector3 NoteMoveSpeed { get; private set; }
        public bool IsPlaying { get; private set; } = false;
        private Queue<InPlayingEnvironment.Note> leftNotes = new();
        private Queue<InPlayingEnvironment.Note> rightNotes = new();
        private TrackGroup leftGroup;
        private TrackGroup rightGroup;
        private Timer timer = null;
        private bool signal = false;

        #region ForSettingsPreview

        [SerializeField] GameObject defaultHitEffect;
        private ObjectPool<GameObject> DefaultHitEffectPool { get; set; }

        #endregion

        private ObjectPool<GameObject> CustomizedHitEffectPool { get; set; }

        public ObjectPool<HitNoteObject> HitNotePool { get; private set; } = null;
        public ObjectPool<HoldNoteObject> HoldNotePool { get; private set; } = null; 
        public ObjectPool<GameObject> HitEffectPool 
        {
            get => CustomizedHitEffectPool != null ? CustomizedHitEffectPool : DefaultHitEffectPool;
            set => CustomizedHitEffectPool = value;
        }
        private Coroutine noteFiringCoroutine = null;
        private Coroutine musicPlayingCoroutine = null;
        private Coroutine signalCoroutine = null;
        private PlayableDirector director = null;
        [SerializeField] private bool autoPlay = false;
        public bool AutoPlay { get => autoPlay; set => autoPlay = value; }
        private bool signaledDirectorPlaying = false;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            this.transform.parent = null;
            DontDestroyOnLoad(gameObject);

            this.DefaultHitEffectPool = new(() => {
                    var o = Instantiate(defaultHitEffect);
                    o.transform.SetParent(this.transform);
                    return o;
                },
                (e) => e.SetActive(true),
                (e) => {
                    e.SetActive(false);
                    e.transform.SetParent(this.transform);
                },
                (e) => Destroy(e));

            BuiltInSettings.OnMoveTimeChange += (value) =>
            {
                if (PlayingLevel != null)
                {
                    this.NoteMoveSpeed = -this.PlayingLevel.meta.NoteAppearPosition / value;
                }
            };
            this.timer = this.AddComponent<Timer>();
            BuiltInSettings.OnGamePause += () =>
            {
                if (
                    this.director != null &&
                    this.signaledDirectorPlaying)
                {

                    this.director.Pause();
                }
            };

            BuiltInSettings.OnGameResume += () =>
            {
                if (
                    this.director != null &&
                    this.signaledDirectorPlaying)
                {

                    this.director.Resume();
                }
            };
        }
        public void PlayLevel(Level level)
        {
            // These are the prepare section of the level
            // It loads all the notes and prepare for the run loop
            if (level == null)
            {
                Debug.LogError("Cannot play null level!");
                return;
            }
            this.signaledDirectorPlaying = false;
            if (this.IsPlaying)
            {
                Stop();
            }
            PlayingLevel = level;
            ScoreCounter.Counter.Reset();

            this.NoteMoveSpeed = -level.meta.NoteAppearPosition / BuiltInSettings.MoveTime;
            try
            {
                this.leftGroup = GameObject.FindGameObjectWithTag("LeftGroup").GetComponent<TrackGroup>();
                this.rightGroup = GameObject.FindGameObjectWithTag("RightGroup").GetComponent<TrackGroup>();
            }
            catch (NullReferenceException)
            {
                Debug.LogError("Failed when finding track group object in scene!");                
            }

            foreach (var note in PlayingLevel.inPlaying.LeftNotes)
            {
                this.leftNotes.Enqueue(note);
            }

            foreach (var note in PlayingLevel.inPlaying.RightNotes)
            {
                this.rightNotes.Enqueue(note);
            }

            // Register a dynamic object pool to get notePool

            this.CustomizedHitEffectPool?.Dispose();

            this.CustomizedHitEffectPool = new ObjectPool<GameObject>(
                () => {
                    var o = Instantiate(level.inPlaying.HitEffectPrefab);
                    o.transform.SetParent(this.transform);
                    return o;
                },
                (obj) => {
                    obj.SetActive(true);
                },
                (obj) => {
                    obj.SetActive(false);
                    obj.transform.SetParent(this.transform);
                },
                (obj) => Destroy(obj),
                defaultCapacity: 30
            );

            this.HitNotePool?.Dispose();
            this.HoldNotePool?.Dispose();

            this.HitNotePool = new ObjectPool<HitNoteObject>(
                () => {
                    if (!Instantiate(this.PlayingLevel.inPlaying.HitNotePrefab).TryGetComponent<HitNoteObject>(out var r)) {
                        Debug.LogError("Error when instantiating hit note, component is null");
                        return null;
                    }
                    r.gameObject.transform.SetParent(this.transform);
                    return r;
                },
                (hit) => {
                    hit.gameObject.SetActive(true);
                },
                (hit) => {
                    hit.gameObject.SetActive(false);
                    hit.transform.SetParent(this.transform);
                },
                (hit) => 
                {
                    if (!hit.IsDestroyed())
                    {
                        Destroy(hit.gameObject);
                    }
                },
                defaultCapacity: 20);
                

            this.HoldNotePool = new ObjectPool<HoldNoteObject>(
                () => {
                    if (!Instantiate(this.PlayingLevel.inPlaying.HoldNotePrefab).TryGetComponent<HoldNoteObject>(out var r)) {
                        Debug.LogError("Error when instantiating hold note, component is null");
                        return null;
                    }
                    r.gameObject.transform.SetParent(this.transform);
                    return r;
                },
                (hold) => hold.gameObject.SetActive(true),
                (hold) => {
                    hold.gameObject.SetActive(false);
                    hold.transform.SetParent(this.transform);
                },
                (hold) => 
                {
                    if (!hold.IsDestroyed())
                    {
                        Destroy(hold.gameObject);
                    }
                },
                defaultCapacity: 20);

            this.director = null;

            try
            {
                director = GameObject.FindWithTag("Director").GetComponent<PlayableDirector>();
                Debug.Log($"Found director {director}");
            }
            catch (Exception)
            {
                Debug.LogWarning("Director not found. Nothing will be played");
                director = null;
            }

            // load
            director.Play();
            director.Pause();

            RunLevel();
        }
        
        public void Stop()
        {
            try
            {
                StopCoroutine(this.musicPlayingCoroutine);
                StopCoroutine(this.noteFiringCoroutine);
                StopCoroutine(this.signalCoroutine);
            }
            catch (Exception) { }

            this.leftNotes.Clear();
            this.rightNotes.Clear();

            if (this.director != null && this.director.isActiveAndEnabled)
            {
                this.director.Stop();
            }

            this.IsPlaying = false;

            this.HitNotePool?.Dispose();
            this.HoldNotePool?.Dispose();
            this.CustomizedHitEffectPool?.Dispose();

            this.HitNotePool = null;
            this.HoldNotePool = null;
            this.CustomizedHitEffectPool = null;

            this.signaledDirectorPlaying = false;
        }

        private void RunLevel()
        {
            this.IsPlaying = true;
            var firstAppear = Mathf.Min(
                    this.leftNotes.Count > 0 ? this.leftNotes.Peek().StrikeTime - BuiltInSettings.MoveTime : float.PositiveInfinity,
                    this.rightNotes.Count > 0 ? this.rightNotes.Peek().StrikeTime - BuiltInSettings.MoveTime : float.PositiveInfinity
                );

            IEnumerator DelayedAnimPlaying(float expectedDelay = 0f)
            {
                if (expectedDelay != 0f)
                {
                    yield return new WaitForSeconds(expectedDelay);
                }

                var delay = firstAppear < 0 ? -firstAppear : 0f;
                yield return new WaitForSeconds(delay);
                if (director != null)
                {
                    director.Play();
                    this.signaledDirectorPlaying = true;
                }

                yield break;
            }

            IEnumerator DelayedNoteFiring(float expectedDelay = 0f)
            {
                if (expectedDelay != 0f)
                {
                    yield return new WaitForSeconds(expectedDelay);
                }
                Debug.Log($"Setting timer to {timer.Time}");
                yield return null;
                // this yield return null will leave the thread to complete
                // time wasting tasks, thus they won't affect the accuracy of note
                // firing

                timer.ForceSet(firstAppear < 0 ? firstAppear : 0);
                while (this.leftNotes.Count > 0 || this.rightNotes.Count > 0)
                {
                    TryDequeue(this.leftNotes, this.leftGroup);
                    TryDequeue(this.rightNotes, this.rightGroup);
                    yield return null;
                }

                this.IsPlaying = false;
                yield break;
            }

            IEnumerator WaitForSignal()
            {
                yield return new WaitUntil(() => this.signal);
                this.signal = false;
                var delay = BuiltInSettings.systemOffsetSeconds + BuiltInSettings.CustomOffsetSeconds;
                Debug.Log($"Running with delay: {delay} seconds");
                this.musicPlayingCoroutine = StartCoroutine(DelayedAnimPlaying(delay < 0f ? -delay : 0f));
                this.noteFiringCoroutine = StartCoroutine(DelayedNoteFiring(delay > 0f ? delay : 0f));
                yield break;
            }
            this.signalCoroutine = StartCoroutine(WaitForSignal());
        }
        private void TryDequeue(Queue<InPlayingEnvironment.Note> queue, TrackGroup group)
        {
            if (queue.Count == 0)
            {
                return;
            }

            if (timer.Time.EpsilonGreater(queue.Peek().StrikeTime - BuiltInSettings.MoveTime))
            {
                var note = queue.Dequeue();
                Debug.Log($"Generating note {note} at time: {timer.Time}; Absolute time: {note.StrikeTime - BuiltInSettings.MoveTime}");
                FireNote(group, note);

                while (queue.TryPeek(out var obj))
                {
                    // I changed this from != to Mathf.Approximately, trying to fix
                    // a protential bug
                    if (!Mathf.Approximately(obj.StrikeTime, note.StrikeTime))
                    {
                        break;
                    }
                    var followNote = queue.Dequeue();
                    FireNote(group, followNote);
                }
            }
        }

        private void FireNote(TrackGroup group, InPlayingEnvironment.Note note)
        {
            if (note is InPlayingEnvironment.HoldNote)
            {
                var noteObject = this.HoldNotePool.Get();
                noteObject.Fire(note, group);
            }
            else
            {
                var noteObject = this.HitNotePool.Get();
                noteObject.Fire(note, group);
            }
        }

        public float Time()
        {
            return this.timer.Time;
        }
        public void Signal()
        {
            Instance.signal = true;
        }
        public GameEndUIShowBundle Export()
        {
            return new GameEndUIShowBundle(
                ScoreCounter.Counter.Scores,
                ScoreCounter.Counter.GetRank(),
                ScoreCounter.Counter.Accuracy
            );
        }
        public void SwitchGameEnd()
        {
            GameEndUI.showTarget = this.Export();
            SceneManager.LoadSceneAsync("End");
        }
    }

    [Serializable]
    public struct Scores
    {
        public static readonly Scores zero = new();
        public int miss, awful, fine, cute, tooHeavy, tooWeak;
        public override string ToString()
        {
            return
                $"Cute: {this.cute} Note(s)\n" +
                $"Fine: {this.fine} Note(s)\n" +
                $"Awful: {this.awful} Note(s)\n" +
                $"Miss: {this.miss} Note(s)\n" +
                $"TooHeavy: {this.tooHeavy} Note(s)\n" +
                $"TooWeak: {this.tooWeak} Note(s)";
        }
        public int TotalNotes { get => miss + awful + fine + cute + tooHeavy + tooWeak; }
        public int TotalScore
        {
            get =>
                this.fine * Score.FINE.score +
                this.cute * Score.CUTE.score +
                this.tooHeavy * Score.TOOHEAVY.score +
                this.tooWeak * Score.TOOWEAK.score;
        }

        public float Accuracy()
        {
            return (
                this.cute * Score.CUTE.accuracy +
                this.fine * Score.FINE.accuracy +
                this.tooHeavy * Score.TOOHEAVY.accuracy +
                this.tooWeak * Score.TOOWEAK.accuracy
                ) / this.TotalNotes;
        }

    }

    public class ScoreCounter
    {
        private Scores scores = new();
        public Scores Scores { get => scores; }

        public static readonly ScoreCounter Counter = new();
        public static event Action OnCute;
        public static event Action OnFine;
        public static event Action OnMiss;
        public static event Action OnFineLate;
        public static event Action OnFineEarly;
        public static event Action OnHeavy;
        public static event Action OnWeak;
        public int Combo { get; private set; } = 0;
        public float Accuracy { get => scores.Accuracy(); }
        public int TotalNotes { get => scores.TotalNotes; }
        public int TotalScore { get => scores.TotalScore; }
        public bool AllCute { get; private set; } = true;
        public bool FullCombo { get; private set; } = true;

        public event Action<Scores, int> OnScoresChanged;

        public void Reset()
        {
            this.scores = new();
            this.Combo = 0;
            this.AllCute = true;
            this.FullCombo = true;
        }
        public void Report(Score scoreLevel, int count, Latency latency)
        {
            if (count == 0)
            {
                return;
            }
            switch (scoreLevel.name)
            {
                case "MISS":
                    this.scores.miss += count;
                    Combo = 0;
                    OnMiss?.Invoke();
                    this.FullCombo = false;
                    this.AllCute = false;
                    break;
                case "AWFUL":
                    this.scores.awful += count;
                    Combo = 0;
                    OnMiss?.Invoke();
                    this.FullCombo = false;
                    this.AllCute = false;
                    break;
                case "FINE":
                    this.scores.fine += count;
                    Combo += count;
                    OnFine?.Invoke();
                    this.AllCute = false;
                    break;
                case "CUTE":
                    this.scores.cute += count;
                    Combo += count;
                    OnCute?.Invoke();
                    break;
                case "TOOHEAVY":
                    this.scores.tooHeavy += count;
                    Combo += count;
                    OnHeavy?.Invoke();
                    this.AllCute = false;
                    break;
                case "TOOWEAK":
                    this.scores.tooWeak += count;
                    Combo += count;
                    OnWeak?.Invoke();
                    this.AllCute = false;
                    break;
            }


            switch(latency)
            {
                case Latency.EARLY:
                    OnFineEarly?.Invoke();
                    break;
                case Latency.LATE:
                    OnFineLate?.Invoke();
                    break;
                default:
                    break;
            }

            this.OnScoresChanged?.Invoke(this.scores, this.Combo);
        }
        public override string ToString()
        {
            return $"[ScoreCounter]\n" +
                this.scores.ToString() +
                $"\nCombo: {this.Combo} Note(s)\n";
        }
        public Rank GetRank() => RankConverter.ToRank(this.Accuracy, this.AllCute);
    }
    [Serializable]
    public enum Rank
    {
        S_plus,
        S,
        A,
        B,
        C,
        F
    };
}

public static class RankConverter
{
    public static Rank ToRank(float accuracy, bool allCute)
    {
        if (allCute)
        {
            return Rank.S_plus;
        }
        if (accuracy > 0.96f)
        {
            return Rank.S;
        }
        if (accuracy > 0.9f)
        {
            return Rank.A;
        }
        if (accuracy > 0.8f)
        {
            return Rank.B;
        }
        if (accuracy > 0.6f)
        {
            return Rank.C;
        }
        return Rank.F;
    }
}

public class Timer : MonoBehaviour
{
    public float Time { get; private set; } = 0.0f;
    public bool Paused { get; internal set; } = false;

    private void Update()
    {
        if (Paused)
        {
            return;
        }
        this.Time += UnityEngine.Time.deltaTime;
    }

    public void ResetTimer()
    {
        this.Time = 0.0f;
    }

    public void ForceSet(float time)
    {
        this.Time = time;
    }
}

public static class KBbeatFloatExtension
{
    public static bool EpsilonGreater(this float value, float other)
    {
        return value > other - BuiltInSettings.epsilon;
    }
}