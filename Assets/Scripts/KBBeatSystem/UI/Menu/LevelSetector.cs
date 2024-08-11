using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using KBBeat.Core;
using KBBeat.Common;
using KBBeat.Audio;

namespace KBBeat
{
    internal class LevelSelector : MonoBehaviour
    {
        public bool Switching { get => this.ball.Switching; }
        [SerializeField] private Texture defaultLevelIcon;
        [SerializeField] private List<LevelShowerInfo> levels;
        [SerializeField] private MenuBall ball;

        [SerializeField] private TextMeshProUGUI musicTitleBox;
        [SerializeField] private TextMeshProUGUI detailBox;
        [SerializeField] private Animator UIAnimator;
        [SerializeField] private HighScoreShower highScoreShower;

        [SerializeField] private float muteTime;
        private event Action OnLoadComplete;
        private AudioPlayer previewPlayer;

        public int SelectedLevelIndex { get; private set; } = 0;
        public string SelectedLevelName { get => this.levels[SelectedLevelIndex].Meta.AssetBundleName.Split('.')[1]; }
        public bool Ready { get; private set; } = false;

        private void Start()
        {
            this.OnLoadComplete += () =>
            {
                if (this.levels.Count == 0)
                {
                    this.Ready = false;
                    Debug.LogWarning($"No valid level found!");
                    return;
                }

                this.Ready = true;
                this.ball.SetCenterTexture(this.levels.Count > 0 ?
                    this.levels[0].Art : defaultLevelIcon
                    );
                this.Put(this.levels[0]);
                this.highScoreShower.Change(ScoreRecorder.GetRecord(this.SelectedLevelName));
                if (this.levels[0].Preview != null)
                {
                    this.previewPlayer = SoundManager.Instance.PlayClip(this.levels[0].Preview, 1.0f, Channel.Music, repeat: true);
                }
            };
            LoadAllLevelsMetaAsync();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                this.SwitchLeft();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                this.SwitchRight();
            }
        }
        private void LoadAllLevelsMetaAsync()
        {
            StartCoroutine(this.LoadAllLevelsMetaAsyncCoroutine());
        }
        private IEnumerator LoadAllLevelsMetaAsyncCoroutine()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            this.levels.Clear();
            AssetBundle manifestBundle = null;
            yield return new ABLoader(Path.Combine(Application.streamingAssetsPath, "StreamingAssets"))
                .LoadAsync((bundle) => manifestBundle = bundle);
            var manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            var list = manifest.GetAllAssetBundles();
            manifestBundle.Unload(true);

            var success = 0;
            var fail = 0;

