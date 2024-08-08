using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/**
 * <summary>A level manager used to load and unload level</summary>
 */
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; } = null;
    public Level LoadedLevel { get; private set; } = null;

    public event Action<string> OnLoadLevelStart;
    public event Action<string> OnLoadLevelSuccess;
    public event Action<string> OnLoadLevelFail;
    public event Action<float> OnLoadLevelProgressChange;
    [SerializeField] private GameObject defaultHoldPrefab;
    [SerializeField] private GameObject defaultFrameObject;
    [SerializeField] private Texture defaultArt;
    [SerializeField] private GameObject defaultHitPrefab;
    [SerializeField] private GameObject defaultHitEffectPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            this.transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TryUnload()
    {
        if (this.LoadedLevel != null)
        {
            if (this.LoadedLevel.inPlaying.MusicClip != null)
            {
                this.LoadedLevel.inPlaying.MusicClip.UnloadAudioData();
                var info = $"unloaded music file {this.LoadedLevel.inPlaying.MusicClip.name}";
                Debug.Log(info);
            }
            this.LoadedLevel = null;
        }
    }

    public void LoadLevelAsync(string levelName, Encoding encoding)
    {
        this.TryUnload();
        StartCoroutine(LoadLevelCoroutine(levelName, encoding));
    }

    private IEnumerator LoadLevelCoroutine(string levelName, Encoding encoding)
    {
        this.OnLoadLevelStart?.Invoke(levelName);
        this.Unload();
        InPlayingEnvironment inPlaying = null;
        Meta meta = null;
        this.OnLoadLevelProgressChange?.Invoke(0f);

        var newtonsoftJsonSettings = new JsonSerializerSettings();
        newtonsoftJsonSettings.Converters.Add(new InPlayingEnvironment.Note.NoteJsonConverter());
    
        List<BunchAssetLoadingTask.LoadAction> tasks = new() {
            new("inPlaying.json", (res) =>
                inPlaying = JsonConvert.DeserializeObject<InPlayingEnvironment>(
                    encoding.GetString((res as TextAsset).bytes),
                    newtonsoftJsonSettings),

                (task) => task.ReportFatalFailure("inPlaying.json not found!")),

            new("mus.ogg", (clip) => {
                inPlaying.MusicClip = clip as AudioClip;
                inPlaying.MusicClip.LoadAudioData();
                this.OnLoadLevelProgressChange?.Invoke(0.2f);
                },
                (task) => task.ReportFatalFailure("mus.ogg not found!")),

            new("meta.json",
                (metaJson) => meta = JsonUtility.FromJson<Meta>(encoding.GetString((metaJson as TextAsset).bytes)),
                (task) => task.ReportFatalFailure("meta.json not found!")),

            // notes loading
            new("Hold.prefab", (prefab) => {
                inPlaying.HoldNotePrefab = prefab as GameObject;
                }, 
                (_) => inPlaying.HoldNotePrefab = defaultHoldPrefab),

            new("Note.prefab", (prefab) => {
                inPlaying.HitNotePrefab = prefab as GameObject;
                this.OnLoadLevelProgressChange?.Invoke(0.5f);
                },
                (_) => inPlaying.HitNotePrefab = defaultHitPrefab),
            
            new("art.png", (png) => meta.art = png as Texture, (_) => meta.art = defaultArt),
            new("Track.prefab", (track) => inPlaying.FramePrefab = track as GameObject, (_) => inPlaying.FramePrefab = defaultFrameObject),
            new("Effect.prefab", (effect) => inPlaying.HitEffectPrefab = effect as GameObject, (_) => inPlaying.HitEffectPrefab = defaultHitEffectPrefab)
        };
        BunchAssetLoadingTask assetTask = null;
        RuntimePlatform platform = Application.platform;
        switch (platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                assetTask = new BunchAssetWindowsLoadingTask($"levels.{levelName}", tasks);
                break;
            case RuntimePlatform.Android:
                assetTask = new BunchAssetAndroidLoadingTask($"levels.{levelName}", tasks);
                break;
            default:
                throw new UnityException($"Platform not supported: {platform}");
        }

        yield return assetTask.LoadAsync();
        if (assetTask.Failed)
        {
            Debug.LogError($"Fatal error loading level: {levelName}");
            this.OnLoadLevelFail?.Invoke(levelName);
            yield break;
        }
        var sceneBundle = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, $"levels.{levelName}scene"));
        yield return sceneBundle;
        this.OnLoadLevelProgressChange?.Invoke(1f);
        inPlaying.SceneAssetBundle = sceneBundle.assetBundle;
        this.LoadedLevel = new(meta, inPlaying);
        this.OnLoadLevelSuccess?.Invoke(levelName);
        this.CleanAllSubscribers();
        yield break;
    }

    private void CleanAllSubscribers()
    {
        this.OnLoadLevelFail = null;
        this.OnLoadLevelProgressChange = null;
        this.OnLoadLevelStart = null;
        this.OnLoadLevelSuccess = null;
    }

    public void Unload()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        this.LoadedLevel = null;
    }
}

[Serializable]
public class Meta
{
    [SerializeField] private string assetBundleName;
    [SerializeField] private string name;
    [SerializeField] private string description;
    [SerializeField] private string[] levelAuthors;
    [SerializeField] private string[] composers;
    [SerializeField] private int difficulty;
    [SerializeField] private int leftTrackSize;
    [SerializeField] private int rightTrackSize;
    [SerializeField] private Vector3 noteAppearPosition;
    [SerializeField] private float bpm;
    public Texture art;

    public string AssetBundleName { get => assetBundleName; }
    public string Name { get => name; }
    public string Description { get => description; }
    public string[] LevelAuthors { get => levelAuthors; }
    public string[] Composers { get => composers; }
    public int Difficulty { get => difficulty; }
    public int LeftTrackSize { get => leftTrackSize; }
    public int RightTrackSize { get => rightTrackSize; }
    public Vector3 NoteAppearPosition { get => noteAppearPosition; }
    public float Bpm { get => bpm; }

    internal Meta(
            string assetBundleName,
            string name,
            string description,
            string[] levelAuthors,
            string[] composers,
            int difficulty,
            int leftTrackSize,
            int rightTrackSize,
            Vector3 noteAppearPosition,
            int bpm
        )
    {
        this.assetBundleName = assetBundleName;
        this.name = name;
        this.description = description;
        this.levelAuthors = levelAuthors;
        this.composers = composers;
        this.difficulty = difficulty;
        this.leftTrackSize = leftTrackSize;
        this.rightTrackSize = rightTrackSize;
        this.noteAppearPosition = noteAppearPosition;
        this.bpm = bpm;
    }

    public bool IsValidAssetBundleName()
    {
        var tokens = this.assetBundleName.Split('.');
        return tokens.Length == 2 && tokens[0].Equals("levels");
    }
}

[Serializable]
public class InPlayingEnvironment
{

    [Serializable]
    public abstract class Note
    {
        [JsonProperty("strikeTime")] public float StrikeTime { get; set; }
        [JsonProperty("trackIndex")] public int TrackIndex { get; set; }

        public override string ToString()
        {
            return $"{{StrikeTime:{this.StrikeTime}; track:{this.TrackIndex}}}";
        }

        public class NoteJsonConverter : JsonConverter<Note>
        {
            public override Note ReadJson(JsonReader reader, Type objectType, Note existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var rawObject = JObject.Load(reader);
                Note note = null;
                JToken typeToken = rawObject["type"];
                int type = 0;
                if (typeToken != null)
                {
                    type = rawObject["type"].Value<int>();
                }
                switch (type)
                {
                    case (int)NoteType.Hit:
                        note = new HitNote();
                        note.StrikeTime = rawObject["strikeTime"].Value<float>();
                        note.TrackIndex = rawObject["trackIndex"].Value<int>();
                        break;
                    case (int)NoteType.Hold:
                        note = new HoldNote();
                        note.StrikeTime = rawObject["strikeTime"].Value<float>();
                        note.TrackIndex = rawObject["trackIndex"].Value<int>();
                        (note as HoldNote).Length = rawObject["length"].Value<float>();
                        break;
                    default:
                        throw new JsonException($"Unknown note object type: {type}");
                }
                return note;
            }

            public override void WriteJson(JsonWriter writer, Note value, JsonSerializer serializer)
            {
                throw new InvalidOperationException("KBBeat game client cannot serialize note");
            }

            private enum NoteType
            {
                Hit = 0,
                Hold = 1
            }
        }
    }

    [Serializable]
    public class HitNote : Note
    {
        public override string ToString()
        {
            return $"{{[Hit Note]StrikeTime:{this.StrikeTime}; track:{this.TrackIndex}}}";
        }
    }

    [Serializable]
    public class HoldNote : Note
    {
        [JsonProperty("length")] public float Length { get; set; }

        public override string ToString()
        {
            return $"{{[Hold Note]StrikeTime:{this.StrikeTime}; track:{this.TrackIndex}; Length: {this.Length}}}";
        }
    }