            foreach (var bundleName in list)
            {
                if (bundleName.EndsWith("scene"))
                {
                    continue;
                }
                RuntimePlatform platform = Application.platform;
                AssetBundle levelBundle = null;
                yield return new ABLoader(Path.Combine(Application.streamingAssetsPath, bundleName))
                    .LoadAsync((bundle) =>
                    {
                        levelBundle = bundle;
                    });
                try
                {
                    var icon = levelBundle.LoadAsset<Texture>("art.png");
                    this.levels.Add(new(
                            levelBundle
                                .LoadAsset<TextAsset>("meta.json")
                                .FromJson<Meta>(),
                            icon == null ? this.defaultLevelIcon : icon,
                            levelBundle
                                .LoadAsset<AudioClip>("preview.ogg")
                        ));
                    success++;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to load level {bundleName}, exception message: {e}");
                    fail++;
                }
                finally
                {
                    levelBundle.Unload(false);
                }
            }
            Debug.Log($"Loaded levels. Success: {success}, fail: {fail}, total: {success + fail}");
            this.OnLoadComplete?.Invoke();
        }
        public void SwitchLeft()
        {
            if (this.Switching || !this.Ready)
            {
                return;
            }
            this.SelectedLevelIndex--;
            if (this.SelectedLevelIndex < 0)
            {
                this.SelectedLevelIndex = this.levels.Count - 1;
            }
            this.Put(this.levels[SelectedLevelIndex]);
            this.ball.SwitchLeft(this.levels[SelectedLevelIndex].Art);
            this.highScoreShower.Change(ScoreRecorder.GetRecord(this.SelectedLevelName));
            if (this.previewPlayer != null)
            {
                this.previewPlayer.Stop();
                this.previewPlayer = null;
            }
            if (this.levels[SelectedLevelIndex] != null)
            {
                this.previewPlayer = SoundManager.Instance.PlayClip(this.levels[SelectedLevelIndex].Preview, 1.0f, Channel.Music, repeat: true);
            }
            SoundManager.Instance.PlayClip("switch", Channel.Effect);
        }
        public void SwitchRight()
        {
            if (this.Switching || !this.Ready)
            {
                return;
            }
            this.SelectedLevelIndex++;
            this.SelectedLevelIndex %= this.levels.Count;
            this.Put(this.levels[SelectedLevelIndex]);
            this.ball.SwitchRight(this.levels[SelectedLevelIndex].Art);
            this.highScoreShower.Change(ScoreRecorder.GetRecord(this.SelectedLevelName));
            if (this.previewPlayer != null)
            {
                this.previewPlayer.Stop();
                this.previewPlayer = null;
            }
            if (this.levels[SelectedLevelIndex] != null)
            {
                this.previewPlayer = SoundManager.Instance.PlayClip(this.levels[SelectedLevelIndex].Preview, 1.0f, Channel.Music, repeat: true);
            }
            SoundManager.Instance.PlayClip("switch", Channel.Effect);
        }
        private void Put(LevelShowerInfo level)
        {
            string.Format(
                "<line-height=10>\n" +
                "<size=95>{0}</size>\n" +
                "<size=60>\n" +
                "<line-height=53>\n" +
                "{1}\n" +
                "{2}\n" +
                "</size>\n" +
                "</line-height>" +
                "</line-height>\n",
                level.Meta.Name,
                string.Join(", ", level.Meta.Composers),
                string.Join(", ", level.Meta.LevelAuthors)
                )
                .PutTo(this.musicTitleBox);
            level.Meta.Description.PutTo(this.detailBox);
        }
        internal void SelectAndRun()
        {
            SoundManager.Instance.PlayClip("start", Channel.Effect);
            StartCoroutine(this.SwitchCoroutine());
        }
        private IEnumerator SwitchCoroutine()
        {
            bool ready = false;
            UIAnimator.SetTrigger("Load");
            if (this.previewPlayer != null)
            {
                IEnumerator Mute()
                {
                    var timer = 0f;
                    var speed = this.previewPlayer.Volume / this.muteTime;
                    while (timer < this.muteTime)
                    {
                        timer += Time.deltaTime;
                        this.previewPlayer.Volume -= Time.deltaTime * speed;
                        yield return null;
                    }
                    yield break;
                }
                StartCoroutine(Mute());
            }

            LevelManager.Instance.OnLoadLevelSuccess += (_) => ready = true;
            LevelManager.Instance.LoadLevelAsync(this.SelectedLevelName, Encoding.UTF8);
            yield return new WaitUntil(() => ready);
            Debug.Log("SetComplete");
            UIAnimator.SetTrigger("LoadComplete");
            yield return new WaitForSeconds(2f);
            var req = SceneManager.LoadSceneAsync("ingame");

            req.completed += (_) =>
            {
                if (this.previewPlayer != null)
                {
                    this.previewPlayer.Volume = 0f;
                    this.previewPlayer.Stop();
                }
                LevelPlayer.Instance.PlayLevel(LevelManager.Instance.LoadedLevel);
            };
            yield break;
        }

        [Serializable]
        internal class LevelShowerInfo
        {
            public Meta Meta { get; }
            public Texture Art { get; }
            public AudioClip Preview { get; }

            public LevelShowerInfo(Meta meta, Texture art, AudioClip preview)
            {
                this.Meta = meta;
                this.Art = art;
                this.Preview = preview;
            }
        }
    }

    public static class StringShowing
    {
        public static void PutTo(this string value, TextMeshProUGUI target)
        {
            target.text = value;
        }
    }
}

public class ABLoader
{
    private string url;

    public ABLoader(string url)
    {
        this.url = url;
    }

    public IEnumerator LoadAsync(Action<AssetBundle> onComplete)
    {
        AssetBundle bundle = null;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                var abRequest = AssetBundle.LoadFromFileAsync(url);
                yield return abRequest;
                bundle = abRequest.assetBundle;
                break;
            case RuntimePlatform.Android:
                var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);
                yield return webRequest.SendWebRequest();
                bundle = DownloadHandlerAssetBundle.GetContent(webRequest);
                break;
            default:
                throw new UnityException($"platform {Application.platform} not supported");
        }
        onComplete?.Invoke(bundle);
        yield break;
    }
}