    [JsonProperty("leftNotes")]
    public Note[] LeftNotes { get; set; }
    [JsonProperty("rightNotes")]
    public Note[] RightNotes { get; set; }

    public GameObject WorldPrefab { get; set; }
    public GameObject HitNotePrefab { get; set; }
    public GameObject HoldNotePrefab { get; set; }
    public GameObject FramePrefab { get; set; }
    public GameObject HitEffectPrefab { get; set; }
    public AudioClip MusicClip { get; set; }
    public AssetBundle SceneAssetBundle { get; set; }
    
    public void SwitchTo()
    {
        var oldObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var obj in oldObjects)
        {
            MonoBehaviour.Destroy(obj);
        }
        MonoBehaviour.Instantiate(this.WorldPrefab);
    }
}

public class Level
{
    public readonly Meta meta;
    public readonly InPlayingEnvironment inPlaying;

    public Level(Meta meta, InPlayingEnvironment notes)
    {
        this.meta = meta;
        this.inPlaying = notes;
    }
}

public static class TextAssetJsonExt
{
    public static T FromJson<T>(this TextAsset asset)
    {
        return JsonUtility.FromJson<T>(asset.text);
    }
}

internal abstract class BunchAssetLoadingTask
{
    protected string parentBundle;
    protected List<LoadAction> tasks = new();
    public bool Failed { get; protected set; } = false;
    protected string failureCause;
    public BunchAssetLoadingTask(string parentBundle, List<LoadAction> tasks)
    {
        this.parentBundle = parentBundle;
        this.tasks = tasks;
    }

    abstract public IEnumerator LoadAsync();

    public void ReportFatalFailure(string cause)
    {
        this.Failed = true;
        this.failureCause = cause;
    }

    internal struct LoadAction
    {
        public readonly string name;
        public readonly Action<object> loadedAction;
        public readonly Action<BunchAssetLoadingTask> onLoadFailAction;
        public LoadAction(string name, Action<object> loadedAction, Action<BunchAssetLoadingTask> onLoadFailAction = null)
        {
            this.name = name;
            this.loadedAction = loadedAction;
            this.onLoadFailAction = onLoadFailAction;
        }
    }
}

internal class BunchAssetWindowsLoadingTask : BunchAssetLoadingTask
{
    public BunchAssetWindowsLoadingTask(string parentBundle, List<LoadAction> tasks) : base(parentBundle, tasks) { }

    public override IEnumerator LoadAsync()
    {
        var bundleReq = AssetBundle.LoadFromFileAsync(Path.Combine(
            Application.streamingAssetsPath, this.parentBundle
        ));
        yield return bundleReq;
        var parent = bundleReq.assetBundle;

        foreach (var task in tasks)
        {
            var request = parent.LoadAssetAsync(task.name);
            yield return request;
            if (request.asset == null)
            {
                Debug.LogWarning($"Failed to load asset {task.name} from parent {this.parentBundle}!");
                task.onLoadFailAction?.Invoke(this);
                if (this.Failed)
                {
                    Debug.LogError($"Fatal error loading asset {task.name}! cause: {this.failureCause}");
                    break;
                }
                continue;
            }
            task.loadedAction?.Invoke(request.asset);
        }
        yield break;
    }
}

internal class BunchAssetAndroidLoadingTask : BunchAssetLoadingTask
{
    public BunchAssetAndroidLoadingTask(string parentBundle, List<LoadAction> tasks) : base(parentBundle, tasks) { }

    public override IEnumerator LoadAsync()
    {
        var bundleReq = UnityWebRequestAssetBundle.GetAssetBundle(Path.Combine(
            Application.streamingAssetsPath, this.parentBundle
        ));
        yield return bundleReq.SendWebRequest();
        if (bundleReq.result != UnityWebRequest.Result.Success)
        {
            throw new Exception($"Parent bundle {parentBundle} not found!");
        }
        var parent = DownloadHandlerAssetBundle.GetContent(bundleReq);

        foreach (var task in tasks)
        {
            var request = parent.LoadAssetAsync(task.name);
            yield return request;
            if (request.asset == null)
            {
                Debug.LogWarning($"Failed to load asset {task.name} from parent {this.parentBundle}!");
                task.onLoadFailAction?.Invoke(this);
                if (this.Failed)
                {
                    Debug.LogError($"Fatal error loading asset {task.name}! cause: {this.failureCause}");
                    break;
                }
                continue;
            }
            task.loadedAction?.Invoke(request.asset);
        }
        yield break;
    }
}